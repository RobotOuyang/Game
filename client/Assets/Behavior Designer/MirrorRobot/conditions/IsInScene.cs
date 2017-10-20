using LuaFramework;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    [TaskCategory("镜像")]
    [TaskDescription("当前场景名")]
    public class IsInScene : Conditional
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("当前要判断的场景名称")]
        public SharedString scene_name;

        public override TaskStatus OnUpdate()
        {
            if (scene_name.Value.ToLower() == LuaHelper.GetResManager().cur_scene.ToLower())
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}
