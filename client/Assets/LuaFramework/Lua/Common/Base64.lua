
Base64 = {}
local this = Base64

function this.Init()
	local origin_str = "sY+N7PTnwM6bfAg4eaxkzCdhvJlUXZQSF82mDpK5WL/uEo31c0jiRtIHyqBOVGr9"
	this.code = {}
	for i = 1, #origin_str, 1 do
		table.insert(this.code, string.byte(string.sub(origin_str, i, i)))
	end

	this.reverse = {}
	for i = 1, 128, 1 do
		this.reverse[i] = 0
	end

	for i = 1, #this.code, 1 do
		this.reverse[this.code[i]] = i - 1
	end
end

this.Init()

function Base64.Encode( input )
	local input_len = #input
	local result_len = math.floor((input_len + 2) / 3) * 4
	local result = {}
	local index = 1
	local i = 3
	while i <= input_len do
		local tmp1 = base64bit:_lshift(input[i - 2], 16)
		local tmp2 = base64bit:_lshift(input[i - 1], 8)
		local val = base64bit:_or(base64bit:_or(tmp1, tmp2), input[i])
		result[index] = this.code[base64bit:_rshift(val, 18) + 1]
		index = index + 1
		result[index] = this.code[base64bit:_and(base64bit:_rshift(val, 12), 63) + 1]
		index = index + 1
		result[index] = this.code[base64bit:_and(base64bit:_rshift(val, 6), 63) + 1]
		index = index + 1
		result[index] = this.code[base64bit:_and(val, 63) + 1]
		index = index + 1
		i = i + 3
	end
	i = i - 2

	local mod = input_len % 3
	if mod == 1 then
		result[index] = this.code[base64bit:_rshift(input[i], 2) + 1]
		index = index + 1
		result[index] = this.code[base64bit:_and(base64bit:_lshift(input[i], 4), 63) + 1]
		index = index + 1
	elseif mod == 2 then
		result[index] = this.code[base64bit:_rshift(input[i], 2) + 1]
		index = index + 1
		result[index] = this.code[base64bit:_or(base64bit:_and(base64bit:_lshift(input[i], 4), 63), base64bit:_rshift(input[i+1], 4)) + 1]
		index = index + 1
		result[index] = this.code[base64bit:_and(base64bit:_lshift(input[i+1], 2), 63) + 1]
		index = index + 1
	end

	while index <= result_len do
		result[index] = string.byte("=")
		index = index + 1
	end
	return result
end

function Base64.Decode( input )
	local input_len = #input
	local len = math.floor(input_len / 4) * 3
	local tail = 0
	if len > 0 then
		tail = tail + (input[input_len] == string.byte("=") and 1 or 0)
		tail = tail + (input[input_len - 1] == string.byte("=") and 1 or 0)
		len = len - tail
	end
	local result = {}
	local i, index, search_len = 3, 1, input_len - tail
	while i < search_len do
		local tmp1 = base64bit:_lshift(this.reverse[input[i-2]], 18)
		local tmp2 = base64bit:_lshift(this.reverse[input[i-1]], 12)
		local tmp3 = base64bit:_lshift(this.reverse[input[i]], 6)
		local val = base64bit:_or(base64bit:_or(base64bit:_or(tmp1, tmp2), tmp3), this.reverse[input[i + 1]])
		result[index] = base64bit:_rshift(val, 16)
		index = index + 1
		result[index] = base64bit:_and(base64bit:_rshift(val, 8), 255)
		index = index + 1
		result[index] = base64bit:_and(val, 255)
		index = index + 1
		i = i + 4
	end
	i = i - 3
	if tail == 1 then
		local tmp1 = base64bit:_lshift(this.reverse[input[i + 1]], 12)
		local tmp2 = base64bit:_lshift(this.reverse[input[i+2]], 6)
		local val = base64bit:_or(base64bit:_or(tmp1, tmp2), this.reverse[input[i+3]])
		result[index] = base64bit:_rshift(val, 10)
		index = index + 1
		result[index] = base64bit:_and(base64bit:_rshift(val, 2), 255)
		index = index + 1
	elseif tail == 2 then
		val = base64bit:_or(base64bit:_lshift(this.reverse[input[i+1]], 6), this.reverse[input[i+2]])
		result[index] = base64bit:_rshift(val, 4)
		index = index + 1
	end
	return result
end

function Base64.EncodeString( str )
	if not str then return nil end
	local input = {}
	for i = 1, #str, 1 do
		table.insert(input, string.byte(string.sub(str, i, i)))
	end
	local output = Base64.Encode(input)
	local result = ''
	for k,v in ipairs(output) do
		result = result .. string.char(v)
	end
	return result
end

function Base64.DecodeString( str )
	if not str then return nil end
	local input = {}
    for i = 1, #str, 1 do
        table.insert(input, string.byte(string.sub(str, i, i)))
    end
    local output = Base64.Decode( input )
    local result = ''
    for k,v in ipairs(output) do
        result = result .. string.char(v)
    end
    return result
	-- body
end

-- print(this.DecodeString("6mJ+emeyfiZPemXRfiwqfkstazzRAkvya7aYARaNAmZPaNMNekMYaND="))

-- print(this.EncodeString("*6BB4837EB74329105EE4568DDA7DC67ED2CA2AD9"))
-- local inputs = {}
-- local str = "test"
-- for i = 1, 4, 1 do
-- 	table.insert(inputs, string.byte(string.sub(str, i, i)))
-- end

-- for k,v in ipairs(inputs) do
-- 	print(string.char(v))
-- end

-- print("=================================")

-- local enc = this.Encode(inputs)
-- for k,v in ipairs(enc) do
-- 	print(string.char(v))
-- end

-- print("=================================")
-- local dec = this.Decode(enc)
-- for k,v in ipairs(dec) do
-- 	print(string.char(v))
-- end
