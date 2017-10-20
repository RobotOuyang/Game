require "Common/define"

CtrlManager = {};
local this = CtrlManager;
local ctrlList = {};	--控制器列表--
local ctrlTable = {};
local ctrlVal = 0

function CtrlManager.Init()
	logWarn("CtrlManager.Init----->>>")
	coroutine.start(function ()
		while true do
			if UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape) then
				local test = {}
				for k,v in pairs(ctrlList) do
					if v.isAwake then
						test[#test + 1] = { k, v }
					end
				end
				table.sort(test, function(a,b) return ctrlTable[a[1]] > ctrlTable[b[1]] end )
				-- for k,v in ipairs(test) do -- 安卓返回键, 从顶层界面往下传递
				-- 	if v[2]._OnEscClicked then
				-- 		if v[2]._OnEscClicked() then   
				-- 			break  -- 一旦对方接受并吞噬了这个事件，就不再传递
				-- 		end
				-- 	else
				-- 		break -- 一旦遇到一个界面不能被关闭
				-- 	end
				-- end
				-- TODO 暂时不做事件传递，只考虑顶层界面
				for k,v in ipairs(test) do
					if v[2]._OnEscClicked then
						v[2]._OnEscClicked()
					else
						CtrlManager._OnEscClicked(v[2])
					end
					break
				end
			end
			coroutine.step()
		end
	end)
	return this
end

-- 删除之前所有的Panel
function CtrlManager.ChangeCtrl(ctrlName, data)
	this.CloseAll()
	return this.Open(ctrlName, data)
end

function CtrlManager.GetOrCreateCtrl(ctrlName)
	local path = find_end_string(ctrlName, "/")
	if ctrlList[path] == nil then
		if _G[path] == nil then
			require("Controller/" .. ctrlName)
		end
		ctrlList[path] = _G[path].New()
	end
	ctrlList[path].ctrl_name = path
	return ctrlList[path]
end

function CtrlManager.GetCtrlName(ctrl)
	return ctrl.ctrl_name
end

-- 比GetOrCreateCtrl更方便
function CtrlManager.Open(ctrlName, data, parent_ctrl)
	local new_ctrl = this.GetOrCreateCtrl(ctrlName)
	if new_ctrl.isAwake and new_ctrl._RefreshData then
		-- logWarn(ctrlName .. '界面重复加载')	-- 目前界面Panel型的都是单例，如果需要重复弹出的，需要额外实现 20160713
		new_ctrl._RefreshData(data)
		return new_ctrl
	end
	if not new_ctrl.isAwake then
		new_ctrl.Awake(data)
		ctrlVal = ctrlVal + 1
		ctrlTable[find_end_string(ctrlName, "/")] = ctrlVal  -- 注意 awake並不代表界面一定打開了，還有一個異步加載的过程，所以awake序，不一定是显示序，这里有潜在的bug
		new_ctrl.parent_ctrl = parent_ctrl
	end
	return new_ctrl
end

-- 重连以后调用的函数，刷新所有已经打开的界面
function CtrlManager.OnReconnect()
	for k,v in pairs(ctrlList) do
		if v.isAwake and v._OnReconnect then
			v._OnReconnect()
		end
	end
end

function CtrlManager.EnterAgain()
	for k,v in pairs(ctrlList) do
		if v.isAwake and v._EnterAgain then
			ChatMgr.is_in_chat_cd = false
			v._EnterAgain()
		end
	end
end

function CtrlManager.QuitApp()
	for k,v in pairs(ctrlList) do
		if v.isAwake and v._QuitApp then
			v._QuitApp()
		end
	end
end

function CtrlManager.GetCurrentCtrl()
	local cur_ctrl
	local temp = 0
	for k, v in pairs(ctrlTable) do
		if v > temp then
			temp = v
			cur_ctrl = k
		end
	end
	return cur_ctrl
end

function CtrlManager.Close(ctrlName)
	ctrlTable[ctrlName] = nil
	if ctrlList[ctrlName] == nil then
		return
	end
	ctrlList[ctrlName].Close()
end

--获取控制器--
-- 弃用，用GetOrCreateCtrl
function CtrlManager.GetCtrl(ctrlName) 
	return ctrlList[ctrlName];
end

function CtrlManager.IsAwake(ctrlName)
	return CtrlManager.GetCtrl(ctrlName).isAwake
end

--移除控制器--
function CtrlManager.RemoveCtrl(ctrlName)
	ctrlList[ctrlName] = nil;
end

function CtrlManager.GetAllAwake( )
	local list = {}
	for k,v in pairs(ctrlList) do
		if v.isAwake then
			list[#list + 1] = v
		end
	end
	return list
end

function CtrlManager._OnEscClicked(ctrl)
	if GameHallCtrl.CloseRank() then
		return
	end
	ctrl.Close()
	return true
end

--关闭所有界面
function CtrlManager.CloseAll()
	for k,v in pairs(ctrlList) do
		if v.isAwake then
			v.Close()
		end
	end
	ctrlVal = 0
	ctrlList = {}
end