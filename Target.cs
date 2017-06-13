using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    private bool isTracking = true;
    private GameObject player;

	// Use this for initialization
	void Start () {

        player = GameObject.Find("PlayerShip");

	}
	
	// Update is called once per frame
	void Update () {
		
        if(isTracking)
        {
            transform.position = player.transform.position;
        }

	}

    public void SetTracking(bool b)
    {
        isTracking = b;
    }
}
