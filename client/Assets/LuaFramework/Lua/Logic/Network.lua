require "Common/define"
-- require "Common/protocal"
require "Common/functions"
-- require "protobuf/protobuf"
require "Common/bit"
require "Common/Base64"
-- require "Controller/GameShop"
-- require "Logic/SDKManager"

Event = require 'events'

-- require "Logic/ProtocolNumber"

Network = {};
local this = Network
this.isAwake = true  -- 因为要watch一些东

local login_conf
local g_is_connecting = false
local keeping_connect = false
local is_logining = false

function Network.back_to_login( call_back, is_change_account)
    -- body
    if GameHallCtrl then
        GameHallCtrl.login_reward_showed = false--切换用户要把这个设为false 不然再进大厅不弹登录奖励
    end
    login_conf.last_login = ''
    login_conf.last_token_time = 0
    UserData.ClearLoginData()
    Network.Close()

    if not is_change_account then
        SDK.logout()
    else
        SpecialUiController.ShowLoading('正在返回登录界面', 7)
        PreLoad.Stop()
        LoadSceneWithCtrl('Login', 'LoginCtrl', function ()     
            CtrlManager.ChangeCtrl('LoginCtrl')
            g_is_connecting = false
            keeping_connect = false
            if call_back then call_back() end
        end)
    end
end

function Network.Start()
    logWarn("Network.Start!!");  
    login_conf = FILE_DB.LoadFileDB('__login__')
    Event.AddListener(Protocal.Connect, this.OnConnect)
    Event.AddListener(Protocal.Message, this.OnMessage)
    Event.AddListener(Protocal.Exception, this.OnException)
    Event.AddListener(Protocal.Disconnect, this.OnDisconnect)
    Event.AddListener(Protocal.MessageFailed, this.OnMessageFail)

    coroutine.start(Network.HeartBeat)
    this.AddListener('Account', 'Account_Login_Response', function ()
        is_logining = false
        UserData.set_is_login(true)

        if SDK.is_yyb() then
            if SDK.is_channel('yybnew') then
                paymentManager.YsdkGuestLogin(function ()
                    this.YybNewCallback( paymentManager.GetYsdkLoginInfo() )
                end) 
            else
                paymentManager.YsdkGuestLogin(function ()
                    this.YybCallback( paymentManager.GetYsdkLoginInfo() )
                end) 
            end
        end
        SpecialUiController.HideLoading()
        CtrlManager.OnReconnect()   -- 每个界面重连，登陆界面没这个响应，所以不影响正常逻辑
    end, this.OnLoginFailed)

    SDK.start()
    SDK.init()
    UserData.watch_account(this, function ( val ) if not val then return end login_conf.account = val FILE_DB.PersistFileDB('__login__') end)
    UserData.watch_password(this, function ( val ) if not val then return end login_conf.password = val FILE_DB.PersistFileDB('__login__')end)
end

function Network.OnLoginFailed(data)
    if data and table.has_value({100011, 202000, 3}, data.ret.code) then
        SpecialUiController.HideLoading()
        Popup.PopupClient("\n服务器正在维护中,请稍后再试。", {{color='green', text='确 定'}}, false)
    elseif data and (data.ret.code == 100012 or data.ret.code == 100013 or data.ret.code == 100014) then
        Popup.PopupErrorWithCode("tcp_login", data.ret.code, tostring(data.player.id))
        coroutine.start(function (  )
            coroutine.wait(3)
            this.back_to_login()
        end)
    else
        this.back_to_login()
    end
end

--Socket消息--
function Network.OnSocket(key, data)
    Event.Brocast(key, data)
end

--当连接建立时--
function Network.OnConnect()
    SpecialUiController.HideLoading()
    logWarn("Game Server connected!!")
    this.CheckReLogin();
    g_is_connecting = false
end

