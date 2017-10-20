--------------------------------------------------------------------------------
--      Copyright (c) 2015 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------

-- 注意，这个协程是全局的，可能在唤醒某个程序时访问了已销毁的gameobject，改善这个问题， 目前部分界面用的1
-- 1：每个单例界面自己维护协程，界面关闭，协程不启动，但是未完成的协程可能导致状态混乱，需要reset
-- 2：每个界面做成实例，也自己维护协程，界面销毁了，所有的东西一起销毁。
-- 3：协程不报错。。。。报个警告啥的。
-- 4: 发现lua是可以判断物体是否被销毁的，加了个IsNil函数, 但是每次yield后都要判断，十分不爽

local create = coroutine.create
local running = coroutine.running
local resume = coroutine.resume
local yield = coroutine.yield
local status = coroutine.status

local error =  	function ( ... ) 
					error(..., 2) 
				end -- error暂时啥也不做

local comap = {}
local parent = {}

-- 反正维护了索引，先不设置为弱表了
-- setmetatable(comap, {__mode = "k"}) 	-- 弱表不能带v，value里面有计时器，只有一份引用
-- setmetatable(parent, {__mode = "k"})  -- 弱表不能带v，wait_cro只有一份引用保存在value里

function coroutine.new_local_cro()
	-- body
	local all_co = {}
	setmetatable(all_co, {__mode = "kv"})
	local my_cro = {}

	my_cro.start = function (f, ... )
		-- body
		local co = coroutine.start(f, ...)
		table.insert(all_co, co)
		return co
	end

	my_cro.stop_all = function ()
		-- body
		for k,v in pairs(all_co) do
			coroutine.stop(v)
		end
		all_co = {}
		setmetatable(all_co, {__mode = "kv"})
	end

	setmetatable(my_cro, {__index = coroutine})
	return my_cro
end

-- 这个start不会交出控制权，而下面几个会
function coroutine.start(f, ...)	
	local co = create(f)
	local flag, msg = resume(co, ...)
	
	if not flag then		
		msg = debug.traceback(co, msg)					
		error(msg)				
	end	
	return co
end

-- by kingbird, 阻塞性协程的递归返回
local function CheckParent( co )
	while status(co) == 'dead' do
		if comap[co] then
			comap[co]:Stop()
		end
		comap[co] = nil
		if parent[co] then
			local _flag, _msg = resume(parent[co])
	
			if not _flag then											
				_msg = debug.traceback(parent[co], _msg)					
				error(_msg)
				return
			end
			local _co = co
			co = parent[co]
			parent[_co] = nil
		else
			break
		end
	end
end

function coroutine.wait(t, co, ...)
	local args = {...}
	co = co or running()		
	local timer = nil
		
	local action = function()				
		local flag, msg = resume(co, unpack(args))
		
		if not flag then	
			timer:Stop()
			msg = debug.traceback(co, msg)							
			error(msg)			
			return
		end
		CheckParent(co)
	end
	
	timer = CoTimer.New(action, t, 1)
	comap[co] = timer	
	timer:Start()
	return yield()
end

-- 等一个协程结束, by kingbird, 用于实现协程内的阻塞性协程
function coroutine.wait_cro(co)
	if co == nil then return end
	local myco = running()
	if myco == nil then
		local msg = debug.traceback(myco, 'run wait_cro in a coroutine!')
		error(msg)
		return
	end

	if status(co) == 'dead' then
		return
	else
		parent[co] = myco
		return yield()
	end
end

function coroutine.wait_cro_list(co_list)
	if co_list == nil or #co_list == 0 then return end
	local myco = running()
	if myco == nil then
		local msg = debug.traceback(myco, 'run wait_cro in a coroutine!')
		error(msg)
		return
	end
	for _, co in pairs(co_list) do
		if status(co) ~= 'dead' then
			parent[co] = myco
			yield()
		end
	end
end

function coroutine.step(t, co, ...)
	local args = {...}
	co = co or running()		
	local timer = nil
	
	local action = function()						
		local flag, msg = resume(co, unpack(args))
	
		if not flag then							
			timer:Stop()					
			msg = debug.traceback(co, msg)					
			error(msg)
			return
		end
		CheckParent(co)
	end
				
	timer = FrameTimer.New(action, t or 1, 1)
	comap[co] = timer
	timer:Start()
	return yield()
end

function coroutine.www(www, co)			
	co = co or running()			
	local timer = nil			
			
	local action = function()				
		if not www.isDone then		
			return		
		end		
				
		timer:Stop()		
		local flag, msg = resume(co)		
			
		if not flag then						
			msg = debug.traceback(co, msg)						
			error(msg)			
			return		
		end		
		CheckParent(co)
	end		
					
	timer = FrameTimer.New(action, 1, -1)	
	comap[co] = timer	
 	timer:Start()
 	return yield()
end

function coroutine.stop(co)
	if not co then return end
 	local timer = comap[co]

 	if timer ~= nil then
 		comap[co] = nil
 		timer:Stop() 		
 	end

 	if parent[co] then
 		coroutine.stop(parent[co])
 	end
 	parent[co] = nil
end
