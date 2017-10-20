using UnityEngine;
using LuaInterface;

    public class AniEventHandler : MonoBehaviour
    {
        LuaFunction call_back;

        public void RegCallback(LuaFunction func)
        {
            call_back = func;
        }

        public void AniEventCallBack(int event_type)
        {
            if (call_back != null)
            {
                call_back.Call(event_type);
            }
        }
    }
