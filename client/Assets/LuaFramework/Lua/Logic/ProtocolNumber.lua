-- 修改这个定义表，顺序决定编号，新的加在后面保持编号的稳定性, 不要使用小于等于0的编号！主次编号都不能超过65535
require "Logic/ProtocolDefine/ProtocolDefine.lua"
local Container = _G.ProtocolDefine
---------------------------------------
-- 预处理四个表，查找更快一点
local ProtocolStructs_Name = {}
local ProtocolStructs_Id = {}
local Name_To_Id = {}
local Id_To_Name = {}
-- 建立索引
for k,v in ipairs(Container) do
	local id = v.id
	local name = v.name -- .. 'Message'
	local file_name = v.name .. 'Message_pb'
	local messages = v.messages
	require(string.format('protocol/%s', file_name))

	local funcs1 = {}
	local funcs2 = {}
	Name_To_Id[name] = {}
	Id_To_Name[id] = {}
    for k1, v1 in pairs(messages) do
    	local func = _G[file_name][v1]
    	funcs1[v1] = func
    	funcs2[k1] = func
    	Name_To_Id[name][v1] = {id, k1}
    	Id_To_Name[id][k1] = {name, v1}
    end
    ProtocolStructs_Name[name] = funcs1
    ProtocolStructs_Id[id] = funcs2
end

function GetProtocolStructById(main_id, sec_id)
	return ProtocolStructs_Id[main_id][sec_id]()
end

-- main_name 不带_pb后缀
function GetProtocolStructByName(main_name, sec_name)
	-- body
	return ProtocolStructs_Name[main_name][sec_name]()
end

function GetProtocolStructs_Name()
	-- body
	return ProtocolStructs_Name
end

-- 其实大多数时候并不需要反向查找
function GetProtocolIdByName( main_name, sec_name )
	local tmp = Name_To_Id[main_name][sec_name]
	return tmp[1], tmp[2]
end

function GetProtocolNameById( main_id, sec_id )
	local tmp = Id_To_Name[main_id][sec_id]
	return tmp[1], tmp[2]
end

function GetProtocolIdTableByName(main_name)
	return Name_To_Id[main_name]
end