local function HttpCall( www, call_back, exception_func, coro )
    if coro ~= nil then
        coro.www(www)
    else
        coroutine.www(www)
    end

    if www.error ~= nil then
        --logError(www.error)
        if exception_func then
            exception_func(www.error)
        end
        return
    end

    if call_back then
        local json_str = www.text
        local json_data = FILE_DB.JsonToLua(www.text)
        if json_data == nil then    -- 如果解析不出来，看是不是匹配下body块
            json_str = string.match(www.text, '<body.->(.*)</body>')
            json_data = FILE_DB.JsonToLua(json_str)
        end
        if json_data == nil or json_data.error_code and json_data.error_code ~= 0 then
            if AppConst.DebugMode then
                if json_data then
                    Popup.PopupError(json_data.error_code, json_data.error_msg)
                else
                    Popup.PopupError(0, json_str)
                end
            end
            if exception_func then
                exception_func(json_data)
            end
            return
        else
            -- log(TableToStr(json_data))
            call_back(json_data)
        end
    end
end

function Network.HttpCallWithWWW( www, coroutine, call_back, exception_func)
    coroutine.start(HttpCall, www, call_back, exception_func, coroutine)
end

-- 需要用协程
function Network.CommonHttpCall(coroutine, url, call_back, exception_func )
    local www = WWW.New(url)
    coroutine.start(HttpCall, www, call_back, exception_func, coroutine)
end

-- 需要用协程
function Network.CommonHttpPost(url, data, call_back, exception_func )
    local www = WWW.New(url, data)
    HttpCall(www, call_back, exception_func)
end

function Network.AccountHttpCall( str, call_back, exception_func )
    -- body
    local url = AppConst.AccountServer .. str
    this.CommonHttpCall(coroutine, url, call_back, exception_func)
end

function TouristLogin()
    UserData.set_account('游客')
    UserData.set_password('default')
    Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), AppConst.PlatformName, AppConst.Access_info))
end

local reconnect_count = 0
function ConnectOnce()
    if reconnect_count >= 3 then
        g_is_connecting = true
        reconnect_count = 0
        LoadingCtrl.ReGetServerlist()  -- 重连的时候重新拉一次port
        Popup.PopupClient('\n服务器断开，是否重新连接？', {{ color='green', text='取 消', func = function ()
            Application.Quit()
        end } ,
        {color='yellow', text='确 定', func = function ( ... )
            g_is_connecting = false
            SpecialUiController.ShowLoading('正在连接服务器', 10, this.OnLoginFailed)
        end}})
    else
        if not networkMgr.connected and not g_is_connecting and not UserData.get_is_login() then
            g_is_connecting = true
            reconnect_count = reconnect_count + 1
            SpecialUiController.ShowLoading('正在连接服务器', 10, this.OnLoginFailed)
            networkMgr:SendConnect()
        elseif networkMgr.connected or UserData.get_is_login() then
            reconnect_count = 0
        end
    end
end

function Network.KeepConnect()
    coroutine.start(function ()
        while true do
            if keeping_connect then
                ConnectOnce()
            end
            coroutine.wait(2)
        end
    end)
end

-- 現在改成默認都用這個登陸
function Network.LoginDirectly()
    keeping_connect = true
    ConnectOnce()
    if SDK.login() then
    else
        UserData.set_account(login_conf.account)
        UserData.set_password(login_conf.password)
        if not login_conf.account then
            TouristLogin()
            return
        end

        if login_conf.account and login_conf.last_token_time and Util.GetUnixTime() - login_conf.last_token_time < 500 then
            UserData.set_login_token(login_conf.login_token)
            if networkMgr.connected then
                this.CheckReLogin();
            end
        elseif login_conf.account and login_conf.last_login and #login_conf.last_login > 0 then
            Network.Login( login_conf.last_login )
        else
            TouristLogin()
        end
    end
end

function Network.PhoneNotBind()
    return not login_conf.bind_times or login_conf.bind_times <=0
end

