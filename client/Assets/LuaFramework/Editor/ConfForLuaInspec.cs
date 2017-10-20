using UnityEditor;
using LuaFramework;
using System.Collections.Generic;

[CustomEditor(typeof(ConfForLua))]
public class ConfForLuaInspec : Editor
{
    ConfForLua conf;
    public Dictionary<string, double> m_varlist;

    void OnEnable()
    {
        conf = target as ConfForLua;
        m_varlist = conf.m_varlist;
    }

    public override void OnInspectorGUI()
    {
        Dictionary<string, double> temp = new Dictionary<string, double>();
        foreach (var kv in m_varlist)
        {
            temp.Add(kv.Key, EditorGUILayout.DoubleField(kv.Key, kv.Value));
        }
        foreach (var kv in temp)
        {
            if (kv.Value != m_varlist[kv.Key])
            {
                conf.ChangeValue(kv.Key, kv.Value);
            }
        }
    }
}
