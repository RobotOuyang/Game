using UnityEngine;
using System.Collections;
using LuaFramework;

public class StartUpCommand : ControllerCommand {

    public override void Execute(IMessage message) {
        if (!Util.CheckEnvironment()) return;

        GameObject gameMgr = GameObject.Find("GlobalGenerator");
        if (gameMgr != null) {
            gameMgr.AddComponent<AppView>();
        }
        //-----------------关联命令-----------------------
        AppFacade.Instance.RegisterCommand(NotiConst.DISPATCH_MESSAGE, typeof(SocketCommand));

        //-----------------初始化管理器-----------------------
        //AppFacade.Instance.AddManager<GAManager>(ManagerName.GA);
        //AppFacade.Instance.AddManager<DataEyeManager>(ManagerName.DataEye);
        AppFacade.Instance.AddManager<LuaManager>(ManagerName.Lua);
        AppFacade.Instance.AddManager<PanelManager>(ManagerName.Panel);
        AppFacade.Instance.AddManager<SoundManager>(ManagerName.Sound);
        AppFacade.Instance.AddManager<TimerManager>(ManagerName.Timer);
        //AppFacade.Instance.AddManager<NetworkManager>(ManagerName.Network);
        AppFacade.Instance.AddManager<pomeloBehaviour>(ManagerName.Pomelo);
        AppFacade.Instance.AddManager<ResourceManager>(ManagerName.Resource);
        AppFacade.Instance.AddManager<GameManager>(ManagerName.Game);
        AppFacade.Instance.AddManager<AvatarManager>(ManagerName.Avatar);
        AppFacade.Instance.AddManager<PaymentManager>(ManagerName.Payment);
        AppFacade.Instance.AddManager<PushManager>(ManagerName.Push);
    }
}