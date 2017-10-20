using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowTips : MonoBehaviour {

    public Text m_text;

    public string[] m_tips;
    public float interval;

	// Use this for initialization
	void Start () {
        if (m_tips.Length >= 1)
        {
            StartCoroutine(StartTips());
        }
	}
	
    IEnumerator StartTips()
    {
        while (true)
        {
            m_text.text = m_tips[Random.Range(0, m_tips.Length)];
            yield return new WaitForSeconds(interval);
        }
    }
}
