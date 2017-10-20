ChargeInfoManager = {}
local this = ChargeInfoManager

local fake_ctrl = {isAwake = true}
local listen_charge_change = {}
local charge_info_table = {}

function this.OnChargeChange( json_str )
	local charge_info_list = FILE_DB.JsonToLua(json_str)
	if not charge_info_list then return end

	for k,v in pairs(charge_info_list) do
		if charge_info_table[k] ~= v then
			for ctl, func in pairs(listen_charge_change) do
				if ctl.isAwake and func[k] then
					func[k](v)
				end
			end
		end
	end

	charge_info_table = charge_info_list
end

function this.GetChargeInfo()
	if type(charge_info_table) ~= 'table' then
		return {}
	end
	return charge_info_table
end

function this.WatchChargeInfoChange( ctrl, goods_id, call_back )
	listen_charge_change[ctrl] = listen_charge_change[ctrl] or {}
	listen_charge_change[ctrl][tostring(goods_id)] = call_back
	call_back(this.GetChargeInfo()[tostring(goods_id)])
end

function this.GetChargeInfoByGoodsId(goods_id)
	return this.GetChargeInfo()[tostring(goods_id)]
end

UserData.watch_first_charge_info(fake_ctrl, this.OnChargeChange)