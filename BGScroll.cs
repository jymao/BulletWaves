using UnityEngine;
using System.Collections;

public class BGScroll : MonoBehaviour {

    public float scrollSpeed;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

        float offset = Mathf.Repeat(Time.time * scrollSpeed, 1);
        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, offset));
        
	}
}
