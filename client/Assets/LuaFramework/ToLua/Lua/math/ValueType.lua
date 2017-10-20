--------------------------------------------------------------------------------
--      Copyright (c) 2015 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------

local getmetatable = getmetatable
local Vector3 = Vector3
local Vector2 = Vector2
local Vector4 = Vector4
local Quaternion = Quaternion
local Color = Color
local Ray = Ray
local Bounds = Bounds
local Touch = Touch
local LayerMask = LayerMask
local RaycastHit = RaycastHit

local ValueType = 
{
	None = 0,
	Vector3 = 1,
	Quaternion = 2,
	Vector2 = 3,
	Color = 4,
	Vector4 = 5,
	Ray = 6,
	Bounds = 7,
	Touch = 8,
	LayerMask = 9,
	RaycastHit = 10,
}

-- by kingbird, 将原先的逐个对比优化成直接获取类型
function GetLuaValueType(udata)	
	local meta = getmetatable(udata)
	if not meta then return 0 end

	local var = rawget(meta, 'v_type')
	if var == nil then
		return 0
	else
		return var
	end
end
