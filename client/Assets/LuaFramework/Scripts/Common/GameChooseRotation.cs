using UnityEngine;
using System.Collections;

public class GameChooseRotation : MonoBehaviour {
    [Range(0, 360)]
    public float angle = 0;
    public GameObject obj;

    public int child_count = 12;
    int start_obj = 0;

    float minz;
    // Use this for initialization
    void Start() {
        minz = obj.transform.GetChild(start_obj).position.z;
        SetAngle(angle);
    }

    public void SetIndex(int index)
    {
        start_obj = index;
        SetAngle(index * GetSingleAngle());
    }

    public void SetChildCount(float count)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject trans = obj.transform.GetChild(i).gameObject;
            trans.SetActive(i < count);
        }
    }

    public float GetSingleAngle()
    {
        if (obj.transform.childCount > 0)
        {
            return 360f / obj.transform.childCount;
        }
        return 0;
    }

	// Update is called once per frame
	public void SetAngle (float val)
    {
        angle = val;

        Vector3 vec = obj.transform.localEulerAngles;
        vec.y = angle;
        obj.transform.localEulerAngles = vec;

        for(int i = 0; i < obj.transform.childCount; i++)
        {
            Transform trans = obj.transform.GetChild(i);
            Vector3 temp = trans.localEulerAngles;
            temp.y = -angle;
            trans.localEulerAngles = temp;

            // 根据距离设置alpha
            SpriteRenderer[] renders = trans.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer render in renders)
            {
                Color color = render.color;
                float alpha = (1 - Mathf.Abs(minz - trans.position.z) / 3);
                if (alpha < 0 )
                {
                    alpha = 0;
                }
                color.r = color.g = color.b = alpha;
                render.color = color;
            }
        }
    } 
}
