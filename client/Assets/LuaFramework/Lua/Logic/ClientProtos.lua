module("ClientProtos", package.seeall)

GateHandlers = {
	QueryEntry = function ( uid, call_back )
		local data = JsonData.New()
		JsonData.set_Item(data, "uid", JsonData.New(uid))
		PomeloBehaviour:SendRequest("gate.gateHandler.queryEntry", data, call_back)
	end
}

ConnectorHandlers = {
	Enter = function ( username, rid, call_back )
		local data = TableToJsonData({username = username, rid = rid})
		PomeloBehaviour:SendRequest("connector.entryHandler.enter", data, call_back)
	end
}

ChatHandlers = {
	Send = function (  )
		
	end
}