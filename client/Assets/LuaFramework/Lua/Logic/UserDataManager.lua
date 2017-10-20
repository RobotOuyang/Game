module('UserData', package.seeall)

local user_data = {}

local function DefineProperty( name, default_Val, event_id )
	-- body
	if user_data[name] ~= nil then
		_M['set_'..name](default_Val, event_id)
		return
	end

	user_data[name] = {val = default_Val, watch={}}
	_M['get_'..name] = function ()
		return user_data[name].val
	end

	_M['set_'..name] = function(val, ...)
		local old_value = user_data[name].val
		user_data[name].val = val
		for k,v in pairs(user_data[name].watch) do
			if k.isAwake then
				v(user_data[name].val, old_value, ...)
			end
		end
	end

	-- 只有number能add的
	_M['add_'..name] = function(val, ...)
		local old_value = user_data[name].val
		user_data[name].val = val + user_data[name].val
		for k,v in pairs(user_data[name].watch) do
			if k.isAwake then
				v(user_data[name].val, old_value, ...)
			end
		end
	end

	-- 属性变化的通知，watch的回调里再调set可能会死循环，注意同一个ctrl，watch同一个名字只有最后一个生效, 这样ctrl可以重复注册
	_M['watch_'..name] = function(ctrl, func)
		user_data[name].watch[ctrl] = func
		-- 立即调用一次, 不带参数
		if ctrl.isAwake then 
			func(user_data[name].val, user_data[name].val)
		end
	end
end

function UploadAvatar( file )
	-- body
	local url = '/upload/?&app_name=gamehall&token=' .. UserData.get_login_token()
	avatarMgr:UploadAvatar(file, url, function (sprite)
		-- body
		UserData.set_xxx_avatar(sprite)
		--换头像成功之后，发条协议到gameserver，用以完成修改头像这个任务
		Network.SendMessage( "TaskInfo", "TaskInfo_ChangeAvatar_Request", {})
	end)
end

function BindSelfAvatar(ctrl, call_back)
	-- 如果是手动上传头像后
	if type(get_xxx_avatar()) == 'number' then
		-- 没addwatch之前的set是不会触发回调
		set_xxx_avatar(SpecialUiController.GetAvatarSprite(get_sex()))
	end
	-- ctrl 可以重复watch
	watch_xxx_avatar(ctrl, call_back)

	if not avatarMgr:IsAvatarExsit(get_uid() .. '.jpg') then
		call_back(SpecialUiController.GetAvatarSprite(get_sex()))
	end

	local url = AppConst.AvatarServer .. '/download/?app_name=' .. UserData.get_app_name() .. '&player_id=' .. get_uid()
	avatarMgr:DownloadAvatarNoCache(get_uid() .. '.jpg', url, function ( sprite )
		-- body
		set_xxx_avatar(sprite)
		--换头像成功之后，发条协议到gameserver，用以完成修改头像这个任务
		Network.SendMessage( "TaskInfo", "TaskInfo_ChangeAvatar_Request", {})
	end, false)
end

function BindOthersAvatar(ctrl, sex, player_id, call_back)
	-- body
	if not avatarMgr:IsAvatarExsit(player_id .. '.jpg') then
		call_back(SpecialUiController.GetAvatarSprite(sex))
	end
	if player_id == 0 then return end

	local url = AppConst.AvatarServer .. '/download/?app_name=' .. UserData.get_app_name() ..'&player_id=' .. player_id
	avatarMgr:TryDownloadAvatar(player_id .. '.jpg', url, function ( sprite )
		-- body
		if ctrl.isAwake then
			call_back(sprite)
		end
	end, false)
end

local function check_field( source, value_name, name, event_id)
	local path = string.find(name, '[.]')

	if path then
		local cur = string.sub(name, 1, path-1)
		local tail = string.sub(name, path + 1)
		if source:HasField(cur) then
			check_field(source[cur], value_name, tail, event_id)
		end
		return
	end

	if source:HasField(name) then
		DefineProperty(value_name, source[name], event_id)
	end
end

