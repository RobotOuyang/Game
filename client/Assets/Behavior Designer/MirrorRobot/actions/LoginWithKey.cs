using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using LuaFramework;

namespace MirrorRobot
{
    [TaskCategory("镜像")]
    [TaskDescription("使用某个key登陆")]

    public class LoginWithKey : Action
    {
        public SharedInt access_info = 100000;

        public override TaskStatus OnUpdate()
        {
            AppConst.Access_info = "robot_" + access_info.Value;

            Util.CallMethod("LoginCtrl", "OnTouristClicked");

            return TaskStatus.Success;
        }
    }
}
