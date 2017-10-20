﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class Pomelo_DotNetClient_PomeloClientWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(Pomelo.DotNetClient.PomeloClient), typeof(System.Object));
		L.RegFunction("poll", poll);
		L.RegFunction("close", close);
		L.RegFunction("Connect", Connect);
		L.RegFunction("HandShake", HandShake);
		L.RegFunction("request", request);
		L.RegFunction("notify", notify);
		L.RegFunction("on", on);
		L.RegFunction("New", _CreatePomelo_DotNetClient_PomeloClient);
		L.RegFunction("__tostring", Lua_ToString);
		L.RegVar("IsConnected", get_IsConnected, null);
		L.RegVar("HandShakeCache", get_HandShakeCache, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreatePomelo_DotNetClient_PomeloClient(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 4)
			{
				Pomelo.DotNetClient.ClientProtocolType arg0 = (Pomelo.DotNetClient.ClientProtocolType)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.ClientProtocolType));
				byte[] arg1 = ToLua.CheckByteBuffer(L, 2);
				string arg2 = ToLua.CheckString(L, 3);
				string arg3 = ToLua.CheckString(L, 4);
				Pomelo.DotNetClient.PomeloClient obj = new Pomelo.DotNetClient.PomeloClient(arg0, arg1, arg2, arg3);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: Pomelo.DotNetClient.PomeloClient.New");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int poll(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.PomeloClient));
			obj.poll();
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int close(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.PomeloClient));
			obj.close();
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Connect(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 7);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.PomeloClient));
			string arg0 = ToLua.CheckString(L, 2);
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			string arg2 = ToLua.CheckString(L, 4);
			System.Action arg3 = null;
			LuaTypes funcType5 = LuaDLL.lua_type(L, 5);

			if (funcType5 != LuaTypes.LUA_TFUNCTION)
			{
				 arg3 = (System.Action)ToLua.CheckObject(L, 5, typeof(System.Action));
			}
			else
			{
				LuaFunction func = ToLua.ToLuaFunction(L, 5);
				arg3 = DelegateFactory.CreateDelegate(typeof(System.Action), func) as System.Action;
			}

			System.Action arg4 = null;
			LuaTypes funcType6 = LuaDLL.lua_type(L, 6);

			if (funcType6 != LuaTypes.LUA_TFUNCTION)
			{
				 arg4 = (System.Action)ToLua.CheckObject(L, 6, typeof(System.Action));
			}
			else
			{
				LuaFunction func = ToLua.ToLuaFunction(L, 6);
				arg4 = DelegateFactory.CreateDelegate(typeof(System.Action), func) as System.Action;
			}

			int arg5 = (int)LuaDLL.luaL_checknumber(L, 7);
			bool o = obj.Connect(arg0, arg1, arg2, arg3, arg4, arg5);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int HandShake(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.PomeloClient));
			LitJson.JsonData arg0 = (LitJson.JsonData)ToLua.CheckObject(L, 2, typeof(LitJson.JsonData));
			System.Action<LitJson.JsonData> arg1 = null;
			LuaTypes funcType3 = LuaDLL.lua_type(L, 3);

			if (funcType3 != LuaTypes.LUA_TFUNCTION)
			{
				 arg1 = (System.Action<LitJson.JsonData>)ToLua.CheckObject(L, 3, typeof(System.Action<LitJson.JsonData>));
			}
			else
			{
				LuaFunction func = ToLua.ToLuaFunction(L, 3);
				arg1 = DelegateFactory.CreateDelegate(typeof(System.Action<LitJson.JsonData>), func) as System.Action<LitJson.JsonData>;
			}

			bool o = obj.HandShake(arg0, arg1);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int request(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes(L, 1, typeof(Pomelo.DotNetClient.PomeloClient), typeof(string), typeof(LuaInterface.LuaFunction)))
			{
				Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.ToObject(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				LuaFunction arg1 = ToLua.ToLuaFunction(L, 3);
				obj.request(arg0, arg1);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes(L, 1, typeof(Pomelo.DotNetClient.PomeloClient), typeof(string), typeof(LitJson.JsonData), typeof(LuaInterface.LuaFunction)))
			{
				Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.ToObject(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				LitJson.JsonData arg1 = (LitJson.JsonData)ToLua.ToObject(L, 3);
				LuaFunction arg2 = ToLua.ToLuaFunction(L, 4);
				obj.request(arg0, arg1, arg2);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Pomelo.DotNetClient.PomeloClient.request");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int notify(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.PomeloClient));
			string arg0 = ToLua.CheckString(L, 2);
			LitJson.JsonData arg1 = (LitJson.JsonData)ToLua.CheckObject(L, 3, typeof(LitJson.JsonData));
			obj.notify(arg0, arg1);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int on(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)ToLua.CheckObject(L, 1, typeof(Pomelo.DotNetClient.PomeloClient));
			string arg0 = ToLua.CheckString(L, 2);
			LuaFunction arg1 = ToLua.CheckLuaFunction(L, 3);
			obj.on(arg0, arg1);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Lua_ToString(IntPtr L)
	{
		object obj = ToLua.ToObject(L, 1);

		if (obj != null)
		{
			LuaDLL.lua_pushstring(L, obj.ToString());
		}
		else
		{
			LuaDLL.lua_pushnil(L);
		}

		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_IsConnected(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)o;
			bool ret = obj.IsConnected;
			LuaDLL.lua_pushboolean(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index IsConnected on a nil value" : e.Message);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_HandShakeCache(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Pomelo.DotNetClient.PomeloClient obj = (Pomelo.DotNetClient.PomeloClient)o;
			string ret = obj.HandShakeCache;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o == null ? "attempt to index HandShakeCache on a nil value" : e.Message);
		}
	}
}
