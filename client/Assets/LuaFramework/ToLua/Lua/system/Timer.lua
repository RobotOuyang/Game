--------------------------------------------------------------------------------
--      Copyright (c) 2015 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------

Timer = 
{
	time	 = 0,
	duration = 1,
	loop	 = 1,
	running	 = false,
	scale	 = false,
	func	 = nil,	
}

local Time = Time
local mt = {}
mt.__index = Timer

--scale false 采用deltaTime计时，true 采用 unscaledDeltaTime计时
function Timer.New(func, duration, loop, scale)
	local timer = {}
	scale = scale or false and true
	setmetatable(timer, mt)	
	timer:Reset(func, duration, loop, scale)
	return timer
end

function Timer:Start()
	self.running = true
	UpdateBeat:Add(self.Update, self)
end

function Timer:Reset(func, duration, loop, scale)
	self.duration 	= duration
	self.loop		= loop or 1
	self.scale		= scale
	self.func		= func
	self.time		= duration
	self.running	= false
	self.count		= Time.frameCount + 1
end

function Timer:Stop()
	self.running = false
	UpdateBeat:Remove(self.Update, self)
end

function Timer:Update()
	if not self.running then
		return
	end
	
	local delta = self.scale and Time.deltaTime or Time.unscaledDeltaTime	
	self.time = self.time - delta
	
	if self.time <= 0 and Time.frameCount > self.count then
		self.func()
		
		if self.loop > 0 then
			self.loop = self.loop - 1
			self.time = self.time + self.duration
		end
		
		if self.loop == 0 then
			self:Stop()
		elseif self.loop < 0 then
			self.time = self.time + self.duration
		end
	end
end

--给协同使用的帧计数timer
FrameTimer = 
{	
	count  		= 1,		
	duration	= 1,
	loop		= 1,
	func		= nil,	
	state	 	= 0,  -- 0：默认，1：等待激活，下一阵开始转running 2：running  3：dead
	-- state的原则是 本帧新起的协程 不会在本帧 update，最少要到下一帧，而本帧stop的协程，会立即生效
}


local mt2 = {}
mt2.__index = FrameTimer

local table_insert = table.insert
local frame_timer_list = {}

local function UpdateFrameTimer()
	for k,v in ipairs(frame_timer_list) do
		if v.state == 1 then
			v.state = 2
		end
	end
	for k,v in ipairs(frame_timer_list) do
		if v.state == 2 then
			v:Update()
		end
	end
	local next_list = {}
	for k,v in ipairs(frame_timer_list) do
		if v.state ~= 3 then
			table_insert(next_list, v)
		end
	end
	frame_timer_list = next_list
end
CoUpdateBeat:Add(UpdateFrameTimer)


-- 这个timer和 unity本身的一样，即使你给count为0，他也至少要到下一帧才能执行
function FrameTimer.New(func, count, loop)
	local timer = {}
	setmetatable(timer, mt2)	
	timer.count = Time.frameCount + count
	timer.duration = count
	timer.loop	= loop
	timer.func	= func
	timer.state = 0
	table_insert(frame_timer_list, timer)
	return timer
end

function FrameTimer:Start()
	-- 状态2 running 不需要改变，状态4，无法改变
	if self.state ~= 2 and self.state ~= 3 then
	 	self.state = 1
	end
end

function FrameTimer:Stop()
	-- 不管怎樣 直接dead
	self.state = 3
end

function FrameTimer:Update()	
	if self.state ~= 2 then
		return
	end
	
	if Time.frameCount >= self.count then
		self.func()	
		
		if self.loop > 0 then
			self.loop = self.loop - 1
		end
		
		if self.loop == 0 then
			self:Stop()
		else
			self.count = Time.frameCount + self.duration
		end
	end
end

CoTimer = 
{
	time	 = 0,
	duration = 1,
	loop	 = 1,
	running	 = false,	
	func	 = nil,	
}

local mt3 = {}
mt3.__index = CoTimer

local co_timer_list = {}

local function UpdateCoTimer()
	for k,v in ipairs(co_timer_list) do
		if v.state == 1 then
			v.state = 2
		end
	end
	for k,v in ipairs(co_timer_list) do
		if v.state == 2 then
			v:Update()
		end
	end
	local next_list = {}
	for k,v in ipairs(co_timer_list) do
		if v.state ~= 3 then
			table_insert(next_list, v)
		end
	end
	co_timer_list = next_list
end
CoUpdateBeat:Add(UpdateCoTimer)


function CoTimer.New(func, duration, loop)
	local timer = {}
	setmetatable(timer, mt3)	
	timer:Reset(func, duration, loop)
	table_insert(co_timer_list, timer)
	return timer
end

function CoTimer:Start()
		-- 状态2 running 不需要改变，状态4，无法改变
	if self.state ~= 2 and self.state ~= 3 then
	 	self.state = 1
	 	self.count = Time.frameCount + 1
	end
end

function CoTimer:Reset(func, duration, loop)
	self.duration 	= duration
	self.loop		= loop or 1	
	self.func		= func
	self.time		= duration
	self.state	= 0
	self.count		= Time.frameCount + 1
end

function CoTimer:Stop()
	-- 不管怎樣 直接dead
	self.state = 3
end

function CoTimer:Update()
	if self.state ~= 2 then
		return
	end		
	
	if self.time <= 0 and Time.frameCount > self.count then
		self.func()		
		
		if self.loop > 0 then
			self.loop = self.loop - 1
			self.time = self.time + self.duration
		end
		
		if self.loop == 0 then
			self:Stop()
		elseif self.loop < 0 then
			self.time = self.time + self.duration
		end
	end

	self.time = self.time - Time.deltaTime
end