module("TipText", package.seeall)

--不与返回的错误码对应的提示文本
Tips = {
	default = "系统错误",
	purchase_fail = "购买失败",
	present_success = "赠送成功",
	purchase_success = "购买成功",
	password_error = "您输入的登录密码有误，请重新输入",
	present_fail = "赠送失败",
	enter_password_too_many_times = "您今日输入密码次数过多，暂时无法使用赠送礼物功能",
	can_not_find_player = "找不到该玩家，请核对对方ID后重试",
	senditem_toplayerid_invalid = "请填写正确的ID",
	emotion_not_enough_gold = "您的金币不足，无法使用表情",
	emotion_not_enough_vip_level = "您的VIP等级不足，无法使用该表情",
	emotion_player_not_on_table = "该玩家已离座",
	kick_success = "踢人成功",
	kick_fail = "踢人失败",
	kick_not_enough_card = "踢人卡不足，是否前往商城购买？",
	kick_not_enough_card_toast = "踢人卡不足",
	kick_not_enough_vip_level = "只能请比您VIP等级低的玩家离开，是否前往提升VIP等级？",
	kick_player_has_left = "该玩家已离座",
	use_chest_fail = "开启失败",
	not_enough_ticket = "金券不足",
	not_enough_chest = "您没有宝箱",
	attachments_receive_fail = "领取失败",
	verify_phone_number_not_correct_client = "手机号码格式不正确",
	login_password_not_correct_client = "密码格式不正确",
	shop_gold_not_enough = "缺少%d金币",
	shop_gold_ticket_not_enough = "您的金券不足，是否去商城购买？",
	senditem_not_bind_phone = "您尚未绑定手机，暂不能使用赠送功能，是否前往绑定手机？",
	senditem_gold_not_enough = "您没有足够的金币",
	senditem_item_not_enough = "您没有足够的道具",
	task_comment_goto = "是否前往AppStore对游戏进行评价？",
	task_share_goto = "恭喜中大奖,是否分享到朋友圈？",
	task_moneytree_share_text = "恭喜获得%s金币,是否分享到朋友圈？",
	spade_ace_gold_not_enough = "抱歉,您的金币不足,无法参与该玩法",
	spade_ace_lack_gold_autocontract = "您的金币不足,无法继续本轮,系统自动收分",
	spade_ace_lack_gold_autoexit = "您的金币不足,无法继续猜黑桃A玩法",
	spade_ace_lack_gold_please_contract = "您的金币不足,无法继续本轮,请收分",
	spade_ace_too_much_gold = "您的金币过多,无法参与该玩法",
}

--与服务端返回的错误码对应的提示文本
Code2Tips = {
	["default"] = "系统错误",
	
	["emotion"] = {
		[110000] = "您的金币不足，无法使用表情",
		[110001] = "您的VIP等级不足，无法使用该表情",
		[110002] = "该玩家已离座",
		[110003] = "发送频繁,请稍后再试",
		[110005] = "表情发送场次不正确",

		["default"] = "表情播放失败",
	},

	["verify"] = {
		[801204] = "请求频繁",
		[801206] = "密码格式不正确",
		[801205] = "登录信息不存在",
		["default"] = "验证码发送失败",
	},

	["register"] = {
		[801305] = "密码格式不正确",
		[801306] = "验证码发送失败,请稍后再试",
		[801307] = "验证码不正确",
		[801308] = "验证码已过期",
		[801309] = "手机号码已被注册",
		["default"] = "注册失败",
	},

	["login"] = {
		[801104] = "账号不存在",
		[801105] = "密码不正确",
		["default"] = "登录失败",
	},

	--修改密码出错的时候的提示
	["password"] = {
		[801504] = "登录信息不存在",
		[801505] = "密码格式不正确",
		[801506] = "手机号码有误",
		[801507] = "验证码不正确",
		[801508] = "验证码已过期",
		["default"] = "修改密码失败",
	},

	["unregister"] = {
		[801404] = "登录信息不存在",
		[801407] = "验证码不正确",
		[801408] = "验证码已过期",
		["default"] = "更换绑定手机号码失败",
	},

	["spade_ace"] = {
		[113000] = "金币不足",
		[113001] = "下注额度超过上限或是低于下限",
		[113002] = "倍数超过上限或是低于下限",
		[113003] = "下注额度错误",
		[113004] = "下注额度太大",
		["default"] = "系统错误",
	},

	["comic_indiana_kick"] = {
		[101004] = "踢出发起者不正确",
		[101005] = "无法踢出无人座位",
		[101006] = "不在同一桌上",
		[101007] = "踢人等级不够",
		[101008] = "缺少踢人卡",
		["default"] = "踢人失败",
	},

	["slots_kick"] = {
		[210003] = "踢出发起者不正确",
		[210004] = "无法踢出无人座位",
		[210005] = "不在同一桌上",
		[210006] = "踢人等级不够",
		[115004] = "缺少踢人卡",
		["default"] = "踢人失败",
	},

	["slots"] = {
		[116008] = "金币不足",
		["default"] = "系统错误",
	}
}

GemsText = {
	[1.0] = {"白玉", "碧玉", "墨玉", "玛瑙", "琥珀"},
	[2.0] = {"祖母绿", "猫眼石", "紫水晶", "翡翠", "珍珠"},
	[3.0] = {"红宝石", "绿宝石", "黄宝石", "蓝宝石", "钻石"},
}