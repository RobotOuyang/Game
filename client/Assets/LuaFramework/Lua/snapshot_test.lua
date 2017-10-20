local snapshot = require "snapshot"

-- Lua内存记录功能
local preLuaSnapshot = nil
function snapshotLuaMemory( file_name )
    -- 首先统计Lua内存占用的情况
    -- print("GC前, Lua内存为:", collectgarbage("count"))
    -- collectgarbage()
    -- print("GC后, Lua内存为:", collectgarbage("count"))
    local curLuaSnapshot = snapshot.snapshot()
    local ret = {}
    local file = io.open(file_name, 'w')
    local count = 0
    if preLuaSnapshot ~= nil then
        for k,v in pairs(curLuaSnapshot) do
            if preLuaSnapshot[k] == nil then
                count = count + 1
                ret[k] = v
            end
        end
    end
 
 	local cons = {}
    for k, v in pairs(ret) do
        table.insert(cons, tostring(k) .. '\t' .. tostring(v))
    end
    file:write('---------------------------------------------------------------\n')
    file:write(table.concat( cons, "\n"))
 	file:close()

    print ("Lua snapshot diff object count is " .. count)
    preLuaSnapshot = curLuaSnapshot
end