require "Controller/golden_shark/GoldenSharkDataConfig"
require "autogen/ComicIndianaConfig11"
require "autogen/ZJHConfig"

RichText = {}

local rich_text = {
	AccPrize = "%s恭喜%s玩家%s在连环夺宝%s%s座位上投注%s线%s金币，在第%s关拍中%s连%s累积奖，共赢得%s金币。",
	NormalPrize = "%s恭喜%s玩家%s在连环夺宝%s%s座位上投注%s线%s金币，共赢得%s金币。",
	ComicIndianaEndlessAccPrize = "%s恭喜%s玩家%s在迷你夺宝中投注%s线%s金币，拍中%s连%s累计奖，共赢得%s金币。",
	ComicIndianaEndlessNormalPrize = "%s恭喜%s玩家%s在迷你夺宝中投注%s线%s金币，共赢得%s金币。",
	PropPrize = "%s恭喜%s玩家%s开启一个%s，获得%s倍充值奖励共%s金币。",
	PropJackpotPrize = "%s恭喜%s玩家%s开启一个%s，人品爆棚获得Jackpot大奖共%s金币。",
	VipUserLogin = "%s热烈欢迎%s玩家%s上线，预祝他今日游戏鸿运当头。",
	VipLevelUp = "%s恭喜玩家%s将VIP等级提升至%s，获得更多VIP特权与奖励。",
	BecomeVip = "%s恭喜玩家%s充值成为VIP玩家，获得尊贵VIP身份与特权。",
	TimerReward = "%s恭喜%s玩家%s在定时抽奖中抽中大奖，共获得%s。",
	TreeReward = "%s恭喜%s玩家%s的摇钱树在他的悉心照料下，今日共结出%s金币。",
	MassiveBattleTopPrize = "%s恭喜%s玩家%s在百人金花中喜中Jackpot大奖，共赢得%s金币。",
	MassiveBattleTopWin = "%s恭喜%s玩家%s在百人金花中大展身手，共赢得%s金币。",
	SendTrumpet = "%s玩家<color=#FFE200>%s</color>：%s",
	SpadeAceWin = "%s恭喜%s玩家%s在猜黑桃A中翻出黑桃A，轻松赢取%s金币！",
	SpadeAceJackpot = "%s恭喜%s玩家%s在猜黑桃A中翻出皇家黑桃A，轻松赢取%s金币。",
	GoldenSharkTopPrize = "%s恭喜%s玩家%s在金鲨银鲨中押中%s，共赢得%s金币。",
	GoldenSharkTopWin = "%s恭喜%s玩家%s在金鲨银鲨中押中%s，共赢得%s金币。",
	GoldenSharkTopWin_Origin = "%s恭喜%s玩家%s在金鲨银鲨中运气爆棚，共赢得%s金币。",
	System = "%s%s",
	SlotsBigPrize = "%s恭喜%s玩家%s在水浒传%s中运气爆棚，共赢得%s金币。",
	SlotsOverallPrize = "%s恭喜%s玩家%s在水浒传%s中开出%s全盘奖，共赢得%s金币。",
	SlotsHandselPrize = "%s恭喜%s玩家%s刚刚在水浒传%s彩金奖中获得%s金币的派彩，真是羡煞旁人！",
	ZJHWinGold = "%s恭喜%s玩家%s在炸金花%s场中大杀四方，共赢得%s金币。",
	ZJHJackpotGold = "%s恭喜%s玩家%s在炸金花%s场中拿到%s，获得Jackpot奖金%s金币。",
	NiuniuWin = "恭喜VIP%s玩家%s在抢庄斗牛%s场中牛气冲天，共赢得%s金币。",
}

local gems_text = {
	[1.0] = {"白玉", "碧玉", "墨玉", "玛瑙", "琥珀"},
	[2.0] = {"祖母绿", "猫眼石", "紫水晶", "翡翠", "珍珠"},
	[3.0] = {"红宝石", "绿宝石", "黄宝石", "蓝宝石", "钻石"},
}

