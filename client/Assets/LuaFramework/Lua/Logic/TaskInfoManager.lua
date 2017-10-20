require "autogen/TaskInfoConfig"

TaskInfoManager = {}
local this = TaskInfoManager

local fake_ctrl = {isAwake = true}

local daily_task_before_hidden, his_task_before_hidden
local daily_task, his_task

function this.OnDailyTaskChange(json_str)
	daily_task_before_hidden = FILE_DB.JsonToLua(json_str)
	daily_task = this.RemoveHiddenTasks(FILE_DB.JsonToLua(json_str))
end

function this.OnHisTaskChange(json_str)
	his_task_before_hidden = FILE_DB.JsonToLua(json_str)
	his_task = this.RemoveHiddenTasks(FILE_DB.JsonToLua(json_str))
end

function this.RemoveHiddenTasks(tasks)
	local removed_daily_task = {}
	for _,v in pairs(tasks) do
		if TaskInfoConfig.TaskInfoList[v.id] then
			if TaskInfoConfig.TaskInfoList[v.id].is_hidden ~= 1 then
				table.insert(removed_daily_task, v)
			end
		end
	end
	return removed_daily_task
end

--过滤任务
function this.FilterDailyTask(filter)
	local filtered_daily_task = {}
	for _, v in ipairs(filter) do
		for _,task in pairs(daily_task) do
			if v == TaskInfoConfig.TaskInfoList[task.id].show_scene then
				table.insert(filtered_daily_task, task)
			end
		end
	end
	return filtered_daily_task
end

function this.FilterHisTask( filter )
	local filtered_his_task = {}
	for _,v in ipairs(filter) do
		for _,task in pairs(his_task) do
			if v == TaskInfoConfig.TaskInfoList[task.id].show_scene then
				table.insert(filtered_his_task, task)
			end
		end
	end
	return filtered_his_task
end

function this.CanReceiveTaskBonus()
	local can_receive = false
	for _,v in ipairs(daily_task) do
		if not v.has_receive_bonus and v.already_complete >= v.need_complete then
			can_receive = true
			break
		end
	end
	for _,v in ipairs(his_task) do
		if not v.has_receive_bonus and v.already_complete >= v.need_complete then
			can_receive = true
			break
		end
	end
	return can_receive
end

function this.CanReceiveFilteredDailyTaskBonus(filter)
	local filtered_daily_task = this.FilterDailyTask(filter)
	local can_receive = false
	for _,v in ipairs(filtered_daily_task) do
		if not v.has_receive_bonus and v.already_complete >= v.need_complete then
			can_receive = true
			break
		end
	end
	return can_receive
end

function this.CanReceiveFilteredTaskBonus( filter )
	local can_receive = false
	local filtered_daily_task = this.FilterDailyTask(filter)
	for _,v in ipairs(filtered_daily_task) do
		if not v.has_receive_bonus and v.already_complete >= v.need_complete then
			can_receive = true
			break
		end
	end

	local filtered_his_task = this.FilterHisTask(filter)
	for _,v in ipairs(filtered_his_task) do
		if not v.has_receive_bonus and v.already_complete >= v.need_complete then
			can_receive = true
			break
		end
	end
	return can_receive
end

function this.IsTaskFinished(task_id)
	local is_finished = false
	for _,v in ipairs(daily_task_before_hidden) do
		if v.id == task_id and v.already_complete >= v.need_complete then
			is_finished = true
			break
		end
	end
	for _,v in ipairs(his_task_before_hidden) do
		if v.id == task_id and v.already_complete >= v.need_complete then
			is_finished = true
			break
		end
	end
	return is_finished
end

function this.IsDailyTaskBonusReceived( task_id )
	local is_received = false
	for _,v in ipairs(daily_task_before_hidden) do
		if v.id == task_id and v.has_receive_bonus == true then
			is_received = true
			break
		end
	end
	return is_received
end

--判断某个任务是否存在
function this.HasTaskInStr(task_id)
	local has_task = false
	for _,v in ipairs(daily_task_before_hidden) do
		if v.id == task_id then
			has_task = true
			break
		end
	end

	for _,v in ipairs(his_task_before_hidden) do
		if v.id == task_id then
			has_task = true
			break
		end
	end
	return has_task
end

UserData.watch_daily_task(fake_ctrl, this.OnDailyTaskChange)
UserData.watch_his_task(fake_ctrl, this.OnHisTaskChange)