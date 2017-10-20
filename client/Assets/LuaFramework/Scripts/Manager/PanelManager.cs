using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LuaInterface;

namespace LuaFramework {
    public class PanelManager : Manager {
        private Transform parent;

        Transform Parent {
            get {
                if (parent == null) {
                    GameObject go = GameObject.FindWithTag("GuiCanvas");
                    if (go != null) parent = go.transform;
                }
                return parent;
            }
        }

        void ResetOrder()
        {
            int count = Parent.childCount;

            for (int i = 0; i < count; i++)
            {
                // 每个界面前后留了各99个层级给美术做例子特效。
                UIDepth.AddDepth(Parent.GetChild(i).gameObject, i * 100);
            }
        }

        /// <summary>
        /// ´´½¨Ãæ°å£¬ÇëÇó×ÊÔ´¹ÜÀíÆ÷£¬ GuiCameraÏÂÖ»ÔÊÐíÓÐÒ»¸öÍ¬ÃûµÄpanel
        /// </summary>
        /// <param name="type"></param>
        public void CreatePanel(string name, LuaFunction func = null) {

            ResManager.LoadPrefab(name, delegate(UnityEngine.Object[] objs) {
                if (objs.Length == 0 || objs[0] == null) return;
                // Get the asset.
                GameObject prefab = objs[0] as GameObject;
                if (prefab == null) return;

                int obj_index = name.LastIndexOf('/');
                string obj_name = name;
                if (obj_index > 0)
                {
                    obj_name = name.Substring(obj_index + 1, name.Length - obj_index - 1);
                }

                Transform trans = Parent.Find(obj_name);
                GameObject go = null;
                if (trans != null)
                {
                    go = trans.gameObject;
                }
                // Èç¹û³¡¾°ÀïÒÑ¾­ÓÐÕâ¸öui ¾Í²»´´½¨ÁË¡£ ÕâÖÖÓ¦ÓÃÓÚ³¡¾°ÓÐ¸öÄ¬ÈÏuiµÄÇé¿ö£¬·ÀÖ¹³öÏÖui¿Õ°×ÆÚ
                if (go == null) {
                    go = Instantiate(prefab) as GameObject;
                }
                RectTransform rect = go.GetComponent<RectTransform>();

                go.name = obj_name;
                go.layer = LayerMask.NameToLayer("UI");
                if (go.GetComponent<LuaBehaviour>() == null)
                {
                    go.AddComponent<LuaBehaviour>();
                }
                else
                {
                    go.GetComponent<LuaBehaviour>().Awake();
                }

                rect.SetParent(Parent);
                
                rect.localScale = Vector3.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localPosition = Vector3.zero;

                ResetOrder();
                if (func != null) func.Call(go);
                // ¼ì²éÏÂ±³¾°ÒôÀÖµÄ¿ª¹Ø°É£¬Õâ¸öµ÷ÓÃ±È½Ï³óÂª£¬ÒòÎªÅäÖÃÊÇÐ´ÔÚluaÀïµÄ
                Util.CallMethod("SoundManager", "BackgroundMusicCheck");
            });
        }

        public void BackgroundMusicCheck(bool is_on)
        {
            AudioSource back = Parent.GetComponent<AudioSource>();
            if (back != null)
            {
                if (is_on)
                {
                    if (!back.isPlaying) back.Play();
                }
                else
                {
                    back.Stop();
                }
            }
        }

        // ¼ÓÒ»¸öÓÅ»¯£¬²»Æµ·±µÄInstantiate, TODO, ¼ÓÒ»¸öÈ«²¿ÇåÀíµÄº¯Êý
        Dictionary<GameObject, List<GameObject>> m_Reuse_Instantiated = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> m_serach_Prefab = new Dictionary<GameObject, GameObject>();

        public int GetCacheCount()
        {
            int count = 0;
            foreach ( var var1 in m_Reuse_Instantiated)
            {
                count += var1.Value.Count;
            }
            return count;
        }

