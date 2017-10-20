using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MirrorRobot
{
    [System.Serializable]
    public enum LuaValueTyle
    {
        Nil,
        Number,
        String,
        Bool,
    }

    [System.Serializable]
    public class LuaValue
    {
        public double num;
        public string str;
        public bool bol;
        public LuaValueTyle value_type;

        public LuaValue()
        {
            value_type = LuaValueTyle.Nil;
        }

        public LuaValue(double val)
        {
            num = val;
            value_type = LuaValueTyle.Number;
        }

        public LuaValue(string val)
        {
            str = val;
            value_type = LuaValueTyle.String;
        }

        public LuaValue(bool val)
        {
            bol = val;
            value_type = LuaValueTyle.Bool;
        }

        public bool equal(LuaValue b)
        {
            if (value_type == b.value_type)
            {
                switch (value_type)
                {
                    case LuaValueTyle.Bool:
                        return bol == b.bol;
                    case LuaValueTyle.Number:
                        return num == b.num;
                    case LuaValueTyle.String:
                        return str == b.str;
                    default: return true;
                }
            }
            else return false;
        }
    }

    [System.Serializable]
    public class SharedLuaValue : SharedVariable<LuaValue>
    {
        public override string ToString()
        {
            return mValue == null ? "null" : mValue.ToString();
        }
    }
}