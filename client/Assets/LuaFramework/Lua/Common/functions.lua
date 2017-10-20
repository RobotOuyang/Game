-- 这里面都是直接加到全局的函数
local show_log = AppConst.DebugMode
-- 有些object在c#层被销毁了，但是lua不知道
function IsNil(uobj)
    return uobj == nil or uobj:Equals(nil)
end

--输出日志--
function log(str)
    if show_log then Util.Log(str) end;
end

--错误日志--
function logError(str) 
	if show_log then Util.LogError(str) end;
end

--警告日志--
function logWarn(str) 
	if show_log then Util.LogWarning(str) end;
end

--查找对象--
function find(str)
	return GameObject.Find(str);
end

function destroy(obj)
	GameObject.Destroy(obj);
end

function newObject(prefab)
	return GameObject.Instantiate(prefab);
end

function clearTableExceptFunc( table )
	for k, v in pairs(table) do
		if type(v) ~= 'function' then
			table[k] = nil
		end
	end
end

--创建面板--
function createPanel(name, func)
	panelMgr:CreatePanel(name, func);
end

function child(str)
	return transform:Find(str);
end

function subGet(childNode, typeName)		
	return child(childNode):GetComponent(typeName);
end

function findPanel(str) 
	local obj = find(str)
	if obj == nil then
		error(str.." is null")
		return nil
	end
	return obj:GetComponent("BaseLua")
end