local _is_loging = false
function Network.Login( www_text )
    if _is_loging then return end
    _is_loging = true

    keeping_connect = true
    login_conf.last_login = www_text  -- 不管是游客，mirror, 微信理论上都可以用这个，失败了的话，再打开登陆界面
    SpecialUiController.ShowLoading('正在连接服务器', 10, this.OnLoginFailed)
    local x,y = string.find(www_text, 'access_info=(.-)&')
    login_conf.access_info = string.sub(www_text, x + 12, y -1)

    FILE_DB.PersistFileDB('__login__')
    coroutine.start(Network.AccountHttpCall, www_text, 
        function ( json_data )
            -- body
            _is_loging = false
            UserData.set_login_token(json_data.token)
            login_conf.login_token = json_data.token
            login_conf.last_token_time = Util.GetUnixTime()

            if json_data.access_info then login_conf.access_info = json_data.access_info end
            if json_data.account_type then login_conf.platformName = json_data.account_type end
            if json_data.encrypt_password then
                login_conf.encrypt_password = Base64.DecodeString(json_data.encrypt_password)
            end
            login_conf.bind_times = tonumber(json_data.bind_times) or 0
            if login_conf.bind_times > 0 then
                UserData.set_account(login_conf.access_info)
            end
            if networkMgr.connected then
                this.CheckReLogin()
            end
        end,
        function ( json_data )
            _is_loging = false
            local btn_data = {{color='green', text='确 定', func = this.back_to_login}}
            SpecialUiController.HideLoading()
            Popup.PopupClientWithCode("login", json_data.error_code, btn_data)
        end
    ) 
end

function Network.ReconnectAccount(www_text, func)
    coroutine.start(Network.AccountHttpCall, www_text, 
        function ( json_data )
            -- body
            func()
        end,
        function ( json_data )
            local btn_data = {{color='green', text='确 定', func = this.back_to_login}}
            SpecialUiController.HideLoading()
            Popup.PopupClientWithCode("login", json_data.error_code, btn_data)
        end
    ) 
end

--切换后台
local time = 0
function Network.OnApplicationPause( paused )
    if paused == false then
        if os.time() - time > 10 and networkMgr.connected then
            CtrlManager.EnterAgain()
        end
    else
        time = os.time()
        CtrlManager.QuitApp()
    end
end

local last_heart_beat = true
local heart_beat_request_time
local heart_beat_response_time
function Network.HeartBeat()
    -- body
    this.AddListener('Account', 'Account_HeartBeat_Response', function ( data )
        last_heart_beat = true
        heart_beat_response_time = Util.GetTime()
    end)
    while true do
        if not last_heart_beat then -- 上次的心跳都没回来, 直接断网 
            Network.OnDisconnect()
            last_heart_beat = true
        end
        if UserData.get_is_login() then
            last_heart_beat = false
            login_conf.last_token_time = Util.GetUnixTime()
            FILE_DB.PersistFileDB('__login__')
            heart_beat_request_time = Util.GetTime()
            heart_beat_response_time = nil
            this.SendMessage( 'Account', 'Account_HeartBeat_Request', {})
        else
            last_heart_beat = true
        end
        coroutine.wait(20)
    end
end

function Network.GetDelay( )
    local res = heart_beat_response_time or Util.GetTime()
    return res - (heart_beat_request_time or Util.GetTime())
end

-- 连上会检擦这个，第一次登陆也走这个逻辑
function Network.CheckReLogin()
    -- body
    if string.len(UserData.get_login_token()) > 0 and not UserData.get_is_login() and not is_logining then
        if Util.GetUnixTime() - login_conf.last_token_time >= 500 and login_conf.last_login and #login_conf.last_login > 0 then
            -- token已經超時，重新獲取
            Network.Login( login_conf.last_login )
        else
            SpecialUiController.ShowLoading('正在登录', 10, this.back_to_login)
            local ip = Base64.DecodeString(LoadingCtrl.ServerJson.client_ip) or '0.0.0.0'
            ip = string_split(ip, ":")[1]
            is_logining = true
            coroutine.start(function ( ... )
                coroutine.wait(5)
                is_logining = false
            end)
            this.SendMessage( 'Account', 'Account_Login_Request', {
                token = UserData.get_login_token(),
                account = {
                    account_type = login_conf.platformName or AppConst.PlatformName,
                    phone_number = UserData.account == '游客' or '' and UserData.account,
                },
                client = {app_name = "gamehall",
                    package = Application.identifier,
                    version = Game.GetVersion(),
                    channel = AppConst.Channel,
                    eth_ip = Util.GetAddressIP(),
                    ip = ip ,
                    mac = Util.GetMacAddress(),
                    device = Util.GetDeviceType(),
                    os = Util.GetOperatingSystem(),
                    os_version = Util.GetSystemVersion(),
                    imei_idfa = Util.GetIdfa(),
                    device_id = Util.GetDeviceUniqueIdentifier(),
                },
            })
        end
    end
