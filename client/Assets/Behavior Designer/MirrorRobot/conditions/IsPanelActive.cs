using LuaFramework;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    [TaskCategory("镜像")]
    [TaskDescription("panel是不是加载在canvas下")]
    public class IsPanelActive : Conditional
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("当前要判断的panel全称")]
        public SharedString panel_name;

        public override TaskStatus OnUpdate()
        {
            GameObject go = GameObject.FindWithTag("GuiCanvas");
            if (go != null)
            {
                int count = go.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    if (panel_name.Value == go.transform.GetChild(i).name)
                    {
                        return TaskStatus.Success;
                    }
                }
            }

            return TaskStatus.Failure;
        }
    }
}
