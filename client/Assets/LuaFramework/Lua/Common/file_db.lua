-- 本地文件存储
local json = require "cjson"
--json lua 互转有一个先天bug， lua里类似 {1,2,3, a=4}这种会被json转成map，再转回lua的时候，1,2,3变成了字符串的key， 为了避免出错，尽量避免使用数字字母混搭的lua table

module('FILE_DB', package.seeall)

-- 这是lua序列化和反序列化成json
function LuaToJson( val )
	return json.encode(val)
end

function JsonToLua( str )
	-- body
	return json.decode(str)
end

-- 下面是lua的 table直接序列化反序列化成lua的, 不支持userdata
function Serialize(t)
	if type(t) ~= "table" then
		return
	end
	
	local mark = {}
	local assign = {}
	
	local function ser_table(tbl, parent, ident)
		mark[tbl] = parent
		local tmp = {}

		for k,v in pairs(tbl) do
			local key= type(k) == "number" and "["..k.."]" or "[\"" .. k .. "\"]"
			if type(v) == "table" then
				local dotkey= parent..(type(k) == "number" and key or "."..key)
				if mark[v] then
					table.insert(assign, dotkey.." = "..mark[v])
				else
					table.insert(tmp, ident..key.." = "..ser_table(v, dotkey, ident..'  '))
				end
			elseif type(v) == "string" then
				table.insert(tmp, ident..key..' = "'..v..'"')
			else
				table.insert(tmp, ident..key.." = "..tostring(v))
			end
		end

		return "{\n"..table.concat(tmp,",\n")..ident.."\n}";
	end
 
	local solid_data = "do\nlocal ret ="..ser_table(t,"ret", '  ')..'\n'..table.concat(assign," ").."\nreturn ret\nend\n"
	return solid_data
end

function file_load(filename)
    local file
    if filename == nil then
        file = io.stdin
    else
        local err
        file, err = io.open(filename, "rb")
        if file == nil then
            logWarn(("Unable to read '%s': %s"):format(filename, err))
            return nil
        end
    end
    local data = file:read("*a")

    if filename ~= nil then
        file:close()
    end

    return data
end

function file_save(filename, data)
    local file
    if filename == nil then
        file = io.stdout
    else
        local err
        file, err = io.open(filename, "wb")
        if file == nil then
            logWarn(("Unable to write '%s': %s"):format(filename, err))
            return
        end
    end
    file:write(data)
    if filename ~= nil then
        file:close()
    end
end

-- 暂时用lua存 不用json存, 路径默认为 Util.DataPath 下
local db_loaded = {} -- 已经读取过的文件数据库的缓存，防止重复读入数据不一致
function PersistFileDB(path)
	-- local str = LuaToJson(table)
	local table = db_loaded[path] 
	local str = Serialize(table)
	file_save(Util.DataPath..path, str)
end

function ClearFileDB( path )
	db_loaded[path] = {}
	local str = Serialize({})
	file_save(Util.DataPath..path, str)
end

function LoadFileDB( path )
	-- 取过一次后，就只写不读了。保证数据一致性
	if db_loaded[path] then
		return db_loaded[path] 
	end
	local str = file_load(Util.DataPath..path)
	if str == nil then
		db_loaded[path] = {}
	else
		local _func = loadstring(str)
		if not _func then
			db_loaded[path] = {}
			logError("读取配置文件失败 ： " .. path)
		else
			db_loaded[path] = loadstring(str)()
			if not db_loaded[path] then
				db_loaded[path] = {}
			end
		end
	end
	return db_loaded[path]
end



--test
-- local tab = {21,23,34534,56,456,5467, aaa=3, {324,3456,567,1}}
-- tab.b = tab[7]

-- local json_str = FILE_DB.LuaToJson( tab )
-- local lua_str = FILE_DB.Serialize(tab)

-- print(json_str)
-- print(lua_str)
-- print(TableToStr(loadstring(lua_str)()))

-- FILE_DB.PersistFileDB('__file_db__', tab)
-- tab = FILE_DB.LoadFileDB('__file_db__')

-- print(TableToStr(tab))