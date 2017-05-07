using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusAfterimage : MonoBehaviour {

    private SpriteRenderer sprite;
    private float initTime;
    private float lifetime = 0.5f;

	// Use this for initialization
	void Start () {
        sprite = GetComponent<SpriteRenderer>();
        initTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - 0.05f);

        if(Time.time - initTime > lifetime)
        {
            Destroy(gameObject);
        }
	}
}
