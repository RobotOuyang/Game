Util = LuaFramework.Util
AppConst = LuaFramework.AppConst
LuaHelper = LuaFramework.LuaHelper
ByteBuffer = LuaFramework.ByteBuffer

resMgr = LuaHelper.GetResManager()
panelMgr = LuaHelper.GetPanelManager()
soundMgr = LuaHelper.GetSoundManager()
-- networkMgr = LuaHelper.GetNetManager()
PomeloBehaviour = LuaHelper.GetPomeloBehaviour()

avatarMgr = LuaHelper.GetAvatarManager()
paymentManager = LuaHelper.GetPaymentManager()
gameManager = LuaHelper.GetGameManager()
pushManager = LuaHelper.GetPushManager()

WWW = UnityEngine.WWW
WWWForm = UnityEngine.WWWForm
GameObject = UnityEngine.GameObject
Application = UnityEngine.Application
RuntimePlatform = UnityEngine.RuntimePlatform
Shader = UnityEngine.Shader

SceneManager = UnityEngine.SceneManagement.SceneManager
VectorFunc = LuaFramework.VectorFunction
Screen = UnityEngine.Screen
DateTime = System.DateTime
JsonData = LitJson.JsonData
JsonMapper = LitJson.JsonMapper
ServerType = Pomelo.DotNetClient.ServerType
ClientProtocolType = Pomelo.DotNetClient.ClientProtocolType

UnityAction = UnityEngine.Events.UnityAction


-- 一些跟特定熱更包相關的東西
AppStoreId = '1153461394'  -- wl
AppStorePayid = ''  -- wl
-- AppStoreId = '1175225823'  --wt
-- AppStorePayid = 'twt'  -- wt

AppStoreCheck = false   -- 是不是在審核狀態，要關閉支付显示