module("EditorConf", package.seeall)

local ConfForLua = LuaFramework.ConfForLua

-- data { name1 = { cur_val, set_func }, name2 = { cur_val, set_func }  }
-- obj, 用於添加組件，給策劃調
function BindConf( obj, data )
	local conf = ConfForLua.BindConf(obj, function (name, val)
		data[name][2](val)
	end)
	for k,v in pairs(data) do
		conf:AddDoubleValue(k, v[1])
	end
end

-- EditorConf.BindConf(panel.obj_quick_start, {
-- 	["綫條1"] = { 3, function ( val ) print(val) end },
-- 	["綫條2"] = { 5, function ( val ) print(val) end },
-- })