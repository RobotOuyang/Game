using LuaFramework;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    [TaskCategory("镜像")]
    [TaskDescription("调用lua函数")]
    public class CallLuaFunction : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("调用的lua函数的模块名，比如Network")]
        public SharedString module_name;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("调用的lua函数名")]
        public SharedString func_name;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("调用的lua参数")]
        public SharedLuaValue func_param;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("调用的lua函数名")]
        public SharedLuaValue func_ret;


        public override void OnStart()
        {
            if (func_param.Value == null)
            {
                func_param.Value = new LuaValue();
            }
            if (func_ret.Value == null)
            {
                func_ret.Value = new LuaValue();
            }
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            object ret_val = null;
            if (func_ret.Value.value_type == LuaValueTyle.Nil)
            {
                if (func_param.Value.value_type == LuaValueTyle.Number)
                {
                    Util.CallMethod(module_name.Value, func_name.Value, func_param.Value.num);
                }
                else if (func_param.Value.value_type == LuaValueTyle.String)
                {
                    Util.CallMethod(module_name.Value, func_name.Value, func_param.Value.str);
                }
                else if (func_param.Value.value_type == LuaValueTyle.Bool)
                {
                    Util.CallMethod(module_name.Value, func_name.Value, func_param.Value.bol);
                }
                else
                {
                    Util.CallMethod(module_name.Value, func_name.Value);
                }
                func_ret.Value.value_type = LuaValueTyle.Nil;
            }
            else
            {
                if (func_param.Value.value_type == LuaValueTyle.Number)
                {
                    ret_val = Util.CallMethod(module_name.Value, func_name.Value, func_param.Value.num)[0];
                }
                else if (func_param.Value.value_type == LuaValueTyle.String)
                {
                    ret_val = Util.CallMethod(module_name.Value, func_name.Value, func_param.Value.str)[0];
                }
                else if (func_param.Value.value_type == LuaValueTyle.Bool)
                {
                    ret_val = Util.CallMethod(module_name.Value, func_name.Value, func_param.Value.bol)[0];
                }
                else
                {
                    ret_val = Util.CallMethod(module_name.Value, func_name.Value)[0];
                }

                //ret
                if (func_ret.Value.value_type == LuaValueTyle.Number)
                {
                    func_ret.Value.num = (double)ret_val;
                }
                else if (func_ret.Value.value_type == LuaValueTyle.String)
                {
                    func_ret.Value.str = (string)ret_val;
                }
                else if (func_ret.Value.value_type == LuaValueTyle.Bool)
                {
                    func_ret.Value.bol = (bool)ret_val;
                }
            }

            return TaskStatus.Success;
        }
    }
}