local animal_type_name = {
	[1] = '燕子',
	[2] = '鸽子',
	[3] = '孔雀',
	[4] = '老鹰',
	[5] = '兔子',
	[6] = '熊猫',
	[7] = '猴子',
	[8] = '狮子',
	[9] = '金鲨',
	[10] = '银鲨',
	[11] = '飞禽',
	[12] = '走兽',
}

local rooms = {
	[1] = "梁山泊",
	[2] = "好汉机",
	[3] = "银花台",
	[4] = "金龙关",
}

local slots_items = {
	[1] = "水浒传",
	[2] = "忠义堂",
	[3] = "替天行道",
	[4] = "宋江",
	[5] = "林冲",
	[6] = "鲁智深",
	[7] = "金刀",
	[8] = "银枪",
	[9] = "铁斧",
	[10] = "人物",
	[11] = "武器",
}

local function get_room_name( room_id )
	return rooms[room_id]
end

local function get_item_name( item_id )
	return slots_items[item_id]
end

local function format_gold(gold)
	return string.format("<color=#fcf302>%s</color>", gold)
end

local function format_prize(prize)
	local remainder = tonumber(prize)
	local str = ""
	if remainder >= 100000000 then
		str = str .. tostring(math.floor(remainder / 100000000)) .. "亿"
		remainder = remainder % 100000000
	end
	if remainder >= 10000 then
		str = str .. tostring(math.floor(remainder / 10000)) .. "万"
		remainder = remainder % 10000
	end
	if remainder >= 1 then
		str = str .. remainder
	end
	return str
end

local function judge_vip(vip)
	return vip > 0 and "VIP" .. vip or ""
end

local function format_vip(vip)
	return string.format("<color=#fd02cb>%s</color>", judge_vip(vip))
end

local function format_nickname(nickname)
	return string.format("<color=#fcf302>%s</color>", nickname)
end

local function get_room_alias_by_room_name( room_name )
	for k,v in ipairs(ComicIndianaConfig11.RoomList) do
		if v.name == room_name then
			return v.alias
		end
	end
	return ""
end

local card_type_to_name = {
	[1] = "三条A",
	[2] = "豹子",
	[3] = "同花顺",
	[4] = "同花",
	[5] = "顺子",
	[6] = "对子",
	[7] = "单牌",
}

function get_zjh_room_alias_by_name(room_name)
	for k,v in ipairs(ZJHConfig.RoomRule) do
		if v.name == room_name then
			return v.alias
		end
	end
	return ""
end

local function format_seat_id( seat_id )
	local seat_id = tonumber(seat_id)
	if not seat_id then return "" end
	if seat_id < 10 then
		return "00" .. tostring(seat_id)
	elseif seat_id < 100 then
		return "0" .. tostring(seat_id)
	else
		return tostring(seat_id)
	end
end

