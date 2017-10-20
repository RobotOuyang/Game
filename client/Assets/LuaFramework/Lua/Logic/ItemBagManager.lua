ItemBagManager = {};
local this = ItemBagManager;

local fake_ctrl = {isAwake = true}
local listen_item_change = {}
local item_bag_table = {}

-- local listen_items_change = {}
-- local items_bag_table = {}

function this.OnBagChange( json_str )
	local item_bag_list = FILE_DB.JsonToLua(json_str)
	local temp = {}
	if not item_bag_list then return end
	
	for _, v in ipairs(item_bag_list) do
		if type(v) == "table" then
			if item_bag_table[v[1]] ~= v[2] then
				for k, func in pairs(listen_item_change) do
					if k.isAwake and func[v[1]] then
						func[v[1]](v[2])
					end
				end
			end
			temp[v[1]] = v[2]
			item_bag_table[v[1]] = nil
		end
	end
	for k, v in pairs(item_bag_table) do
		for ctrl, func in pairs(listen_item_change) do
			if ctrl.isAwake and func[k] then
				func[k](0)
			end
		end
	end
	item_bag_table = temp
end

--支持watch多个item，只需调一个callback
-- function this.OnBagChangeExt( json_str )
-- 	local item_bag_list = FILE_DB.JsonToLua(json_str)

-- 	--没有的话就返回0，这个时候实际上也是变化了
-- 	local function get_item_new_count( id )
-- 		for _,v in ipairs(item_bag_list) do
-- 			if v[1] == id then
-- 				return v[2]
-- 			end
-- 		end
-- 		return 0
-- 	end

-- 	-- local temp = {}
-- 	for _, name_funcs in pairs(listen_items_change) do
-- 		local has_change = false
-- 		for _, name in ipairs(name_funcs[1]) do
-- 			local id = PropConfig.PropMap[name].id
-- 			if not items_bag_table[id] then items_bag_table[id] = 0 end
-- 			if items_bag_table[id] ~= get_item_new_count(id) then
-- 				has_change = true
-- 				break
-- 			end
-- 		end

-- 		if has_change then
-- 			name_funcs[2]()
-- 		end
-- 	end
-- 	items_bag_table = item_bag_list
-- end

function this.GetItemBag()
	if type(item_bag_table) ~= 'table' then
		return {}
	end
	return item_bag_table
end

function this.WatchWholeBag()
	-- body
end

function this.WatchItemChange(ctrl, item_name, call_back, ...)
	-- body
	listen_item_change[ctrl] = listen_item_change[ctrl] or {}
	listen_item_change[ctrl][PropConfig.PropMap[item_name].id] = call_back
	local item_num = this.GetItemNumByName(item_name)
	call_back(item_num, item_num)
end

--针对同一个ctrl,对同样的item_names,只支持有一个callback
-- function this.WatchItemsChange( ctrl, item_names, call_back, ... )
-- 	listen_items_change[ctrl] = listen_items_change[ctrl] or {}
-- 	local count = #listen_items_change[ctrl]
-- 	listen_items_change[ctrl][count + 1] = {}
-- 	listen_items_change[ctrl][count + 1][1] = item_names
-- 	listen_items_change[ctrl][count + 1][2] = call_back
-- end

function this.GetItemNumByName(item_name)
	if this.GetItemBag()[PropConfig.PropMap[item_name].id] then
		return this.GetItemBag()[PropConfig.PropMap[item_name].id]
	else 
		return 0
	end
end

function this.GetItemNumByItemId(item_id)
	if this.GetItemBag()[item_id] then
		return this.GetItemBag()[item_id]
	else
		return 0
	end
end

UserData.watch_item_bag(fake_ctrl, this.OnBagChange)