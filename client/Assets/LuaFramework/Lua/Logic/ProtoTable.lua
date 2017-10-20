module('ProtoTable', package.seeall)

-- val 的类型定义，1：number， 2：str， 3：table
local function _get_type( val )
	if type(val) == 'number' then
		return '1'
	elseif type(val) == 'string' then
		return '2'
	elseif type(val) == 'table' then
		return '3'
	else
		return '0'
	end
end

local val_split_str = "|" -- 这个连接符自己改
local function _build_path(parent, cur)
	if not parent then return cur end
	return parent.."@"..cur   -- 这个连接符自己改
end

local function get_redis_Val( key )

	-- test
	if key == "TestRedis" then return '3|a|b'
	elseif key == "TestRedis@a" then return '1|3241'
	elseif key == 'TestRedis@b' then return '3|c|d'
	elseif key == 'TestRedis@b@c' then return '2|king'
	elseif key == 'TestRedis@b@d' then return '3'
	end

	return nil  -- 这里要能根据某个key返回值
end

local function save_to_redis( key, val )
	-- 这个函数要根据key，val写redis
	print(key, val)
end

-- 用指定的数据data_table， 为tab添加meta_table
local function AddMetaTable( tab, tab_name, data_table )
	local table_meta = {}

	local function _save_val( key, val)
		if type(val) == 'table' then
			local save_str = {_get_type(val)}
			-- 遍历，这个不知道有没有简单方法
			local val_tab = getmetatable(val).data_table
			for k,v in pairs(val_tab) do
				_save_val(_build_path(key, k), v)
				table.insert(save_str, k)
			end
			save_to_redis(key, table.concat(save_str, val_split_str))
		else	
			save_to_redis(key, _get_type(val)..val_split_str..tostring(val))
		end
	end

	-- 这个函数会对ret进行填充val，如果val是个table，甚至是嵌套，会循环构造meta
	local function _build_val_table( ret ,key, val, parent_key )
		if type(val) == 'table' then
			local _data_build = {}
			local _full_path = _build_path(parent_key, key)
			for k,v in pairs(val) do
				_build_val_table(_data_build, k, v, _full_path)
			end 
			-- 先设值再设meta，免得触发写redis, 对于嵌套的table，肯定也是里面的先完成赋值
			local out_tab = {}
			AddMetaTable(out_tab, _full_path, _data_build)
			ret[key] = out_tab
		else
			ret[key] = val
		end
	end

	local function _setter( t, key, val )
		local full_path = _build_path(table_meta.tab_name,key)
		local is_new_key = not table_meta.data_table[key]

		_build_val_table(table_meta.data_table, key, val, table_meta.tab_name)
		-- 保存该值
		_save_val(full_path, table_meta.data_table[key], is_new_key)
		-- 新增加的key的话，要保存下父亲的key列表
		local save_str = {_get_type(table_meta.data_table)}
		for k,v in pairs(table_meta.data_table) do
			table.insert(save_str, k)
		end
		save_to_redis(table_meta.tab_name, table.concat(save_str, val_split_str))
	end

	table_meta.data_table = data_table or {}
	table_meta.tab_name = tab_name
	table_meta.__index = table_meta.data_table
	table_meta.__newindex = _setter

	setmetatable(tab, table_meta)
end

local function string_split(str, split_char)
    local sub_str_tab = {};
    while true do
        local pos = string.find(str, split_char);
        if not pos then
            sub_str_tab[#sub_str_tab + 1] = str;
            break;
        end
        local sub_str = string.sub(str, 1, pos - 1);
        sub_str_tab[#sub_str_tab + 1] = sub_str;
        str = string.sub(str, pos + 1, #str);
    end
    return sub_str_tab;
end

--------------------------------
---------下面为对外接口----------
function FillProtobuffer(proto_buf, my_table)
	local val_tab = getmetatable(my_table).data_table
	if not proto_buf or not val_tab then
		return
	end

	for k,v in pairs(val_tab) do
		if type(v) == 'table' then
			FillProtobuffer(proto_buf[k], v)
		else
			proto_buf[k] = v
		end
	end
end

function InitFromRedis(name)
	local val = get_redis_Val(name)
	local cons = string_split(val, val_split_str)

	local function build_val_table( ret ,key, cons, parent_key )
		if cons[1] == '1' then
			ret[key] = tonumber(cons[1])
		elseif cons[1] == '2' then
			ret[key] = table.concat(cons, val_split_str, 2)
		elseif cons[1] == '3' then
			local _data_build = {}
			local _full_path = _build_path(parent_key, key)
			for i = 2, #cons do
				local _val = get_redis_Val(_build_path(_full_path, cons[i]))
				local _cons = string_split(_val, val_split_str)
				build_val_table(_data_build, cons[i], _cons, _full_path)
			end 
			-- 先设值再设meta，免得触发写redis, 对于嵌套的table，肯定也是里面的先完成赋值
			local out_tab = {}
			AddMetaTable(out_tab, _full_path, _data_build)
			ret[key] = out_tab
		else
			error("什么玩意？")
		end
	end

	local ret = {}
	build_val_table(ret, name, cons)
	if type(ret[name]) ~= 'table' then
		error('只能从redis初始化一个table')
	end
	return ret[name]
end


function New(name)
	local ret = {}
	AddMetaTable(ret, name)
	return ret
end


-- test
local Player = InitFromRedis('TestRedis')
Player.b.test = 1

local Player = New('Player')
-- 切记，不能直接对Player赋值，否则会覆盖掉所有meta信息

Player.ret = {code = 1113, msg = 'just a joke!'}
Player.player = {
		id = 36321,
		account = {username = 'kingbird'},
		character = {gold =0, level = 1},
		module = {
			comic_indiana = {
				current_gold = 9999,
				level = 2,
				bricks_left = 45,
				last_bet_line = 5,
				last_bet_score = 100,
			}
		}
}
Player.player.module.test = {}

local data_struct = GetProtocolStructById(100, 2)
FillProtobuffer(data_struct, Player)
print(tostring(data_struct))