--累计奖  
function RichText.CreateAccPrizeInfo(chat_player, data, is_rich)
	--参数依次为vip等级，昵称，关卡等级，投注线数，投注金币数，连数，宝石颜色，中奖金额
    if is_rich then
    	return string.format(rich_text["AccPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip) , format_nickname(chat_player.user.nickname), get_room_alias_by_room_name(data.roomid), format_seat_id(data.tableid), 
			data.bet_line, format_gold(data.bet_score), data.stage, format_nickname(data.lines), gems_text[data.stage][data.color], format_gold(format_prize(data.prize))) 
    else
    	return string.format(rich_text["AccPrize"], "", judge_vip(chat_player.character.vip) , chat_player.user.nickname, get_room_alias_by_room_name(data.roomid), format_seat_id(data.tableid), data.bet_line, 
    		data.bet_score, data.stage, data.lines, gems_text[data.stage][data.color], format_prize(data.prize)) 
    end
end

--普通中奖
function RichText.CreateNormalPrizeInfo(chat_player, data, is_rich)
	--参数依次为vip等级，昵称，关卡等级，投注线数，投注金币数，中奖金额
	if is_rich then
		return string.format(rich_text["NormalPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), get_room_alias_by_room_name(data.roomid), format_seat_id(data.tableid),
			data.bet_line, format_gold(data.bet_score), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["NormalPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, get_room_alias_by_room_name(data.roomid), format_seat_id(data.tableid),
			data.bet_line, data.bet_score, format_prize(data.prize))
	end
end

--迷你夺宝累计奖  
function RichText.CreateComicIndianaEndlessAccPrizeInfo(chat_player, data, is_rich)
	--参数依次为vip等级，昵称，关卡等级，投注线数，投注金币数，连数，宝石颜色，中奖金额
    -- local stage = data.stage == 4 and 1 or data.stage
    local stage = 1
    if is_rich then
    	return string.format(rich_text["ComicIndianaEndlessAccPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip) , format_nickname(chat_player.user.nickname),
			data.bet_line, format_gold(data.bet_score), format_nickname(data.lines), gems_text[stage][data.color], format_gold(format_prize(data.prize))) 
    else
    	return string.format(rich_text["ComicIndianaEndlessAccPrize"], "", judge_vip(chat_player.character.vip) , chat_player.user.nickname, data.bet_line, 
    		data.bet_score, data.lines, gems_text[stage][data.color], format_prize(data.prize)) 
    end
end

--迷你夺宝普通中奖
function RichText.CreateComicIndianaEndlessNormalPrizeInfo(chat_player, data, is_rich)
	--参数依次为vip等级，昵称，关卡等级，投注线数，投注金币数，中奖金额
	if is_rich then
		return string.format(rich_text["ComicIndianaEndlessNormalPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname),
			data.bet_line, format_gold(data.bet_score), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["ComicIndianaEndlessNormalPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			data.bet_line, data.bet_score, format_prize(data.prize))
	end
end

--开宝箱奖励
function RichText.CreatePropPrizeInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["PropPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), data.item_name,
			data.mutil, format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["PropPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, data.item_name,
			data.mutil, format_prize(data.prize))	
	end
end

--开宝箱Jackpot奖励
function RichText.CreatePropJackpotPrizeInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["PropJackpotPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), data.item_name,
			format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["PropJackpotPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, data.item_name,
			format_prize(data.prize))
	end
end

--Vip玩家登录
function RichText.CreateVipUserLoginInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["VipUserLogin"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname))
	else
		return string.format(rich_text["VipUserLogin"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname)
	end
end

--Vip等级提升
function RichText.CreateVipLevelUpInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["VipLevelUp"], "<color=red>系统消息</color>：", format_nickname(chat_player.user.nickname), format_vip(chat_player.character.vip))
	else
		return string.format(rich_text["VipLevelUp"], "", chat_player.user.nickname, judge_vip(chat_player.character.vip))
	end
end

--成为Vip
function RichText.CreateBecomeVipInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["BecomeVip"], "<color=red>系统消息</color>：", format_nickname(chat_player.user.nickname))
	else
		return string.format(rich_text["BecomeVip"], "", chat_player.user.nickname)
	end
end

--定时抽奖
function RichText.CreateTimerRewardInfo(chat_player, data, is_rich)
	if not data.item_name then
		local amount = "<color=#fcf302>" .. format_prize(data.amount) .. "</color>" .. "金币"
		if is_rich then
			return string.format(rich_text["TimerReward"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), amount)
		else
			return string.format(rich_text["TimerReward"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, format_prize(data.amount))
		end
	else
		local amount = "<color=#fcf302>" .. data.amount .. "</color>"
		local item_name = "<color=#fcf302>" .. data.item_name .. "</color>"
		if is_rich then
			return string.format(rich_text["TimerReward"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), amount .. "个" .. item_name)
		else
			return string.format(rich_text["TimerReward"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, data.amount .. "个" .. data.item_name)
		end
	end
end

--摇钱树抽奖
function RichText.CreateTreeRewardInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["TreeReward"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["TreeReward"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, format_prize(data.prize))
	end
end

--百人金花赢钱
function RichText.CreateMassiveBattleTopWinInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["MassiveBattleTopWin"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["MassiveBattleTopWin"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, format_prize(data.prize))
	end
end

--百人金花中大奖
function RichText.CreateMassiveBattleTopPrizeInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["MassiveBattleTopPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["MassiveBattleTopPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname, format_prize(data.prize))
	end
end

function RichText.CreateSendTrumpetInfo(chat_player, data, is_rich)
	-- local content = ChatMgr.ReplaceSensitiveWords(data.content)
	local content = data.content
	if is_rich then
		return string.format(rich_text["SendTrumpet"], format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), content)
	else
		return content
	end
end

function RichText.CreateSpadeAceWinInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["SpadeAceWin"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["SpadeAceWin"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			format_prize(data.prize))
	end
end

function RichText.CreateSpadeAceJackpotInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["SpadeAceJackpot"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["SpadeAceJackpot"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			format_prize(data.prize))
	end
end

function RichText.CreateGoldenSharkTopPrizeInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["GoldenSharkTopPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			GoldenSharkDataConfig.JackpotName[data.subtype], format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["GoldenSharkTopPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			GoldenSharkDataConfig.JackpotName[data.subtype], format_prize(data.prize))
	end
end

function RichText.CreateGoldenSharkTopWinInfo( chat_player, data, is_rich )
	if data.top_win_type then
		if is_rich then
			return string.format(rich_text["GoldenSharkTopWin"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
				"<color=#fd02cb>" .. animal_type_name[data.top_win_type] .. "</color>", format_gold(format_prize(data.prize)))
		else
			return string.format(rich_text["GoldenSharkTopWin"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
				animal_type_name[data.top_win_type], format_prize(data.prize))
		end
	else
		if is_rich then
			return string.format(rich_text["GoldenSharkTopWin_Origin"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
				format_gold(format_prize(data.prize)))
		else
			return string.format(rich_text["GoldenSharkTopWin_Origin"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
				format_prize(data.prize))
		end
	end
end

function RichText.CreateSlotsBigPrizeInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["SlotsBigPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			get_room_name(data.room_id), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["SlotsBigPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			get_room_name(data.room_id), format_prize(data.prize))
	end
end

function RichText.CreateSlotsOverallPrizeInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["SlotsOverallPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			get_room_name(data.room_id), get_item_name(data.item_id), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["SlotsOverallPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			get_room_name(data.room_id), get_item_name(data.item_id), format_prize(data.prize))
	end
end

function RichText.CreateSlotsHandselPrizeInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["SlotsHandselPrize"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			get_room_name(data.room_id), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["SlotsHandselPrize"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			get_room_name(data.room_id), format_prize(data.prize))
	end
end

function RichText.CreateZJHWinGoldInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["ZJHWinGold"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			get_zjh_room_alias_by_name(data.room_name), format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["ZJHWinGold"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			get_zjh_room_alias_by_name(data.room_name), format_prize(data.prize))
	end
end

function RichText.CreateZJHJackpotGoldInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["ZJHJackpotGold"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			get_zjh_room_alias_by_name(data.room_name), card_type_to_name[data.card_type], format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["ZJHJackpotGold"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			get_zjh_room_alias_by_name(data.room_name), card_type_to_name[data.card_type], format_prize(data.prize))
	end
end

function RichText.CreateNiuniuWinInfo( chat_player, data, is_rich )
	if is_rich then
		return string.format(rich_text["NiuniuWin"], "<color=red>系统消息</color>：", format_vip(chat_player.character.vip), format_nickname(chat_player.user.nickname), 
			data.room_name, format_gold(format_prize(data.prize)))
	else
		return string.format(rich_text["NiuniuWin"], "", judge_vip(chat_player.character.vip), chat_player.user.nickname,
			data.room_name, format_prize(data.prize))
	end
end

function RichText.CreateSystemInfo(chat_player, data, is_rich)
	if is_rich then
		return string.format(rich_text["System"], "<color=red>系统消息</color>：", "<color=yellow>" .. data.content .. "</color>")
	else
		return string.format(rich_text["System"], "", data.content)
	end
end

function RichText.CreateRichText(chat_player, data, is_rich)
	if rich_text[data.type] then
		return RichText[ "Create" .. data.type .. "Info"](chat_player, data, is_rich)
	end
	return ""
end