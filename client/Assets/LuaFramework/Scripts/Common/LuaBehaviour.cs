using UnityEngine;
using LuaInterface;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

namespace LuaFramework {
    class LongPressInfo
    {
        public float interval = -1;
        public float begin_interval = 1;
        public LuaFunction click_call_back = null;
        public LuaFunction press_call_back = null;
        public AudioSource audio = null;
        bool is_pressing = false;
        bool is_press_func_called = false;
        float press_time = 0;
        float last_press_func_time = 0;
        Button this_btn;

        public void Update()
        {
            if (!is_pressing)
            {
                return;
            }
            if (Time.time - press_time > begin_interval)
            {
                if (interval < 0 && is_press_func_called)
                {
                    return;
                }
                if (Time.time - last_press_func_time > interval && press_call_back != null)
                {
                    last_press_func_time = Time.time;
                    press_call_back.Call(this_btn);
                    is_press_func_called = true;
                }
            }
        }

        public LongPressInfo(Button btn)
        {
            this_btn = btn;
            ButtonHandler listener = ButtonHandler.Get(btn.gameObject);
            listener.onDown += (delegate (Vector2 point_event)
            {
                if (!this_btn.IsInteractable())
                {
                    return;
                }
                is_pressing = true;
                press_time = Time.time;
                is_press_func_called = false;
            });
            listener.onUp += (delegate (Vector2 point_event)
            {
                if (is_pressing == false)
                {
                    return;
                }
                is_pressing = false;
                if (!this_btn.IsInteractable())
                {
                    return;
                }

                // 长按没生效，才考虑单按
                if (!is_press_func_called && click_call_back != null)
                {
                    Util.CallMethod("SoundManager", "PlayButtonSound", btn, audio);
                    click_call_back.Call(btn);
                }
            });
            listener.onExit += (delegate (Vector2 point_event)
            {
                is_pressing = false;
            });
        }
    }

    public class LuaBehaviour : View {
        private string data = null;
        private Dictionary<Selectable, LuaFunction> buttons = new Dictionary<Selectable, LuaFunction>();
        private Dictionary<Button, LongPressInfo> m_long_press = new Dictionary<Button, LongPressInfo>();

        public void Awake() {
            Util.CallMethod(name, "Awake", gameObject);
        }

        protected void Start() {
            Util.CallMethod(name, "Start");
        }

        protected void Update()
        {
            foreach(var info in m_long_press)
            {
                if (info.Value != null)
                {
                    info.Value.Update();
                }
            }
        }

        /// <summary>
        /// 添加单击事件,默认触发音效，也可以自己指定, 目前按钮的实现更改成其他方式，方便长按，但是缺点是不能同时加多个回调
        /// </summary>
        public void AddClick(Button btn, LuaFunction luafunc, AudioClip clip, bool no_sound)
        {
            if (btn == null || luafunc == null) return;

            AudioSource audio = null;
            if (!no_sound)
            {
                audio = btn.GetComponent<AudioSource>();
                if (audio == null)
                {
                    audio = btn.gameObject.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                }
                audio.reverbZoneMix = 0;
                audio.spatialBlend = 0.5f;
            }
            if (audio != null && clip != null)
            {
                audio.clip = clip;
            }

            LongPressInfo button_info = null;
            if (!m_long_press.TryGetValue(btn, out button_info))
            {
                button_info = new LongPressInfo(btn);
                m_long_press.Add(btn, button_info);
            }
            button_info.click_call_back = luafunc;
            button_info.audio = audio;
        }

        // 长按，需要考虑和click的互斥
        public void AddPress(Button btn, LuaFunction luafunc, float begin_time, float interval)
        {
            if (btn == null || luafunc == null) return;

            LongPressInfo button_info = null;
            if (!m_long_press.TryGetValue(btn, out button_info))
            {
                button_info = new LongPressInfo(btn);
                m_long_press.Add(btn, button_info);
            }
            button_info.press_call_back = luafunc;
            button_info.begin_interval = begin_time;
            button_info.interval = interval;
        }

        public void AddClick(Button btn, LuaFunction luafunc)
        {
            AddClick(btn, luafunc, null, false);
        }

        public void AddClick(Button btn, LuaFunction luafunc, bool no_sound)
        {
            AddClick(btn, luafunc, null, no_sound);
        }

        public void AddClick(Button btn, LuaFunction luafunc, AudioClip clip)
        {
            AddClick(btn, luafunc, clip, false);
        }

