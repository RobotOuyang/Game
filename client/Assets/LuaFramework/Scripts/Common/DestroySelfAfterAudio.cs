using UnityEngine;

public class DestroySelfAfterAudio : MonoBehaviour
{
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }


	void Update ()
    {
	    if (source == null || !source.isPlaying)
        {
            Destroy(gameObject);
        }
	}
}
