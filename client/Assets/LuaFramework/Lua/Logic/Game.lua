local lpeg = require "lpeg"

require "Logic/LuaClass"
require "Logic/Popup"
require "Logic/CtrlManager"
require "Logic/SoundManager"
require "Logic/UserDataManager"
require "Logic/ItemBagManager"
require "Logic/TaskInfoManager"
require "Logic/ChargeInfoManager"
require "Common/functions"
require "Logic/AnimationManager"
require "Logic/SpecialUiController"
require "Logic/AccountManager"
require "Controller/ArtNumber"
require "Common/ConfInEditor"
require "Logic/DataEyeManager"
require "Controller/ChatMgr"
require "Controller/GameChat"
require "Controller/SubpackageUnload"
require "preload"

math.randomseed(os.time())
--管理器--
Game = {};
local this = Game;

local game; 
local transform;
local gameObject;
local test = {}

local iOSVersionCode = '1.06' -- 引擎的版本号小于这个了就需要重新下载引擎
local AndroidVersionCode = '1.06'
local AndroidUpUrl = 'http://gamehall-package.oss-cn-shanghai.aliyuncs.com/gamehall.apk'
local VersionCode = iOSVersionCode

local function ChannelUpdate()
    -- if SDK.is_channel('lhdb') or SDK.is_channel('hllhdb') or SDK.is_channel('mojitianqi') then
    --     AndroidVersionCode = '2.0.0510'
    -- end

    if Application.platform == RuntimePlatform.Android then
        VersionCode = AndroidVersionCode
    end
end

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    -- 这里还需要切换场景，然后再开始登录，连接登录服
    SoundManager.Init()
    CtrlManager.Init()
    SpecialUiController.Init()
    GameChat.Init()
    DataEye.Init()
    ChannelUpdate()
    MirrortechRichText.Init()

    if (Application.version < VersionCode and Application.isMobilePlatform) then
        Popup.PopupClient('\n客户端版本过旧，请更新客户端', {{color='green', text='确 定', func = function ( )
            -- 需要删除现有补丁，防止不兼容  (现在不需要了，不兼容用文件标记在引擎里解决)
            -- os.remove(Util.DataPath .. 'files.txt')
            if Application.platform == RuntimePlatform.IPhonePlayer then
                Application.OpenURL('http://itunes.apple.com/app/id' .. AppStoreId)
            elseif Application.platform == RuntimePlatform.Android then
                Application.OpenURL(AndroidUpUrl)
            end
            end}}, false)
        return
    end
    SubpackageUnload.LoadMainPanel("GoldenSharkMainCtrl")
    Network.KeepConnect()
    if AppConst.DebugMode then
        LoadSceneWithCtrl('Login', 'LoginCtrl')
    else
        Network.AddListener('Account', 'Account_Login_Response', this.OnLoginRes, function ()
            -- body
            Network.RemoveListener('Account', 'Account_Login_Response', this.OnLoginRes)
        end)
        PreLoad.Start()
        Network.LoginDirectly()
    end

    this.InitNetHandler()

    -- 不同包的配置信息
    this.conf_table = LuaFramework.Main and FILE_DB.JsonToLua(LuaFramework.Main.m_ConfForLua) or {}
    if this.conf_table['appstoreid'] then AppStoreId = this.conf_table['appstoreid'] end
    if this.conf_table['appstorepayid'] then AppStorePayid = this.conf_table['appstorepayid'] end
    logWarn('LuaFramework InitOK--->>>');
end

-- 這個隻生效一次
function this.OnLoginRes( data )
    -- coroutine.start(function ()
    --     coroutine.wait(2)
    --     LoadSceneWithCtrl('EliminateGame', 'eliminate/CheckPointCtrl')
    -- end)
    local url = UserData.get_wx_avatar_url()
    if #url > 0 and UserData.get_last_login_time() == 0 then
        avatarMgr:TryDownloadAvatar(UserData.get_uid() .. '.jpg', url, function ( sprite )
            -- body
            UserData.UploadAvatar(UserData.get_uid() .. '.jpg')
            UserData.set_wx_avatar_url('')
        end, true)
    end
    PreLoad.Stop()
    LoadSceneWithCtrl('Hall', 'GameHallCtrl')
    Network.RemoveListener('Account', 'Account_Login_Response', this.OnLoginRes)
end

function this.InitNetHandler()
    -- body
    Network.AddListener('PopUp', 'PopUp_Show_Notice', Popup.PopupServer)
    Network.AddListener('PopUp', 'PopUp_Call_Notice', Popup.PopupCall)
    Network.AddListener('PopUp', 'PopUp_Toast_Notice', function ( data )
        SpecialUiController.ShowTextMessage(nil, FILE_DB.JsonToLua(data.content).content, nil, FILE_DB.JsonToLua(data.content).duration)
    end)
    
    Network.AddListener("Command", "Command_GetGoods_Notice", Popup.PopupChargeSuccess)
    Network.AddListener("Prop", "Prop_Use_Response", function(data)
        if data.item.id >= 13001 and data.item.id <= 13012 then
            -- dataeye 开宝箱获得金币
            local props = {}
            for k,v in ipairs(data.obtain_items) do
                local one_item = {[1] = v.id, [2] = v.amount}
                table.insert(props, one_item)
            end

            local total_gold = 0
            for _, item in ipairs(props) do
                if item[1] == 10001 or item[1] == "Gold" then
                    total_gold = total_gold + item[2]
                end
            end
            DataEye.ItemConsume(data.item.id, 1, "开宝箱")
            if total_gold and total_gold > 0 then
                DataEye.CoinGain("开宝箱奖励", "Gold", total_gold, UserData.get_coin())
            end
        end
    end)

    pushManager:AddNotificationMessage("又到了每日定时抽奖的时间，快来转动转盘赢取百万大奖！", 11, 00, true)
    pushManager:AddNotificationMessage("又到了每日定时抽奖的时间，快来转动转盘赢取百万大奖！", 19, 00, true)
end

-- 要清除所有gameobject的缓存，他们都不可用
function Game.AfterSceneLoad()
    -- body
    SoundManager.ClearCache()
end

function Game.GetVersion()
    return Application.version
end

--销毁--
function Game.OnDestroy()
    --logWarn('OnDestroy--->>>');
end