end


-- 发送消息失败，是不是重连后重发？
function Network.OnMessageFail( buffer )

end

function Network.OnKickOff( )
    keeping_connect = false
    g_is_connecting = false
    Popup.PopupClient('\n您的账号在其它地方登陆', {{color='green', text='确 定', func = this.back_to_login}})
end

--客户端因为读写连异常主动断线--
function Network.OnException(  ) 
    --networkMgr:SendConnect()
    networkMgr:Close()
    g_is_connecting = false
    UserData.set_is_login(false)
   	--logError("OnException------->>>>")
end

--可能是tcp连接不成功，客户端因为包解析错误主动断线等--
function Network.OnDisconnect( )
    networkMgr:Close()
    g_is_connecting = false
    UserData.set_is_login(false)
    -- Popup.PopupClient('\n服务器断开', {{color='green', text='确 定', func = this.back_to_login}})
    --logError("OnDisconnect------->>>>")
end

function Network.Close()
    -- body
    networkMgr:Close()
end

function Network.SendMessageWithJson( main_name, sec_name, json )
    local tmp = FILE_DB.JsonToLua(json)
    Network.SendMessage( main_name, sec_name, tmp )
end

function Network.SendMessage( main_name, sec_name, val_tab )
    logWarn(string.format('send pkg %s %s %s', main_name, sec_name, TableToStr(val_tab)))
    local main_id, sec_id = GetProtocolIdByName(main_name, sec_name)
    local data_struct = GetProtocolStructByName(main_name, sec_name)
    -- if AppConst.DebugMode then
    --     SendMessageCtrl.AddHistoryItem(main_name, sec_name, val_tab)
    -- end
    function assignWithRepeated( source, pool )
        source = source or {}
        pool = pool or {}

        local meta = getmetatable(pool)
        if meta and meta._descriptor and meta._descriptor.fields then
            for k,v in pairs(meta._descriptor.fields) do
                if source[v.name] ~= nil then
                    if type(source[v.name]) == "table" then
                        if v.label == 3 then -- repeated
                            for index, _ in ipairs(source[v.name]) do
                                if v.cpp_type == 10 and type(source[v.name][index]) == "table" then -- message
                                    local msg = pool[v.name]:add()
                                    -- TaskCtrl.NormalAssignProto(source[v.name][index], msg)
                                    assignWithRepeated(source[v.name][index], msg)
                                else -- normal
                                    pool[v.name]:append(source[v.name][index])
                                end
                            end
                        else -- message
                            -- TaskCtrl.NormalAssignProto(source[v.name], pool[v.name]) 
                            assignWithRepeated(source[v.name], pool[v.name])
                        end
                    else -- normal
                        if type(source[v.name]) == 'number' then
                            pool[v.name] = math.floor(source[v.name])
                        else
                            pool[v.name] = source[v.name]
                        end 
                    end
                end
            end
        end
    end

    assignWithRepeated(val_tab, data_struct)

    local msg = data_struct:SerializeToString()

    local buffer = ByteBuffer.New()
    buffer:WriteShort(main_id)
    buffer:WriteShort(sec_id)
    buffer:WriteBase64Buffer(msg)
    local sequence_id = networkMgr:SendMessage(buffer)
end

function Network.AddListener( main_name, sec_name, handler, exception_func )
    local main_id, sec_id = GetProtocolIdByName(main_name, sec_name)
    Event.AddListener(this.GetEventId(main_id, sec_id), handler)
    Event.AddListener(this.GetExceptionId(main_id, sec_id), exception_func)
end

