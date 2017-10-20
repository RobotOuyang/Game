module('CHECK_MEM', package.seeall)

-- 需要忽略的table
local dis_table = {
	AccountManager=true,
	Base64=true,
	LevelConfig = true,
	GoldenSharkConfig = true,
	GetProtocolStructById = true,
	ProtocolDefine = true,
	GetProtocolNameById = true,
	OddsTable = true,
	Main = true,
}

local all_table = {}
setmetatable(all_table, {__mode = 'kv'})
local function FindGrowingTable( obj, key )
	-- body
	if all_table[obj] then return end

	if type(obj) == 'table' then
		all_table[obj] = key
		for k,v in pairs(obj) do
			if (type(v) == 'table' or type(v) == 'function') and not dis_table[k] then
				FindGrowingTable(v, string.format('%s/%s', key, k))
			end
		end
		return FindGrowingTable(getmetatable(obj), key..'(meta)')
	elseif type(obj) == 'function' then
		all_table[obj] = key
		local uvIndex = 1
        while true do  
            local name, value = debug.getupvalue(obj, uvIndex)  
            if name == nil then  
                break  
            end
            uvIndex = uvIndex + 1 
            if (type(value) == 'table' or type(value) == 'function') and not dis_table[name] then 
            	FindGrowingTable(value, string.format('%s(%s)', key, name))
            end
        end
        return FindGrowingTable(getfenv(obj), key..'(env)')
	end
end

function CheckMem( ... )
		coroutine.start(function ( )
		while true do
			coroutine.wait(5)
			all_table = {}
			setmetatable(all_table, {__mode = 'kv'})

			local max_table = {}
			FindGrowingTable(_G, '_G')

			for k,v in pairs(all_table) do
				if type(k) == 'table' then
					local count = 0
					for _k, _c in pairs(k) do count = count + 1 end
					max_table[#max_table + 1] = {v, count}
				end
			end
			table.sort( max_table, function ( a,b )
				return a[2] > b[2]
			end )
			local str_cons = string.format("Lua内存为: %.2fMb C#-Lua 对象有%d个 对象池对象有%d个\n", 
				collectgarbage("count") / 1000, Util.GetLuaObjectCount(), panelMgr:GetCacheCount() + resMgr:GetCacheCount())
			for i = 1, 10 do
				str_cons = str_cons .. string.format('%s %d\n',max_table[i][1], max_table[i][2])
			end
			log(str_cons)
		end
	end)
end

