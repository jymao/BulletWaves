using UnityEngine;
using System.Collections;

public class BGScroll : MonoBehaviour {

    public float scrollSpeed;
    private bool pause = false;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

        if (!pause)
        {
            float offset = Mathf.Repeat(Time.time * scrollSpeed, 1);
            GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, offset));
        }
	}

    public void Pause(bool b)
    {
        pause = b;
    }
}
