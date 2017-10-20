module('SDK', package.seeall)

uc_sid = ''

function is_bad_version()
	return "appstore_zsfqzs" == AppConst.Channel
end

function is_yyb()
	return string.sub(AppConst.Channel, 1, 3) == 'yyb'
end

function is_xiaomi()
	return string.sub(AppConst.Channel, 1, 6) == 'xiaomi'
end

function is_channel( chan )
	return string.sub(AppConst.Channel, 1, #chan) == chan
end
--目前用不到微信登录,同时跟百度sdk会有兼容性问题所以先屏蔽掉
-- function no_wechat()
-- 	return is_yyb() or is_channel('vivo') or is_channel('lenovo') or is_channel('xm_iapppay') --or not paymentManager.HasWXInstalled()
-- end

function CantChangeAcc()
	return GameHallCtrl.CloseBindPhone()
    -- return is_channel('xiaomi') or is_channel('tiantian') or is_channel('uc-single') or is_channel('huawei') or is_channel('ttlhdb_huawei')
    -- or is_channel('baidu')  or is_channel('dbyxds_mi') or string.sub(AppConst.Channel, 1, 2) == 'uc' or is_channel('yyb')
end

function CantBindPhone()
	return GameHallCtrl.CloseBindPhone()
	--return GameHallCtrl.CloseRank()
	--return is_channel('uc-single') or is_channel('huawei') or is_channel('ttlhdb_huawei') or is_channel('ttlhdbmi') or is_channel('vivo_ttlhdb')
    --or is_channel('uc_dbyxds') or is_channel('yyb') or is_channel('baidu') or is_channel('dbyxds_mi') or is_channel('yyb')
end

function CantShowInfo()
	--return is_channel('uc-single')
    return GameHallCtrl.CloseRank()
end

function start()
	-- 
end

function sdk_login( ... )
	-- body
end

local function urlEncode(s)  
     s = string.gsub(s, "([^%w%.%- ])", function(c) return string.format("%%%02X", string.byte(c)) end)  
    return string.gsub(s, " ", "+")  
end 

function init()
	if is_channel('appstore_yyaisi') then
		paymentManager.AsInit()
	end
end

function login()
	if is_channel('uc-single') or is_channel('uc_dbyxds') then
		paymentManager.InitUcSingle(function ( )
			UserData.set_account('游客')
			UserData.set_password('default')
   		 	Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), AppConst.PlatformName, AppConst.Access_info))
		end)
		return true
	elseif AppConst.Channel == 'xiaomi' or AppConst.Channel == 'xiaominew' or AppConst.Channel == 'ttlhdbmi' or AppConst.Channel == 'dbyxds_mi' then
		local channel_name = 'Xiaomi'
		if AppConst.Channel == 'xiaominew' then
			channel_name = 'Xiaominew'
		end
		if AppConst.Channel == 'ttlhdbmi' then
			channel_name = 'ttlhdbmi'
		end
		if AppConst.Channel == 'dbyxds_mi' then 
			channel_name = 'dbyxds_mi'
		end
		if AppConst.Channel == 'ttlhdbmi' or AppConst.Channel == 'dbyxds_mi' then
			UserData.set_account('游客')
			UserData.set_password('default')
	 		Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), AppConst.PlatformName, AppConst.Access_info))
		else	
			paymentManager.XiaoMiLogin(function (uid, session)	 
			    UserData.set_account('游客')
				UserData.set_password('default')
				Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default&session_info=%s', UserData.get_app_name(), channel_name, uid, session))
				end)
		end
		return true
	elseif is_channel('tiantian') then
		paymentManager.TianTianLogin(function ( msg )
			local login_data = FILE_DB.JsonToLua(msg)
				UserData.set_wx_avatar_url(login_data.avatar)
			UserData.set_account('游客')
			UserData.set_password('default')
			-- 可能出现切换账号
			if UserData.get_is_login() then
			    UserData.ClearLoginData()
			    Network.Close()
				Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), 'Tiantian', login_data.id))
			else
				Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), 'Tiantian', login_data.id))
			end

		end)
		return true
	-- elseif is_channel('huawei') then
	-- 	paymentManager.HuaweiLogin(false, function ( code )
	-- 		local url = string.format('https://api.vmall.com/rest.php?nsp_svc=OpenUP.User.getInfo&nsp_ts=%s&access_token=%s', os.time(), urlEncode(code))
	-- 		log(url)
	-- 		Network.CommonHttpCall(coroutine, url, function ( data )
	-- 			-- body
	-- 			if data.userID then
	-- 				UserData.set_account('游客')
	-- 				UserData.set_password('default')
	-- 				Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), 'Huawei', data.userID))
	-- 			else

	-- 			end
	-- 		end)
	-- 	end)
	-- 	return true
	elseif is_channel('ttlhdb_huawei') or is_channel('huawei') then
		paymentManager.HuaweiTTLogin(1, function ( playerID , result)
			-- if result == "error" then
			-- 	TouristLogin();
			-- end
					UserData.set_account('游客')
					UserData.set_password('default')
					Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), 'huawei', playerID))
		end)
		return true
	elseif is_channel('appstore_yyaisi') then
		paymentManager.ASLogin(function ( uid )
			UserData.set_account('游客')
			UserData.set_password('default')
			Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), 'As', uid))
		end)
		return true
	end
	return false
end


function TouristLogin()
    UserData.set_account('游客')
    UserData.set_password('default')
    Network.Login(string.format('/login/?app_name=%s&account_type=%s&access_info=%s&password=default', UserData.get_app_name(), AppConst.PlatformName, AppConst.Access_info))
end
local function loginfailed()

end

local function login_out_suc()
	UserData.set_is_login(false)
	Network.Close()
	UserData.ClearLoginData()
end

function pop_exit()
	if is_channel('uc-single') or is_channel('uc_dbyxds')  then
		paymentManager.UcExit()
		return true
	end
	return false
end

function direct_exit( ... )
	if is_channel('baidu') then
		paymentManager.BaiduExit()
		return true
	end
	return false
end

function logout()
	if is_channel('xiaomi') then
		paymentManager.XiaoMiLogout(function ( ... )
			login()
		end)
		return true
	elseif is_channel('tiantian') then
		paymentManager.TianTianLogout(function ( ... )
			login()
		end)
		return true
	elseif is_channel('uc-single') or is_channel('uc_dbyxds') then
		login()
		return true
	elseif is_channel('ttlhdb_huawei') or is_channel('huawei') then
		login()
		return true
	elseif is_channel('ttlhdbmi') or is_channel('dbyxds_mi') then
		login()
		return true
	elseif is_channel('vivo_ttlhdb') or is_channel('baidu') or is_channel('yyb') then
		--个别渠道接入sdk没有登录功能,不能出现登录界面
		TouristLogin()
		return true
	end
	return false
end