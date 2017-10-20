require "Common/define"
require "Common/protocal"
require "Common/functions"
require "Common/clone"
require "Common/class"
require 'Common/file_db'
require "Logic/CtrlManager"
require "Logic/Pomelo"

--执行顺序是： load Main.lua, Main.Main(), Main.CheckUpdate(), --->load Game.lua, --->load NetWork.lua， NetWork.Start(), Game.OnInitOK()
function Main()
	-- 开启这个函数，能观察lua中表的长度，lua内存泄漏99%是因为表的无限增长
	-- CHECK_MEM.CheckMem()
	-- CtrlManager.Open("LoadingCtrl")
	-- Pomelo.Start()
	PomeloBehaviour:ConnectToGate()
end