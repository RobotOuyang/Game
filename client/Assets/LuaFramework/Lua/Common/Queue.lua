local Queue = {}

function Queue:New(init_list)
	local new_queue = init_list or {}
	self.__index = self
	setmetatable(new_queue, self)
	return new_queue
end

function Queue:Push(node)
	table.insert(self, node)
end

function Queue:PushHead( node )
	table.insert(self, 1, node)
end

function Queue:Pop()
	if #self > 0 then
		local first_node = self[1]
		table.remove(self, 1)
		return first_node
	else
		return nil
	end
end

function Queue:PopTail(  )
	if #self > 0 then
		local last_node = self[#self]
		table.remove(self, #self)
		return last_node
	else
		return nil
	end
end

function Queue:GetByIndex( index )
	return self[index]
end

function Queue:Slice( from_index, to_index )
	if from_index <= 0 or from_index > #self or to_index <= 0 or to_index > #self then return end
	local result = {}
	for i = from_index, to_index do
		table.insert(result, self[i])
	end
	return result
end

function Queue:ReplaceBlock( from_index, to_index, replace_tab )
	if from_index <= 0 or from_index > #self or to_index <= 0 or to_index > #self then return end
	for i = from_index, to_index do
		self[i] = replace_tab[i - from_index + 1]
	end
end

function Queue:DeleteBlock( from_index, to_index )
	if from_index <= 0 or from_index > #self or to_index <= 0 or to_index > #self then return end
	for i = 1, to_index - from_index + 1 do
		table.remove(self, from_index)
	end
end

function Queue:InsertBlock( from_index, insert_tab )
	for i = #insert_tab, 1, -1 do
		table.insert(self, from_index, insert_tab[i])
	end
end

function Queue:Empty()
	return #self == 0
end

function Queue:Show()
	for k,v in ipairs(self) do
		print(v)
	end
end

function Queue:Length()
	return #self
end

return Queue