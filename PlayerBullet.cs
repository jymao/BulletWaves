using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour {

    Vector2 storedVelocity;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    //for hitting the boss
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name == "Boss")
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

    public void Pause()
    {
        storedVelocity = gameObject.GetComponent<Rigidbody2D>().velocity;
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
    }

    public void Resume()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = storedVelocity;
    }
}
