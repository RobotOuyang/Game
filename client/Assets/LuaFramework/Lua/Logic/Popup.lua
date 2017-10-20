require "Common/TipText"
require "autogen/PropConfig"
require "View/ShopPanel"

module('Popup', package.seeall)

local error_code_msg = {
	[309] = '该手机号码已绑定其他账号',
}

function PopupError( code, msg )
	-- body
	SpecialUiController.HideLoading()
	local error_msg = error_code_msg[code]
	if not error_msg then error_msg = msg end

	CtrlManager.Open('MessageCtrl', {is_client = true ,data = {content = error_msg, has_quit = true, buttons = {{color='green', text='确 定'}}}} )
end

function PopupErrorWithCode( module_name, code, data )
	SpecialUiController.HideLoading()
	local module_tips = TipText.Code2Tips[module_name]
	local content
	if module_tips then
		content = module_tips[code]
		if not content then content = module_tips["default"] end
	end
	if not content then content = TipText.Code2Tips["default"] end
	if data then
		content = string.format(content, data)
	end
	CtrlManager.Open('MessageCtrl', {is_client = true ,data = {content = content, has_quit = true, buttons = {{color='green', text='确 定'}}}} )
end

function PopupClient( content, btn_data, has_quit )
	-- body
	return CtrlManager.Open('MessageCtrl', {is_client = true , data = {content = content, has_quit = has_quit, buttons = btn_data}})
end

--直接传code，然后在TipText.ServerPopupText里写code以及code对应的文字即可
function PopupClientWithCode(module_name, code, btn_data, has_quit )
	local module_tips = TipText.Code2Tips[module_name]
	local content
	if module_tips then
		content = module_tips[code]
		if not content then content = module_tips["default"] end
	end
	if not content then content = TipText.Code2Tips["default"] end

	CtrlManager.Open('MessageCtrl', {is_client = true , data = {content = content, has_quit = has_quit, buttons = btn_data}})
end

function PopupServer( data )
	CtrlManager.Open('MessageCtrl', {is_client = false , data = data})
end

function PopupCall(data)
	if data:HasField("player_id") and data.player_id ~= UserData.get_uid() then
		return
	end
	Network.SendMessage(data.module_id, data.message_id, data.args and FILE_DB.JsonToLua(data.args) or {})
end

local timed_gift_goods_id = {50001, 50002, 50003, 50004, 50005, 50006, 50007, 60001, 60002, 60003, 60004, 60005, 60006, 60007}
function PopupChargeSuccess( data )
	--服务端发来的item中id可能会有重复的，在客户端手动合并
	local function unite_items( items )
		local items_dict = {}
		for _,v in ipairs(items) do
			if not items_dict[v[1]] then
				items_dict[v[1]] = v[2]
			else
				items_dict[v[1]] = items_dict[v[1]] + v[2]
			end
		end
		local item_list = {}
		for k,v in pairs(items_dict) do
			table.insert(item_list, {k,v})
		end
		return item_list
	end

	SoundManager.PlaySoundOnce("money_sound6")

	if data.item then
		local clean_data = {}
		for k,v in ipairs(data.item) do
			table.insert(clean_data, {v.id, v.amount})
		end

		local item_list = unite_items(clean_data)

		if not AppConst.DebugMode then
			--接dataeye
			for _, item in ipairs(item_list) do
				if item[1] == 10001 or item[1] == "Gold" then

					DataEye.CoinGain("充值获得金币", "Gold", item[2], UserData.get_coin())
				elseif item[1] == 10002 or item[1] == "Ticket" then
					DataEye.CoinGain("充值获得金券", "GoldTicket", item[2], ItemBagManager.GetItemNumByItemId(10002))
				else
					DataEye.ItemGet(item[1], item[2], "充值附赠道具")
				end
			end

			--接dataeye
			if data.goods_id and data.payment_id and data.payment_type then
				if PropConfig.Shop[data.goods_id] then
					local rmb_count = PropConfig.Shop[data.goods_id].amount
					DataEye.ChargeSuccess(data.payment_id, rmb_count, data.payment_type)
				end
			end
		end
		if	data.goods_id and data.payment_type then
			if	PropConfig.Shop[data.goods_id] then
				local rmb_count = PropConfig.Shop[data.goods_id].amount;
				print("充值人民币数量为：" .. rmb_count);
				for _, item in ipairs(item_list) do
					if item[1] == 10001 or item[1] == "Gold" then
						if AppConst.Channel == "appstore_lhdb" then
							print("如果渠道id正确就调用GA_Pay");
							paymentManager.GA_Pay(rmb_count, "appstore_lhdb", item[2]);
						end
						if AppConst.Channel == "appstore_bsfb" then	
							print("如果渠道id正确就调用GA_Pay");
							paymentManager.GA_Pay(rmb_count, "appstore_bsfb", item[2]);
						end
					end
				end	
			end
		end
		local chest_id = PropConfig.Shop[data.goods_id].chest
		local data = {}
		data.item_list = item_list
		data.type = "charge"
		data.buttons = {[1] = {text = "确定", color = "blue"}}
		if chest_id ~= 0 then
			table.insert(data.buttons, {text = "打开宝箱", color = "green", callback_openchest = function()
				Network.SendMessage("Prop", "Prop_Use_Request", {item = {id = chest_id, amount = 1}})
			end})
		end
		CtrlManager.Open("CommonRewardCtrl", data)
	end

	if table.has_value(timed_gift_goods_id, data.goods_id) then
		local pay_rect = ShopPanel:Find('back/gold_panel/scroll/Viewport/Content/gold_list_time')
		if (GameHallPanel and GameHallPanel.obj_time_gift) or (ShopPanel and pay_rect) then
			GameHallPanel.obj_time_gift:SetActive(false)
			pay_rect.gameObject:SetActive(false)
		end
	end
end