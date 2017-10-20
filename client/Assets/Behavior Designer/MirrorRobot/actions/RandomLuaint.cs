using LuaFramework;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    [TaskCategory("镜像")]
    [TaskDescription("返回随机的int")]
    public class RandomLuaint : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("random val")]
        public SharedLuaValue val;

        public int from_int = 0;
        public int to_int = 1;

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            val.Value = new LuaValue(Random.Range(from_int, to_int + 1));

            return TaskStatus.Success;
        }
    }
}
