using UnityEngine;
using System.Collections;

public class BossBullet : MonoBehaviour {

    /* type 1: no velocity change (straight pattern)
     * type 2: + constant velocity change (curve left)
     * type 3: - constant velocity change (curve right)
     */
    private int type = 1;
    private Rigidbody2D rBody;

    private Vector2 curveVelocity;

    private float lastActivate = 0;
    private float cooldown = 0.5f;

	// Use this for initialization
	void Start () {
        rBody = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        
        if (Time.time - lastActivate > cooldown)
        {
            if (type == 2)
            {
                rBody.velocity += curveVelocity;
                ReCalcDirection();
            }
            else if (type == 3)
            {
                rBody.velocity -= curveVelocity;
                ReCalcDirection();
            }

            lastActivate = Time.time;
        }   
	}

    public void SetType(int type)
    {
        this.type = type;
    }

    //Rotate bullet direction for curved patterns
    private void ReCalcDirection()
    {
        Vector3 rotate = Quaternion.AngleAxis(-90, new Vector3(0, 0, 1)) * new Vector3(rBody.velocity.x, rBody.velocity.y, 0);
        curveVelocity = new Vector2(rotate.x, rotate.y);
    }

    //for out of bounds
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Border")
        {
            Destroy(gameObject);
        }
    }

    //for hitting the player
    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.name == "PlayerShip")
        {
            Destroy(gameObject);
        }
    }
}
