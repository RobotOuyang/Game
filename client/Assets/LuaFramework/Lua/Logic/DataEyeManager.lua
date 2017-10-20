module('DataEye', package.seeall)
require "autogen/PropConfig"

local this = {}
this.isAwake = true

function Init()	
	if Application.platform ~= RuntimePlatform.Android then
		UserData.watch_level(this, function ( new_level )
			if DCAccount then
				DCAccount.setLevel(new_level)
			end
		end)
		--age当vip等级
		UserData.watch_vip(this, function(vip_level)
			if DCAccount then
				DCAccount.setAge(vip_level)
			end
		end)

		UserData.watch_account(this, function(account_type)
			if DCAccount then
				if account_type == "游客" then
					DCAccount.setAccountType(DCAccountType.DC_Anonymous)
				else
					DCAccount.setAccountType(DCAccountType.DC_Registered)
				end
			end
		end)

		UserData.watch_is_login(this, function( is_login )
			if is_login then
				Login(UserData.get_uid())
			else
				Logout()
			end
		end)

		UserData.watch_coin(this, function( coin )
			CoinSetNum(coin, "Gold")
		end)

		ItemBagManager.WatchItemChange(this, "Ticket", function (amount)
			CoinSetNum(amount, "GoldTicket")
		end)
	end
end

function Login(account_id)
	if DCAccount and Application.platform ~= RuntimePlatform.Android then
		DCAccount.login(tostring(account_id))
	end
end

function Logout()
	if DCAccount and Application.platform ~= RuntimePlatform.Android then
		DCAccount.logout()
	end
end

--道具
--vc:virtualcurrency虚拟币:gold
function ItemBuy( item_id, count, vc_cost, vc_type)
	local item_alias = PropConfig.GetItemInfo(item_id, "alias")
	if DCItem and Application.platform ~= RuntimePlatform.Android and item_alias then
		DCItem.buy(tostring(item_id), item_alias, count, vc_cost, vc_type, "")
	end
end

function ItemGet( item_id, count, reason )
	local item_alias = PropConfig.GetItemInfo(item_id, "alias")
	if DCItem and Application.platform ~= RuntimePlatform.Android and item_alias then
		DCItem.get(tostring(item_id), item_alias, count, reason)
	end
end

function ItemConsume(item_id, count, reason)
	local item_alias = PropConfig.GetItemInfo(item_id, "alias")
	if DCItem and Application.platform ~= RuntimePlatform.Android and item_alias then
		DCItem.consume(tostring(item_id), item_alias, count, reason)
	end
end

--虚拟币统计
function CoinSetNum( coin_num, coin_type )
	if DCCoin and Application.platform ~= RuntimePlatform.Android then
		DCCoin.setCoinNum(coin_num, coin_type)
	end
end

--gain:玩家获取的值,left:加上获取后的值
function CoinGain( reason, coin_type, gain, left )
	if DCCoin and Application.platform ~= RuntimePlatform.Android then
		DCCoin.gain(reason, coin_type, gain, left)
	end
end

function CoinLost( reason, coin_type, lost, left )
	if DCCoin and Application.platform ~= RuntimePlatform.Android then
		DCCoin.lost(reason, coin_type, lost, left)
	end
end

--付费统计
function ChargeSuccess(order_id, currency_amount, payment_type)
	if DCVirtualCurrency and Application.platform ~= RuntimePlatform.Android then
		DCVirtualCurrency.paymentSuccess(tostring(order_id), "", currency_amount, "CNY", payment_type)
	end
end