-- 登陸時收到的palayer, 所有的协议，如果返回带有player域，则会刷新这个player
function SetPlayer( data, event_id )
	check_field(data, 'uid', 'id')
	check_field(data, 'name', 'user.nickname')
	check_field(data, 'sex', 'user.sex')
	check_field(data, 'signature', 'user.signature')
	check_field(data, 'coin', 'character.gold', event_id)		-- 暂时只有这个金币要延迟显示
	check_field(data, 'lhdb_cur_score', 'comic_indiana.current_gold')
	check_field(data, 'level', 'character.level')
	check_field(data, 'vip', 'character.vip')
	check_field(data, 'experience', 'character.experience')
	check_field(data, 'single_win_bonus_his', 'character.single_win_bonus_his')
	check_field(data, 'item_bag', 'prop.normal')
	check_field(data, 'daily_task', 'task_info.daily_task')
	check_field(data, 'his_task', 'task_info.his_task')
	check_field(data, 'max_hold_gold', 'record.max_hold_gold')
	check_field(data, 'max_win_gold', 'record.max_win_gold')
	check_field(data, 'acc_lottery_times', 'record.acc_lottery_times')
	check_field(data, 'banker_times', 'record.banker_times')
	check_field(data, 'win_all_times', 'record.win_all_times')
	check_field(data, 'lose_all_times', 'record.lose_all_times')
	check_field(data, 'brjh_jackpot_times', 'record.brjh_jackpot_times')
	check_field(data, 'aaa_times', 'record.aaa_times')
	check_field(data, 'change_nickname_times', 'record.change_nickname_times')
	check_field(data, 'change_sex_times', 'record.change_sex_times')
	check_field(data, 'charge', 'character.charge')
	check_field(data, 'last_login_time', 'character.last_login_time')
	check_field(data, 'is_bind_phone', 'record.bind_free_set')
	check_field(data, 'android_charge', 'character.android_charge')
	check_field(data, 'ios_charge', 'character.ios_charge')
	check_field(data, 'daily_coin', 'character.acc_win_bonus_daily')
	check_field(data, 'first_charge_info', 'charge_info.normal')
	check_field(data, 'shop_tag', 'character.shop_tag')
	check_field(data, 'banker_gold', 'massive_battle.banker_gold')
	check_field(data, 'golden_shark_banker_gold', 'golden_shark.banker_gold')
	check_field(data, 'daily_acc_win', 'massive_battle.daily_acc_win')
	check_field(data, 'hta_jackpot_times', "record.hta_royal_times")
	check_field(data, 'jsys_banker_times', 'record.jsys_banker_times')
	check_field(data, 'jsys_jackpot_time', 'record.jsys_jackpot_time')
	check_field(data, 'jsys_sharks_times', "record.jsys_sharks_times")
	check_field(data, 'golden_shark_daily_acc_win', 'golden_shark.daily_acc_win')
	check_field(data, 'slots_amount_gear', 'slots.bet_amounts')
	check_field(data, 'zjh_auto_compare', "config.zjh_auto_compare")
	check_field(data, 'zjh_bet_jackpot', "config.zjh_bet_jackpot")
	check_field(data, 'is_timedgift_display', "timed_gift.is_display")
	check_field(data, "shz_overall_times", "record.shz_overall_times")
	check_field(data, "activity_history_data", "activity_record.history_data")
	check_field(data, 'niuniu_use_robot', "config.niuniu_use_robot")
	check_field(data, 'pre_consume', "niuniu.pre_consume")
end

function SetUser( data )
	-- body
	check_field(data, 'name', 'nickname')
	check_field(data, 'sex', 'sex')
	check_field(data, 'signature', 'signature')
end

-- 注意 如果定义一个table，只修改table内部的值可能触发不了watch，待改进
DefineProperty('uid', 0)
DefineProperty('name', '')
DefineProperty('sex', 0) 
DefineProperty('signature', '')
DefineProperty('coin', 0)
DefineProperty('lhdb_cur_score', 0 )
DefineProperty('level', 0)
DefineProperty('item_bag', '{}')
DefineProperty('daily_task', '{}')
DefineProperty('his_task', '{}')
DefineProperty('first_charge_info', '{}')
DefineProperty('ticket', 0)
DefineProperty('last_login_time', 0)

DefineProperty('vip', 0)
DefineProperty('charge', 0)
DefineProperty('seat_id', 10)
DefineProperty('daily_coin', 0)
DefineProperty('lhdb_max_coin', 0)
DefineProperty('lhdb_max_win_once', 0)
DefineProperty('lhdb_max_win_round', 0)
DefineProperty('lhdb_jackpot_count', 0)
DefineProperty('jsys_banker_times', 0)
DefineProperty('jsys_jackpot_time', 0)
DefineProperty('jsys_sharks_times', 0)
DefineProperty('shz_overall_times', 0)
DefineProperty('acc_lottery_times', 0)
DefineProperty('change_nickname_times', 0)
DefineProperty('change_sex_times', 0)
DefineProperty('slots_amount_gear', '{}')
DefineProperty('is_timedgift_display', 0)
DefineProperty('next_online_reward_sec', 923456789)

DefineProperty('account', nil)
DefineProperty('password', nil)
DefineProperty('shop_tag', '') -- 是不是特殊账号
DefineProperty('app_name', 'gamehall')
-- 一些状态也纪录
DefineProperty('is_login', false)
DefineProperty('login_token', '')

DefineProperty('wx_avatar_url', '')
DefineProperty('banker_gold', 0)

-- 下面这是用于一些特殊的回调，平时不要使用
DefineProperty('xxx_avatar', 0)

DefineProperty("from_login", true)
DefineProperty("activity_history_data", "[]")
DefineProperty("niuniu_use_robot", 0)
DefineProperty("pre_consume", 0)

function ClearLoginData()
	set_is_login(false)
	set_login_token('')
	set_from_login(true)
end