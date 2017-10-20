local _M = { }

function _M:new(name)
    assert(self ~= nil and type(self) == "table" and self == _M, "Invalid EventLib table (make sure you're using ':' not '.')")
    local s = { }
    s.handlers = { }
    s.EventName = name or "<Unknown Event>"
    return setmetatable(s, { __index = self })
end
_M.CreateEvent = _M.new

function _M:Connect(handler)
    table.insert(self.handlers, handler)
    local t = { }
    t.Disconnect = function()
        return self:Disconnect(handler)
    end
    return t
end

function _M:Disconnect(handler)
    if not handler then
        self.handlers = { }
    else
        for k, v in pairs(self.handlers) do
            if v == handler then
                self.handlers[k] = nil
                return k
            end
        end
    end
end

function _M:DisconnectAll()
    self:Disconnect()
end

function _M:Fire(...)
    for k, v in pairs(self.handlers) do
        v(...) 
    end
end

function _M:ConnectionCount()
    return #self.handlers
end

function _M:Destroy()
    self:DisconnectAll()
    for k, v in pairs(self) do
        self[k] = nil
    end
    setmetatable(self, { })
end

return _M
