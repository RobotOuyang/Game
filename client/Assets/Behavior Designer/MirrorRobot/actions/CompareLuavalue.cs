using LuaFramework;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    public enum CompareType
    {
        Equal, // ==
        Bigger, // >=
        Smaller, // <=
    }

    [TaskCategory("镜像")]
    [TaskDescription("比较两个LuaValue")]
    public class CompareLuavalue : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("值a")]
        public SharedLuaValue val_a;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("值b")]
        public SharedLuaValue val_b;

        public CompareType compare_type;

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            if (compare_type == CompareType.Equal && val_a.Value.equal(val_b.Value))
            {
                return TaskStatus.Success;
            }
            else if (compare_type == CompareType.Smaller && val_a.Value.num <= val_b.Value.num)
            {
                return TaskStatus.Success;
            }
            // TODO

            return TaskStatus.Failure;
        }
    }
}