-- 保证一个ctrl对同一个listen只能注册一个监听， exception_func默认为空
local _auto_table = {}
local _auto_model_table = {}
function Network.AutoListener( ctrl, main_name, sec_name, handler, exception_func)
    local main_id, sec_id = GetProtocolIdByName(main_name, sec_name)
    local event_id = this.GetEventId(main_id, sec_id)
    local exception_id = this.GetExceptionId(main_id, sec_id)

    _auto_table[ctrl] = _auto_table[ctrl] or {}
    if not _auto_table[ctrl][event_id] then
        Event.AddListener(event_id, function(...) _auto_table[ctrl][event_id][1](...) end)
        Event.AddListener(exception_id, function(...) _auto_table[ctrl][event_id][2](...) end)
    end
    _auto_table[ctrl][event_id] = {function(...) if ctrl.isAwake then handler(...) end end,
        function(...) if ctrl.isAwake and exception_func then exception_func(...) end end}
end

function Network.ClearAllAutoListener( ctrl )
    if _auto_table[ctrl] then
        for k,v in pairs(_auto_table[ctrl]) do
            _auto_table[ctrl][k] = {function ( ... ) end, function ( ... ) end}
        end
    end
end

function Network.AutoModelListener(ctrl, main_name, handler, exception_func)
    local id_table = GetProtocolIdTableByName(main_name)
    for sec_name, _ in pairs(id_table) do
        local main_id, sec_id = GetProtocolIdByName(main_name, sec_name)
        local event_id = this.GetEventId(main_id, sec_id)
        local exception_id = this.GetExceptionId(main_id, sec_id)

        _auto_model_table[ctrl] = _auto_model_table[ctrl] or {}

        if not _auto_model_table[ctrl][event_id] then
            Event.AddListener(event_id, function(...) _auto_model_table[ctrl][event_id][1](...) end)
            Event.AddListener(exception_id, function(...) _auto_model_table[ctrl][event_id][2](...) end)
        end
        _auto_model_table[ctrl][event_id] = {function(...) if ctrl.isAwake then handler(...) end end,
            function(...) if ctrl.isAwake and exception_func then exception_func(...) end end}
    end
end

function Network.RemoveListener( main_name, sec_name, handler, exception_func )
    local main_id, sec_id = GetProtocolIdByName(main_name, sec_name)
    Event.RemoveListener(this.GetEventId(main_id, sec_id), handler)
    Event.RemoveListener(this.GetExceptionId(main_id, sec_id), exception_func)
end

function Network.GetExceptionId(main_id, sec_id)
    -- body
    return math.floor(4294967296 + main_id * 65536 + sec_id)
end

function Network.GetEventId( main_id, sec_id )
    return math.floor(main_id * 65536 + sec_id)
end

--登录返回--
function Network.OnMessage(buffer) 
	local main_id = buffer:ReadShort()
    local sec_id = buffer:ReadShort()
    local data = buffer:ReadBase64Buffer()
    local main_name, sec_name = GetProtocolNameById(main_id, sec_id)
    local data_struct = GetProtocolStructById(main_id, sec_id)
    data_struct:ParseFromString(data)
    if AppConst.DebugMode then
        logWarn(string.format('recv pkg %s %s %s', main_name, sec_name, tostring(data_struct)))
    end
    if data_struct:HasField('ret') and data_struct.ret.code ~= 0 then
        -- TODO 上线前关闭这个弹框
        -- Popup.PopupError(data_struct.ret.code, data_struct.ret.msg)
        Event.Brocast(this.GetExceptionId(main_id, sec_id) , data_struct)
        return
    end

    if data_struct:HasField('player') then
        UserData.SetPlayer(data_struct.player, this.GetEventId(main_id, sec_id))
    end

    -- 提交响应函数处理，这里也用事件吧
    Event.Brocast(this.GetEventId(main_id, sec_id) , data_struct)
	----------------------------------------------------
end

--卸载网络监听--
function Network.Unload()
    Event.RemoveListener(Protocal.Connect);
    Event.RemoveListener(Protocal.Message);
    Event.RemoveListener(Protocal.Exception);
    Event.RemoveListener(Protocal.Disconnect);
    Event.RemoveListener(Protocal.MessageFailed);
    FILE_DB.PersistFileDB('__login__')
    logWarn('Unload Network...');
end