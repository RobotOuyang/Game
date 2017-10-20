--Buildin Table
Protocal = {
	Connect		= 101,	--连接服务器
	Exception   = 102,	--异常掉线
	Disconnect  = 103,	--正常断线   
	Message		= 104,--接收消息
	MessageFailed = 105,	-- 发送消息失败

	----------上面这个四个不能更改，因为c#层也做了这个四个的定义--------
}


