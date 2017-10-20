-- 不會創建銷毀，也不會改變父親，也不會改變active的對象池
module('ObjectPool', package.seeall)

local _ObjectPool = {}
_ObjectPool.__index = _ObjectPool

-- 新建一个对象池，父亲为transform，预制体为prefab, 物体的初始和安息位置为init_pos
function New( transform, init_pos )
	local self = {}
	setmetatable(self, _ObjectPool)

	self.root = GameObject.New('ObjectPool').transform
	self.root:SetParent(transform, false)
	self.root.localScale = Vector3.one
	self.root.localPosition = init_pos
	self.obj_pool = {}
	self.serach_pool = {}

	return self
end

-- 在出生点创建或者获得一个物体
function _ObjectPool:Get( prefab )
	local collect = self.obj_pool[prefab]
	local obj
	if collect and #collect > 0 then
		obj = collect[#collect]
		collect[#collect] = nil
	else
		-- 这里不传true，不用对象池了，应用场景不一样
		obj = panelMgr:CreateEffect(self.root, prefab)
	end
	self.serach_pool[obj] = prefab
	return obj
end

-- 将一个物体返回出生点
function _ObjectPool:Collect( object )
	local prefab = self.serach_pool[object]
	self.serach_pool[object] = nil

	if prefab then
		local collect = self.obj_pool[prefab]
		if collect then
			collect[#collect + 1] = object
		else
			self.obj_pool[prefab] = { object }
		end
		-- 归到root上
		object.transform.localPosition = Vector3.zero
	else
		destroy(object)
	end
end

function _ObjectPool:Destroy( )
	-- 直接摧毁父亲就可以了
	destroy(self.root)
	self.root = nil
	self.obj_pool = nil
	self.serach_pool = nil
end