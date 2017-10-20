function table.find(tab, val)
    for k, v in pairs(tab) do
        if v == val then
            return k
        end
    end
end

function table.copy(source)
	local pool = {} 
	table.assign(source, pool)
	return pool
end

function table.assign(source, pool)
	source = source or {}
	pool = pool or {}
	for key, value in pairs(source) do
		if type(value) == "table" then
			if pool[key] == nil then
				pool[key] = {}
			end
			table.assign(source[key], pool[key])
		else
			pool[key] = source[key]
		end
	end
end

function table.assign_over(source, pool)
   	source = source or {}
	pool = pool or {}
	for key, value in pairs(source) do
		if type(value) == "table" then
			if pool[key] == nil then
				pool[key] = {}
			end
			table.assign(source[key], pool[key])
		else
            if pool[key] == nil then
			    pool[key] = source[key]
            end
		end
	end 
end

function table.show(t, name, indent)
   local cart     -- a container
   local autoref  -- for self references

   -- (RiciLake) returns true if the table is empty
   local function isemptytable(t) return next(t) == nil end

   local function basicSerialize (o)
      local so = tostring(o)
      if type(o) == "function" then
         local info = debug.getinfo(o, "S")
         -- info.name is nil because o is not a calling level
         if info.what == "C" then
            return string.format("%q", so .. ", C function")
         else 
            -- the information is defined through lines
            return string.format("%q", so .. ", defined in (" ..
                info.linedefined .. "-" .. info.lastlinedefined ..
                ")" .. info.source)
         end
      elseif type(o) == "number" or type(o) == "boolean" then
         return so
      else
         return string.format("%q", so)
      end
   end

   local function addtocart (value, name, indent, saved, field)
      indent = indent or ""
      saved = saved or {}
      field = field or name

      cart = cart .. indent .. field

      if type(value) ~= "table" then
         cart = cart .. " = " .. basicSerialize(value) .. ";\n"
      else
         if saved[value] then
            cart = cart .. " = {}; -- " .. saved[value] 
                        .. " (self reference)\n"
            autoref = autoref ..  name .. " = " .. saved[value] .. ";\n"
         else
            saved[value] = name

            if isemptytable(value) then
               cart = cart .. " = {};\n"
            else
               cart = cart .. " = {\n"
               for k, v in pairs(value) do
                  k = basicSerialize(k)
                  local fname = string.format("%s[%s]", name, k)
                  field = string.format("[%s]", k)
                  -- three spaces between levels
                  addtocart(v, fname, indent .. "   ", saved, field)
               end
               cart = cart .. indent .. "};\n"
            end
         end
      end
   end

   name = name or "__unnamed__"
   if type(t) ~= "table" then
      return name .. " = " .. basicSerialize(t)
   end
   cart, autoref = "", ""
   addtocart(t, name, indent)
   return cart .. autoref
end

function table.slice( tab, from_index, to_index )
   if from_index <= 0 or from_index > #tab or to_index <= 0 or to_index > #tab then return end
   local result = {}
   for i = from_index, to_index, 1 do
      if type(tab[i]) == "table" then
         table.insert(result, table.copy(tab[i]))
      end
   end
   return result
end

function table.has_value_ext( tab, value )
   for k,v in pairs(tab) do
      if v == value then
         return k
      end
   end
   return nil
end