        /// <summary>
        /// 添加触摸按下
        /// </summary>
        public void AddPointDown(GameObject obj, LuaFunction luafunc)
        {
            if (obj == null) return;

            ButtonHandler listener = ButtonHandler.Get(obj);
            listener.onDown += (delegate (Vector2 point_event)
            {
                luafunc.Call(point_event);
            });
        }

        public void AddPointMove(GameObject obj, LuaFunction luafunc)
        {
            if (obj == null) return;

            ButtonHandler listener = ButtonHandler.Get(obj);
            listener.onMove += (delegate (Vector2 point_event)
            {
                luafunc.Call(point_event);
            });
        }

        /// <summary>
        /// 添加触摸弹起
        /// </summary>
        public void AddPointUp(GameObject btn, LuaFunction luafunc)
        {
            if (btn == null) return;

            ButtonHandler listener = ButtonHandler.Get(btn);
            listener.onUp += (delegate (Vector2 point_event)
            {
                luafunc.Call(point_event);
            });
            // 这里之所以加两个是把手指划出去也算弹起
            //listener.onExit += (delegate ()
            //{
            //    luafunc.Call();
            //});
        }

        /// <summary>
        /// 添加触摸进入
        /// </summary>
        public void AddPointEnter(GameObject btn, LuaFunction luafunc)
        {
            if (btn == null) return;

            ButtonHandler listener = ButtonHandler.Get(btn);
            listener.onEnter += (delegate (Vector2 point_event)
            {
                luafunc.Call(point_event);
            });
        }

        /// <summary>
        /// 添加触摸退出
        /// </summary>
        public void AddPointExit(GameObject btn, LuaFunction luafunc)
        {
            if (btn == null) return;

            ButtonHandler listener = ButtonHandler.Get(btn);
            listener.onExit += (delegate (Vector2 point_event)
            {
                luafunc.Call(point_event);
            });
        }

        public void AddToggleListen(Toggle tog, LuaFunction func)
        {
            if (tog == null || func == null) return;
            AudioSource audio = tog.GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = tog.gameObject.AddComponent<AudioSource>();
                audio.playOnAwake = false;
            }
            tog.onValueChanged.AddListener(
                delegate (bool val)
                {
                    func.Call(tog, val);
                    Util.CallMethod("SoundManager", "PlayToggleSound", tog, audio);
                }
            );
        }

        public void InputAddValueListen(InputField input, LuaFunction func)
        {
            if (input == null || func == null) return;

            input.onValueChanged.AddListener(
                delegate (string val)
                {
                    func.Call(input, val);
                }
            );
        }

        public void InputAddFinishListen(InputField input, LuaFunction func)
        {
            if (input == null || func == null) return;

            input.onEndEdit.AddListener(
                delegate (string val)
                {
                    func.Call(input, val);
                }
            );
        }

        public void SliderAddValueListen(Slider slider, LuaFunction func)
        {
            if (slider == null || func == null) return;

            slider.onValueChanged.AddListener(
                delegate (float val)
                {
                    func.Call(val);
                }
            );
        }

        public void DropDownAddListen(Dropdown drop, LuaFunction func)
        {
            if (drop == null || func == null) return;

            drop.onValueChanged.AddListener(
                delegate(int index)
                {
                    func.Call(index);
                }
            );
        }

        public void DropDownAddOption(Dropdown drop, string[] strs)
        {
            List<string> ret = new List<string>(strs);
            drop.AddOptions(ret);
        }

        /// <summary>
        /// 删除单击事件
        /// </summary>
        /// <param name="go"></param>
        public void RemoveClick(Selectable btn)
        {
            if (btn == null) return;
            LuaFunction luafunc = null;
            if (buttons.TryGetValue(btn, out luafunc)) {
                luafunc.Dispose();
                luafunc = null;
                buttons.Remove(btn);
            }
        }

        /// <summary>
        /// 清除单击事件
        /// </summary>
        public void ClearClick() {
            foreach (var de in buttons) {
                if (de.Value != null) {
                    de.Value.Dispose();
                }
            }
            buttons.Clear();
        }

        //-----------------------------------------------------------------
        protected void OnDestroy() {
            ClearClick();
#if ASYNC_MODE
            // 这句话意味着每个panel的目录下都要有一个目录名+panel
            string abName = name.ToLower().Replace("panel", "");
            if (ResManager != null)
            {
                ResManager.UnloadAssetBundle(abName + AppConst.ExtName);
            }
#endif
            Util.ClearMemory();
            Util.CallMethod(name, "OnDestroy");
        }
    }
}