        /// <summary>
        /// ¸ù¾Ý¸ø¶¨µÄÔ¤ÖÆ£¬ÔÚÖ¸¶¨Î»ÖÃ´´½¨Ò»¸öÌØÐ§¡£
        /// </summary>
        public GameObject CreateEffect(Transform panel_parent, GameObject prefab, bool in_pool)
        {
            if ( panel_parent == null || prefab == null ) return null;

            List<GameObject> collect = null;
            GameObject go = null;

            m_Reuse_Instantiated.TryGetValue(prefab, out collect);
            if (in_pool && collect != null && collect.Count > 0)
            {
                go = collect[collect.Count - 1];
                collect.RemoveAt(collect.Count - 1);
                go.SetActive(true);
            }

            if (go == null)
            {
                go = Instantiate(prefab) as GameObject;
                if (in_pool)
                {
                    m_serach_Prefab[go] = prefab;
                }
            }

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(panel_parent, false);
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;

            UIDepth.SetOrderToParent(rect);
            return go;
        }

        public GameObject CreateEffect(Transform panel_parent, GameObject prefab)
        {
            return CreateEffect(panel_parent, prefab, false);
        }

        /// <summary>
        /// ¸ù¾Ý¸ø¶¨µÄÔ¤ÖÆ£¬ÔÚÖ¸¶¨Î»ÖÃ´´½¨Ò»¸öÌØÐ§¡£
        /// </summary>
        public GameObject CreateParticle(Transform panel_parent, GameObject prefab, bool in_pool)
        {
            if ( panel_parent == null || prefab == null ) return null;

            List<GameObject> collect = null;
            GameObject go = null;

            m_Reuse_Instantiated.TryGetValue(prefab, out collect);
            if (in_pool && collect != null && collect.Count > 0)
            {
                go = collect[collect.Count - 1];
                collect.RemoveAt(collect.Count - 1);
                go.SetActive(true);
            }

            if (go == null)
            {
                go = Instantiate(prefab) as GameObject;
                if (in_pool)
                {
                    m_serach_Prefab[go] = prefab;
                }
            }

            Transform rect = go.GetComponent<Transform>();
            rect.SetParent(panel_parent, false);
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;

            UIDepth.SetOrderToParent(rect);
            return go;
        }

        public GameObject CreateParticle(Transform panel_parent, GameObject prefab)
        {
            return CreateParticle(panel_parent, prefab, false);
        }

        public void ClearAllEffect()
        {
            m_Reuse_Instantiated.Clear();
            m_serach_Prefab.Clear();
        }

        public void CollectEffect(GameObject go)
        {
            if (go == null) return;

            GameObject prefab = null;

            m_serach_Prefab.TryGetValue(go, out prefab);

            if (prefab != null)
            {
                List<GameObject> collect = null;
                m_Reuse_Instantiated.TryGetValue(prefab, out collect);

                if (collect == null)
                {
                    collect = new List<GameObject>();
                }
                collect.Add(go);
                m_Reuse_Instantiated[prefab] = collect;
                go.transform.SetParent(null);
                go.SetActive(false);
            }
            else
            {
                // ÕÒ²»µ½×Ô¼ºµÄprefab, ¾ÍÖ»ÄÜÏú»ÙÁË
                Destroy(go);
            }
        }


        /// <summary>
        /// Ö»Òþ²ØÆðÀ´£¬µ«ÊÇ²»ÏÔÊ¾ÁË£¬ÃâµÃÃ¿´ÎÒªÖØÐÂ¼ÓÔØ
        /// </summary>
        /// <param name="name"></param>
        public void SetPanelActive(string name, bool isActive)
        {
            // Í¬Ê±Ö§³ÖÂ·¾¶ºÍ²»´øÂ·¾¶µÄ°Ñ
            int obj_index = name.LastIndexOf('/');
            string obj_name = name;
            if (obj_index > 0)
            {
                obj_name = name.Substring(obj_index + 1, name.Length - obj_index - 1);
            }
            Transform go = Parent.Find(obj_name);
            go.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// ¹Ø±ÕGuiCameraÏÂµÄpanel£¬ÕâÀïµÄpanelÒ²¾ÍÊÇÒ»¸öui½çÃæ
        /// </summary>
        /// <param name="name"></param>
        public void ClosePanel(string name)
        {
            if (Parent == null)
            {
                return;
            }
            // Í¬Ê±Ö§³ÖÂ·¾¶ºÍ²»´øÂ·¾¶µÄ°Ñ
            int obj_index = name.LastIndexOf('/');
            string obj_name = name;
            if (obj_index > 0)
            {
                obj_name = name.Substring(obj_index + 1, name.Length - obj_index - 1);
            }
            Transform go = Parent.Find(obj_name);
            if (go != null && go.gameObject != null)
            {
                Destroy(go.gameObject);
            }
        }
    }
}