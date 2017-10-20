using UnityEngine;
using LuaInterface;
using System.Collections.Generic;

namespace LuaFramework
{
    /// <summary>
    /// 用法：在lua脚本里，调用BindConf方法，将组件添加到某个gameobj上
    /// 使用AddDoubleValue 来添加变量， ChangeValue是编辑器每帧自动调用的，将编辑器内修改的变量重新赋值回lua，不从lua调用
    /// </summary>
    public class ConfForLua : MonoBehaviour {

        public Dictionary<string, double> m_varlist = new Dictionary<string, double>();

        LuaFunction m_luafunc;

        public static ConfForLua BindConf(GameObject obj, LuaFunction func)
        {
            ConfForLua conf = obj.AddComponent<ConfForLua>();
            conf.m_luafunc = func;
            return conf;
        }

        // Update is called once per frame
        public void AddDoubleValue(string name, double val)
        {
            m_varlist.Add(name, val);
        }

        // 这个函数尽量不能从lua调用
        public void ChangeValue(string name, double val)
        {
            m_varlist[name] = val;
            if (m_luafunc != null)
            {
                m_luafunc.Call(name, val);
            }
        }
    }
}
