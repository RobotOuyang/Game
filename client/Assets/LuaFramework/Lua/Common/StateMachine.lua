module("StateMachine", package.seeall)

local State = {}
local Machine = {}
State.__index = State
Machine.__index = Machine

function State:Enter( reason )
	
end

function State:Update( update_frame )
	
end

function State:Refresh( reason )
	-- body
end

function State:Exit( reason )
	-- body
end

function State:GetNextStateName( reason )
	for k,v in ipairs(self._transitions) do
		if v[1] == reason then
			return v[2]
		end
	end
	return nil
end

function State:SetMachine( machine )
	self._machine = machine
end

function State:PopEvent( event_name )
	return self._machine:PopEvent( event_name )
end

function State:AddEvent( event_name, event_data )
	return self._machine:AddEvent( event_name, event_data )
end

function State:Get( name )
	return self._machine[name]
end

function State:Trigger( reason )
	return self._machine:Trigger( reason )
end

function State:AddTransition( reason, to_state_name )
	for k,v in ipairs(self._transitions) do
		if v[1] == reason then
			error(string.format("state %s already have transition %s", self._name, reason))
			return
		end
	end
	table.insert(self._transitions, {reason, to_state_name})
end

function Machine:AddState( state )
	table.insert(self._state_list, state)
end

function Machine:GetStateByName( state_name )
	local  next_state
	for k,v in ipairs(self._state_list) do
		if v.name == state_name then
			if next_state then
				logWarn('重复的状态：' .. state_name)
			end
			next_state = v
		end
	end
	return next_state
end

function Machine:Trigger( reason )
	local next_state_name
	local next_state
	if self._cur_state then
		next_state_name = self._cur_state.GetNextStateName(reason)
	end
	next_state = self:GetStateByName(next_state_name)

	if next_state then
		self._update_frame = 1
		if self._cur_state ~= next_state then
			self._cur_state:Exit(reason)
			self._cur_state = next_state
			next_state:Enter(reason)
		else
			self._cur_state:Refresh(reason)
		end
	end
end

function Machine:Set( name, val )
	self[name] = val
end

-- 默认同一个事件只有一个
function Machine:AddEvent( event_name, event_data )
	self._event_list[event_name] = event_data
end

function Machine:PopEvent( event_name )
	local event_data = self._event_list[event_name]
	self._event_list[event_name] = nil
	return event_data
end

function NewState( name, transitions )
	local self = {}
	setmetatable(self, State)
	self._name = name
	self._transitions = {}

	for k,v in ipairs(transitions) do
		self:AddTransition(v[1], v[2])
	end
	return self
end

-- 传入一个协程实例，用于Update这条机器, state_list是state数组，begin_state
function NewMachine( coro, update_interval, state_list, begin_state_name )
	local self = {}
	setmetatable(self, Machine)
	self._coro = coro
	self._state_list = state_list
	self._event_list = {}  --不立即驱动状态机，由状态自己消费的事件列表
	self._update_frame = 1

	for k,v in ipairs(state_list) do
		v:SetMachine(self)
		if v.name == begin_state_name then
			self._cur_state = v
		end
	end

	if not self._cur_state then
		error(begin_state_name .. " is not in state_list")
		return
	end

	self._cur_state:Enter()
	coro.start(function ()
		while true do
			coro.wait(update_interval)
			self._cur_state:Update(self._update_frame)
			self._update_frame = self._update_frame + 1
		end
	end)

	return self
end