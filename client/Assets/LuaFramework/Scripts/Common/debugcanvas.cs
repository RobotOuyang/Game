using UnityEngine;
using System.Collections.Generic;

public class debugcanvas : MonoBehaviour {
    public List<string> cons = new List<string>();

    public Vector3 get_pos;
    public Vector3 set_pos;
    public bool is_set = false;

    void Update()
    {
        get_pos = transform.position;
        if (is_set) transform.position = set_pos;
    }
}
