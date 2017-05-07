using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    //for hitting the boss
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }

    //for out of bounds
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Border")
        {
            Destroy(gameObject);
        }
    }
}
