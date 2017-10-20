local Queue = class("Queue")

function Queue:ctor(capacity)
	self.capacity = capacity + 1
    self.queue = {}
    self.size_ = 0
    self.head = -1
    self.rear = -1
end
 
function Queue:enQueue(element)
	if self.size_ == 0 then
        self.head = 0
        self.rear = 1
        self.size_ = 1
        self.queue[self.rear] = element
    else
        local temp = (self.rear + 1) % self.capacity
        if temp == self.head then
            return false
        else
            self.rear = temp
        end
 		self.queue[self.rear] = element
        self.size_ = self.size_ + 1
    end
    return true
end
 
function Queue:deQueue()
    if self:isEmpty() then
        return
    end
    self.size_ = self.size_ - 1
    self.head = (self.head + 1) % self.capacity
    local value = self.queue[self.head]
    return value
end

function Queue:getByIndex(index)
    if self:isEmpty() then
        return
    end
    local value = self.queue[(self.head + index) % self.capacity]
    return value
end
 
function Queue:clear()
    self.queue = {}
    self.size_ = 0
    self.head = -1
    self.rear = -1
end
 
function Queue:isEmpty()
    if self:size() == 0 then
        return true
    end
    return false
end
 
function Queue:size()
    return self.size_
end
 
return Queue