function string_split(str, split_char)
    local sub_str_tab = {}
    while true do
        local pos = string.find(str, split_char, nil, true)
        if not pos then
            sub_str_tab[#sub_str_tab + 1] = str
            break
        end
        local sub_str = string.sub(str, 1, pos - 1)
        sub_str_tab[#sub_str_tab + 1] = sub_str
        str = string.sub(str, pos + 1, #str)
    end
    return sub_str_tab
end

function add_char_into_chinese(str, char)
	local sub_str_tab = {}
	for index = 1, #str / 3 - 1 do
		table.insert(sub_str_tab, string.sub(str, index * 3 - 2, index * 3))
		table.insert(sub_str_tab, char)
	end
	table.insert(sub_str_tab, string.sub(str, #str - 2, #str))
	return table.concat(sub_str_tab, "")
end

-- 找一个字符串中某个字符串之后且不包含该字符串的字符串
function find_end_string(str, split_char)
	local end_string = str
	while true do
		local pos = string.find(end_string, split_char)
		if not pos then
			break
		end
		end_string = string.sub(end_string, pos + 1, #end_string)
	end
	return end_string
end

-- 找到最后一个匹配
function string.rfind( ... )
	-- body
	local old = string.find(...)
	local ret = -1
	while old > 0 do
		ret = old
		old = string.find(...)
	end
	return ret
end

function table.keys( t )
    local keys = {}
    for k, _ in pairs( t ) do
        keys[#keys + 1] = k
    end
    return keys
end

function print_table( tab )
	log(TableToStr(tab))
end

-- 将lua的table序列化成一个str
function TableToStr(tab)
	local cons = {}

	local function _print_table(tab, indent) 
		if tab == nil then
			table.insert(cons, 'nil')
			return
		end
		if #indent > 10 then
			logError('卧槽，table深度超过10？是不是循环引用了')
			return
		end
		if type(tab) == 'table' then
			table.insert(cons, '{\n')
			for i,v in pairs(tab) do
				local key_string = tostring(i)
				if type(i) == 'string' then
					key_string = string.format('"%s"', key_string)
				end
				if type(v) == "table" then
					table.insert(cons, string.format("%s[%s] = ", indent, key_string))
					_print_table(v, indent .. '  ')
				else
					table.insert(cons, string.format('%s[%s] = %s,\n', indent, key_string, tostring(v)))
				end
			end
			table.insert(cons, indent..'}\n')
		else
			table.insert(cons, string.format("%s%s,\n", indent, tostring(tab)))
		end
	end

	_print_table(tab, '')
	return table.concat(cons)
end

-- 比较两个lua值是否相等
function compare_values(val1, val2)
    local type1 = type(val1)
    local type2 = type(val2)
    if type1 ~= type2 then
        return false
    end

    -- Check for NaN
    if type1 == "number" and val1 ~= val1 and val2 ~= val2 then
        return true
    end

    if type1 ~= "table" then
        return val1 == val2
    end

    -- check_keys stores all the keys that must be checked in val2
    local check_keys = {}
    for k, _ in pairs(val1) do
        check_keys[k] = true
    end

    for k, v in pairs(val2) do
        if not check_keys[k] then
            return false
        end

        if not compare_values(val1[k], val2[k]) then
            return false
        end

        check_keys[k] = nil
    end
    for k, _ in pairs(check_keys) do
        -- Not the same if any keys from val1 were not found in val2
        return false
    end
    return true
end

-- 转成可读的
function get_readable_number( num )
    -- body
    local str = tostring(num)
    local ret = ''
    local index = 3
    while index < #str do
        ret = ','..string.sub(str, -index, -index + 2)..ret
        index = index + 3
    end
    return string.sub(str, 1, -index + 2)..ret
end

function get_space_number( num )
	-- body
	local str = tostring(num)
	local ret = ''
	for index = 1, #str do
		ret = ret .. ' ' .. string.sub(str, index, index)
	end
	return ret
end

-- 判断字符串是否合法
function judge_has_special_char(str, extra_char)
	if type(str) ~= 'string' then
		return false
	end
	extra_char = extra_char or {'#', ' '}
	local function has_value(index)
		for _, val in pairs(extra_char) do
			if string.byte(val) == index then
				return true
			end
		end
		return false
	end
	for k = 1, #str do
		local c = string.byte(str, k)
		if not c then return false end
		if c <= 47 or (58 <= c and c <= 64) or 
			(91 <= c and c <= 96) or (c >= 123 and c <=127) then
			if not has_value(c) then
				return false
			end
		end 
	end
	return true
end

function gen_current_datetime_key()
	return DateTime.Now.Year*400 + (DateTime.Now.Month - 1)*31 + DateTime.Now.Day
end

function string.trim(s)
	return string.gsub(s, "^%s*(.-)%s*$", "%1")
end

function table.has_value(tab, val)
	local function judge_equal(tab1, tab2)
		if type(tab1) ~= "table" or type(tab2) ~= "table" then return tab1 == tab2 end
		local result = true
		for k, v in pairs(tab1) do
			result = result and judge_equal(v, tab2[k])
		end
		return result
	end
	for _, v in pairs(tab) do
		if judge_equal(v, val) then
			return true
		end
	end
	return false
end

function table.sub(tab, start_index, end_index)
	local new_table = {}
	if type(tab) ~= "table" then return new_table end
	for index = start_index, end_index do
		if tab[index] then
			new_table[#new_table + 1] = tab[index]
		else
			break
		end
	end
	return new_table
end

function is_integer(num)
	return math.floor(num) == num
end

-- 将一个数字保留n位有效数字返回,整数则返回原整数(n <= 4)
function retain_float(num, n)
	n = n or 4
	local val = math.pow(10, n - 1)
	local i = 0
	while num < val and i < n do
		num = num * 10
		i = i + 1
	end
	num = math.floor(num)
	for j = 1, i do
		num = num / 10
	end
	return num
end


function get_retain_integer(num, n)
	n = n or 4
	local val = math.pow(10, n - 1)
	local multiplier = 1
	while num < val do
		num = num * 10
		multiplier = multiplier * 10
	end
	num = math.floor(num)
	local divisor = 10
	for i = 1, n - 1 do
		if num % divisor ~= 0 or multiplier < 10 then
			break
		end
		num = num / 10
		multiplier = multiplier / 10
	end
	return num, multiplier
end

function get_player_id_for_show(val)
	return tonumber(val)  -- + 30906102 //todo
end

function check_text_surpass(str, length, func)
	if str and string.len(str) > length then
		if func then func() end
		str = string.sub(str, 1, length)
	end
	return str
end

function get_num_text(num)
	if type(num) ~= 'number' then num = tonumber(num) end
	if num <10000 then
		return num
	elseif num < 100000000 then
		return math.floor(num / 10000) .. '万'
	else
		return math.floor(num / 100000000) .. '亿'
	end
end

function rand_array( len )
	math.randomseed(os.time())
	local array = {}
	for i = 1, len do
		table.insert(array, math.random())
	end
	return array
end

--随机打乱一个数组
function disturb_array(array)
	math.randomseed(os.time())
	local len = #array
	local random_array = rand_array(len)
	local sequence = {}
	for i = 1, len do
		local index = 1
		for j = 1, len do
			if i ~= j then
				if random_array[i] > random_array[j] then
					index = index + 1
				end
			end
		end
		table.insert(sequence, index)
	end
	local result = {}
	for _,v in ipairs(sequence) do
		table.insert(result, array[v])
	end
	return result
end

-- 只对数组插入一个数组有效
function table.insert_table( insert_table, inserted_table )
	if type(insert_table) ~= 'table' or type(inserted_table) ~= 'table' then return end
	for _, item in ipairs(inserted_table) do
		table.insert(insert_table, item)
	end
	return insert_table
end

function UnloadAllRes( input_table )
	if not input_table then return end
	for k,v in ipairs(input_table) do
		if v.type == "texture" then
			resMgr:UnloadAssetBundle('prefabs/textureprefab/' .. v.files[1])
		else
			resMgr:UnloadAssetBundle('prefabs/' .. v.files[1])
		end
	end
end

function LoadAllRes( input_table, callback_func )
	local result = {}
	local all_names = {}
	local len = #input_table
	local loaded = 0

	local function load_one_column( names, objs, index )
		local one_column_result = {}
		if #names == 0 then -- 不传的就是全加载
			for i = 1, objs.Length do
				names[i] = objs[i-1].name
			end
		end
		for k,v in ipairs(names) do
			one_column_result[v] = objs[k-1]
		end

		result[index] = one_column_result
		all_names[index] = names
		loaded = loaded + 1
		if loaded == len then
			callback_func(all_names, result)
		end
	end

	local function load_type( type_str, files, index )
		if type_str == "texture" then
			resMgr:LoadTexture(files[1], files[2], function(objs)
				load_one_column(files[2], objs, index)
			end)
		else
			resMgr:LoadPrefab(files[1], files[2], function(objs)
				load_one_column(files[2], objs, index)
			end)
		end
	end

	for k,v in ipairs(input_table) do
		load_type(v.type, v.files, k)  -- 再封裝一层是为了解决闭包索引k值的问题
	end
end

function LoadSceneWithCtrl( scene_name, ctrl_name, call_back, data)
	local list = CtrlManager.GetAllAwake()
	for k,v in ipairs(list) do
		v.isAwake = false
	end
	-- 将响应关闭一段时间，但是不关闭界面
	resMgr:LoadSceneAsync(scene_name, function() end, function(progress)
		for k,v in ipairs(list) do
			v.isAwake = true
		end
		if call_back then
			call_back()
		else
			CtrlManager.ChangeCtrl(ctrl_name, data)

			if AppConst.DebugMode then
				CtrlManager.Open("SendMessageCtrl")
			end
		end	 
	end)
end

--precision精度
function number_to_str( num, precision )
	local precision = precision or 1
	local multi = math.pow(10, precision)
	if num >= 100000000 then
		local tmp = num / 100000000
		str = tostring(math.floor(tmp*multi + 0.5)*(1/multi)) .. "亿"
	elseif num >= 10000 then
		local tmp = num / 10000
		str = tostring(math.floor(tmp*multi + 0.5)*(1/multi)) .. "万"
	else
		str = tostring(num)
	end
	return str
end

function get_integer_digit(num)
	local digit = 0
	while num >= 10 do
		digit = digit + 1
		num = num / 10
	end
	return digit
end

function os.same_day(first, second)
    local delta_hour = 3
    local first_date = os.date("*t", first - 3600 * delta_hour)
    local second_date = os.date("*t", second - 3600 * delta_hour)
    return first_date.day == second_date.day and first_date.month == second_date.month and first_date.year == second_date.year
end

function os.is_today(timestamp)
	if not timestamp then return false end
	return os.same_day(timestamp, os.time())
end

local json = require "cjson"
function JsonDataToTable( data )
	local json_str = JsonData.ToJson(data)
	local tab = json.decode(json_str)
	return tab
end

function TableToJsonData( tab )
	local json_str = json.encode(tab)
	return JsonMapper.ToObject(json_str)
end