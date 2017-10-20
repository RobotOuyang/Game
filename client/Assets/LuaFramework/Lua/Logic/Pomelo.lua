require "Common/define"
require "Common/functions"
require "Common/bit"
require "Common/Base64"
require "Logic/ClientProtos"

Pomelo = {}
local this = Pomelo

function this.Start(  )
	-- print("11111111")
	-- print(PomeloBehaviour.Test())
	-- print(PomeloBehaviour:ConnectServer("127.0.0.1", 3014))
end

function this.OnConnectToGate(  )
	-- body
	print("connect to gate in lua")
	local uid = "1"
	-- PomeloBehaviour:SendRequest(uid, )
	ClientProtos.GateHandlers.QueryEntry(uid, this.OnQueryEntry)
end

function this.OnQueryEntry( data )
	local data_tab = JsonDataToTable(data)

	local code = data_tab.code
	local host = data_tab.host
	local port = data_tab.port

	PomeloBehaviour:CloseClient()
	if code == 200 then
		local handshakecache = ""
		PomeloBehaviour:ConnectServer(host, port, ServerType.CONNECTOR, ClientProtocolType.NORMAL, handshakecache, nil, "", nil)
	end
end

function this.OnConnectToConnector(  )
	-- body
	--在这里注册各种notice的响应,然后走登录流程
	print("connect to connecter in lua")
	PomeloBehaviour:On("onAdd", function ( data )
		print("onAdd")
	end)

	ClientProtos.ConnectorHandlers.Enter("pomelo_test", "pomelo", this.OnEntered)
end

function this.OnEntered( data )
	-- print(data)
	-- print(type(data))
	-- print_table(data)
	local data_tab = JsonDataToTable(data)
	print_table(data_tab)
end

function this.OnServerDisconnect(  )
	-- body
	print("disconnect server in lua")
end