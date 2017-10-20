using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using LuaFramework;
using Pomelo.DotNetClient;
using LitJson;

using BindType = ToLuaMenu.BindType;
using UnityEngine.UI;

public static class CustomSettings
{
    public static string FrameworkPath = AppConst.FrameworkRoot;
    public static string saveDir = FrameworkPath + "/ToLua/Source/Generate/";
    public static string luaDir = FrameworkPath + "/Lua/";
    public static string toluaBaseType = FrameworkPath + "/ToLua/BaseType/";
	public static string toluaLuaDir = FrameworkPath + "/ToLua/Lua";

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {        
        typeof(UnityEngine.Application),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.SleepTimeout),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Physics),
        typeof(UnityEngine.RenderSettings),
        typeof(UnityEngine.QualitySettings),
        typeof(SceneManager),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList = 
    {        
        _DT(typeof(Action)),        
        _DT(typeof(UnityEngine.Events.UnityAction)),
    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList = 
    {                
        //------------------------为例子导出--------------------------------
        //_GT(typeof(TestEventListener)),                
        //_GT(typeof(TestAccount)),
        //_GT(typeof(Dictionary<int, TestAccount>)).SetLibName("AccountMap"),                
        //_GT(typeof(KeyValuePair<int, TestAccount>)),    
        //-------------------------------------------------------------------        
                                       
        _GT(typeof(Debugger)),
        _GT(typeof(Component)),
        _GT(typeof(Behaviour)),
        _GT(typeof(MonoBehaviour)),
        _GT(typeof(WWWForm)),
        // 这个框架有点bug，多级的继承倒不出来wrap文件貌似，然而看代码又是考虑了多级继承的
        _GT(typeof(MaskableGraphic)),
        _GT(typeof(Graphic)),
        _GT(typeof(UIBehaviour)),
        _GT(typeof(GameObject)),
        _GT(typeof(Transform)),
        _GT(typeof(TrackedReference)),
        _GT(typeof(Application)),
        _GT(typeof(NetworkReachability)),
        _GT(typeof(SceneManager)),
        _GT(typeof(Physics)),
        _GT(typeof(Collider)),
        _GT(typeof(Time)),        
        _GT(typeof(Texture)),
        _GT(typeof(Texture2D)),
        _GT(typeof(Shader)),
        _GT(typeof(Material)),
        _GT(typeof(Renderer)),
        _GT(typeof(WWW)),
        _GT(typeof(Screen)),
        _GT(typeof(Camera)),
        _GT(typeof(CameraClearFlags)),
        _GT(typeof(AudioClip)),
        _GT(typeof(AudioSource)),
        _GT(typeof(AssetBundle)),
        _GT(typeof(ParticleSystem)),
        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)),
        _GT(typeof(LightType)),
        _GT(typeof(SleepTimeout)),
        _GT(typeof(Animator)),
        _GT(typeof(Input)),
        _GT(typeof(KeyCode)),
        _GT(typeof(SkinnedMeshRenderer)),
        _GT(typeof(Space)),        
                                           
        _GT(typeof(MeshRenderer)),            
        _GT(typeof(ParticleSystem)),
                              
        _GT(typeof(BoxCollider)),
        _GT(typeof(MeshCollider)),
        _GT(typeof(SphereCollider)),        
        _GT(typeof(CharacterController)),
        _GT(typeof(CapsuleCollider)),
        
        _GT(typeof(Animation)), 
        _GT(typeof(AnimatorStateInfo)),     
        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)),        
        _GT(typeof(AnimationState)),
        _GT(typeof(AnimationBlendMode)),
        _GT(typeof(QueueMode)),  
        _GT(typeof(PlayMode)),
        _GT(typeof(WrapMode)),
        _GT(typeof(RuntimePlatform)),

        _GT(typeof(QualitySettings)),
        _GT(typeof(RenderSettings)),                                                   
        _GT(typeof(BlendWeights)),           
        _GT(typeof(RenderTexture)),
        _GT(typeof(Rigidbody)),

        // 自定义的
        _GT(typeof(RectWebView)),
        _GT(typeof(BatteryLevel)),
        _GT(typeof(VectorFunction)),
        _GT(typeof(AvatarManager)),
        _GT(typeof(PushManager)),
        _GT(typeof(MirrortechRichText)),
        _GT(typeof(GameChooseRotation)),
        _GT(typeof(PaymentManager)),
        _GT(typeof(AniEventHandler)),
        _GT(typeof(ConfForLua)),
        _GT(typeof(PageView)),
        _GT(typeof(ClosePopupOnBg)),
        _GT(typeof(Main)),
        //DataEye
        //_GT(typeof(DCItem)),
        //_GT(typeof(DCCoin)),
        //_GT(typeof(DCVirtualCurrency)),
        //_GT(typeof(DCAccount)),
        //_GT(typeof(DCAccountType)),

        //for LuaFramework UI
        _GT(typeof(DateTime)),
        _GT(typeof(Canvas)),
        _GT(typeof(CanvasGroup)),
        _GT(typeof(RectTransform)),
        _GT(typeof(Text)),
        _GT(typeof(TextAnchor)),
        _GT(typeof(Rect)),
        _GT(typeof(Button)),
        _GT(typeof(Slider)),
        _GT(typeof(Mask)),
        _GT(typeof(Sprite)),
        _GT(typeof(Toggle)),
        _GT(typeof(ToggleGroup)),
        _GT(typeof(Scrollbar)),
        _GT(typeof(ScrollRect)),
        _GT(typeof(RawImage)),
        _GT(typeof(Image)),
        _GT(typeof(Dropdown)),
        _GT(typeof(InputField)),
        _GT(typeof(SpriteRenderer)),
        _GT(typeof(LayoutElement)),
        _GT(typeof(HorizontalLayoutGroup)),
        _GT(typeof(VerticalLayoutGroup)),
        _GT(typeof(GridLayoutGroup)),
        _GT(typeof(AspectRatioFitter)),
        _GT(typeof(ContentSizeFitter)),

        _GT(typeof(Util)),
        _GT(typeof(AppConst)),
        _GT(typeof(NotiConst)),
        _GT(typeof(ClipBoard)),
        _GT(typeof(LuaHelper)),
        _GT(typeof(ByteBuffer)),
        _GT(typeof(LuaBehaviour)),

        _GT(typeof(GameManager)),
        _GT(typeof(LuaManager)),
        _GT(typeof(PanelManager)),
        _GT(typeof(SoundManager)),
        _GT(typeof(TimerManager)),
        //_GT(typeof(NetworkManager)),
        _GT(typeof(ResourceManager)),
        _GT(typeof(pomeloBehaviour)),
        _GT(typeof(PomeloClient)),
        _GT(typeof(JsonData)),
        _GT(typeof(JsonMapper)),
        _GT(typeof(ServerType)),
        _GT(typeof(ClientProtocolType)),
    };

    public static List<Type> dynamicList = new List<Type>()
    {        
        typeof(MeshRenderer),
        typeof(ParticleSystem),

        typeof(BoxCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(CharacterController),
        typeof(CapsuleCollider),

        typeof(Animation),
        typeof(AnimationClip),
        typeof(AnimationState),

        typeof(BlendWeights),
        typeof(RenderTexture),
        typeof(Rigidbody),
    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {
        
    };

    static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    
}