using UnityEngine;
using System.Collections;

public class BossBullet : MonoBehaviour {

    /* type 1: no velocity change
     * type 2: + constant velocity change
     * type 3: - constant velocity change
     * type 4: irregular x velocity change
     * type 5: irregular y velocity change
     */
    private int type = 1;
    private Rigidbody2D rBody;

    private Vector3 initDir;
    private Vector2 curveVelocity;

    private float initX;
    private float initY;
    private float MAX_SWEEP_DISTX = 150;
    private float MAX_SWEEP_DISTY = 80;

    private Vector2 storedVelocity;
    private bool pause = false;

	// Use this for initialization
	void Start () {
        rBody = gameObject.GetComponent<Rigidbody2D>();
        initX = gameObject.transform.position.x;
        initY = gameObject.transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {

        if (!pause)
        {

            if (type == 2)
            {
                rBody.velocity += curveVelocity;
            }
            else if (type == 3)
            {
                rBody.velocity -= curveVelocity;
            }
            else if (type == 4)
            {
                if (gameObject.transform.position.x < initX - MAX_SWEEP_DISTX)
                {
                    //Note: velocity.y is negative
                    rBody.velocity = new Vector2(2 * -rBody.velocity.y, rBody.velocity.y);
                }
                if (gameObject.transform.position.x > initX + MAX_SWEEP_DISTX)
                {
                    rBody.velocity = new Vector2(2 * rBody.velocity.y, rBody.velocity.y);
                }

            }
            else if (type == 5)
            {
                if (gameObject.transform.position.y < initY - MAX_SWEEP_DISTY)
                {
                    //Need two cases since PathX pattern has two opposite beams
                    if (rBody.velocity.x < 0)
                    {
                        rBody.velocity = new Vector2(rBody.velocity.x, 2 * -rBody.velocity.x);
                    }
                    else
                    {
                        rBody.velocity = new Vector2(rBody.velocity.x, 2 * rBody.velocity.x);
                    }
                }
                if (gameObject.transform.position.y > initY + MAX_SWEEP_DISTY)
                {
                    if (rBody.velocity.x < 0)
                    {
                        rBody.velocity = new Vector2(rBody.velocity.x, 2 * rBody.velocity.x);
                    }
                    else
                    {
                        rBody.velocity = new Vector2(rBody.velocity.x, 2 * -rBody.velocity.x);
                    }
                }

            }

        }
	}

    public void SetType(int type)
    {
        this.type = type;
    }

    public void SetInitDirection(Vector3 v)
    {
        initDir = v;

        Vector3 rotate = Quaternion.AngleAxis(-90, new Vector3(0, 0, 1)) * initDir;
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
        if(other.gameObject.name == "Player")
        {
            Destroy(gameObject);
        }
    }

    public void Pause()
    {
        if (!pause)
        {
            storedVelocity = gameObject.GetComponent<Rigidbody2D>().velocity;
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        }
        pause = true;
    }

    public void Resume()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = storedVelocity;
        pause = false;
    }
}
