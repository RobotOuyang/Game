using LuaFramework;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    [TaskCategory("镜像")]
    [TaskDescription("发送协议")]
    public class SendMessage : Action
    {
        public string main_name;
        public string sec_name;
        public string json_val;

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            Util.CallMethod("Network", "SendMessageWithJson", main_name, sec_name, json_val);

            return TaskStatus.Success;
        }
    }
}
