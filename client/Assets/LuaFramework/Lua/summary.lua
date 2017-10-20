-- LuaProfiler
-- Copyright Kepler Project 2005-2007 (http://www.keplerproject.org/luaprofiler)
-- $Id: summary.lua,v 1.6 2009-03-16 15:55:32 alessandrohc Exp $

-- Function that reads one profile file
local function ReadProfile(file)

	local profile

	-- Check if argument is a file handle or a filename
	if io.type(file) == "file" then
		profile = file
	else
		-- Open profile
		profile = io.open(file)
	end

	-- Table for storing each profile's set of lines
	line_buffer = {}

	-- Get all profile lines
	local i = 1
	for line in profile:lines() do
		line_buffer[i] = line
		i = i + 1
    end

	-- Close file
	profile:close()
	return line_buffer
end

-- Function that creates the summary info
local function CreateSummary(lines, summary)

	local global_time = 0

	-- Note: ignore first line
	for i = 2, table.getn(lines) do
		local word = string.match(lines[i], "[^\t]+\t[^\t]+\t([^\t]+)")
		local file_defined, line_defined, line_cur, local_time, total_time = string.match(lines[i], "[^\t]+\t([^\t]+)\t[^\t]+\t([^\t]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)")
        local_time = string.gsub(local_time, ",", ".")
        total_time = string.gsub(total_time, ",", ".")
        word = string.format("%s——>%s_%s_%s", file_defined, word, line_defined, line_cur)

        if not (local_time and total_time) then return global_time end
        if summary[word] == nil then
			summary[word] = {};
			summary[word]["info"] = {}
			summary[word]["info"]["calls"] = 1
			summary[word]["info"]["total"] = local_time
			summary[word]["info"]["func"] = word
		else
			summary[word]["info"]["calls"] = summary[word]["info"]["calls"] + 1
			summary[word]["info"]["total"] = summary[word]["info"]["total"] + local_time;
		end

		global_time = global_time + local_time;
	end

	return global_time
end

function summary_profile( filename, to_file )
	print(filename, to_file)
	-- Global time
	local global_t = 0

	-- Summary table
	local profile_info = {}
	local file = io.open(filename)
	local file2 = io.open(to_file, "w")

	if not file then
	  print("File " .. filename .. " does not exist!")
	  return
	end

	local firstline = file:read(11)

	-- File is single profile
	if firstline == "stack_level" then

		-- Single profile
		local lines = ReadProfile(file)
		global_t = CreateSummary(lines, profile_info)

	else

		-- File is list of profiles
		-- Reset position in file
		file:seek("set")

		-- Loop through profiles and create summary table
		for line in file:lines() do

			local profile_lines

			-- Read current profile
			profile_lines = ReadProfile(line)

			-- Build a table with profile info
			global_t = global_t + CreateSummary(profile_lines, profile_info)
		end

		file:close()
	end

	-- Sort table by total time
	sorted = {}
	for k, v in pairs(profile_info) do table.insert(sorted, v) end
	table.sort(sorted, function (a, b) return tonumber(a["info"]["total"]) > tonumber(b["info"]["total"]) end)

	file2:write(string.format("%80s%25s%25s%25s%25s\n" ,"Node_name", "Calls", "Average_per_call", "Total_time", "%Time"))

	for k, v in pairs(sorted) do
		if v["info"]["func"] ~= "(null)" then
			local average = v["info"]["total"] / v["info"]["calls"]
			local percent = 100 * v["info"]["total"] / global_t
			file2:write(string.format("%80s%25s%25s%25s%25s\n", tostring(v["info"]["func"]), tostring(v["info"]["calls"]), tostring(average), tostring(v["info"]["total"]), tostring(percent)))
		end
	end
	file2:close()
end