
local EventLib = require "eventlib"

local Event = {}
local events = {}

-- str, function
function Event.AddListener(event,handler)

	if not events[event] then
		events[event] = EventLib:new(event)
	end
	--conn this handler
	events[event]:Connect(handler)
end

function Event.Brocast(event,...)
	if not events[event] then
		logWarn("brocast " .. event .. " has no event.")
	else
		events[event]:Fire(...)
	end
end

function Event.RemoveListener(event,handler)
	if not events[event] then
		logWarn("remove " .. event .. " has no event.")
	else
		events[event]:Disconnect(handler)
	end
end

return Event