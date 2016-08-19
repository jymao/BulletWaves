using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public int health;
    public int maxHealth;
    public int moveSpeed;
    public int power;

    public Transform healthBar;
    private float maxHealthScale;
    private float initBarPos;

    public GameObject bullet;
    public int bulletSpeed;

    private Rigidbody2D rigidBody;

    private bool left = false;
    private bool right = false;
    private bool up = false;
    private bool down = false;

    private float lastShot = 0;
    private bool canMove = false;

    private Animator animate;

	// Use this for initialization
	void Start () {
        animate = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        maxHealthScale = healthBar.localScale.x;
        initBarPos = healthBar.position.x;
	}
	
	// Update is called once per frame
	void Update () {

        if (canMove)
        {

            if (Input.GetKey(KeyCode.LeftArrow)) { left = true; }
            else { left = false; }

            if (Input.GetKey(KeyCode.RightArrow)) { right = true; }
            else { right = false; }

            if (Input.GetKey(KeyCode.UpArrow)) { up = true; }
            else { up = false; }

            if (Input.GetKey(KeyCode.DownArrow)) { down = true; }
            else { down = false; }

            if (Input.GetKey(KeyCode.Space))
            {
                if (Time.time - lastShot > 0.2)
                {
                    lastShot = Time.time;
                    FireBullet();
                }
            }

        }
	}

    void FixedUpdate()
    {
        Vector2 finalPosition = rigidBody.position;

        if (canMove)
        {

            if (left)
            {
                finalPosition += new Vector2(-moveSpeed, 0) * Time.deltaTime;
            }
            if (right)
            {
                finalPosition += new Vector2(moveSpeed, 0) * Time.deltaTime;
            }
            if (up)
            {
                finalPosition += new Vector2(0, moveSpeed) * Time.deltaTime;
            }
            if (down)
            {
                finalPosition += new Vector2(0, -moveSpeed) * Time.deltaTime;
            }
        }

        rigidBody.MovePosition(finalPosition);
    }

    public void SetMove(bool b)
    {
        canMove = b;
    }

    public void EnableAnimation(bool b)
    {
        animate.enabled = b;
    }

    private void FireBullet()
    {
        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 30, -1);

        GameObject currBullet = (GameObject) Instantiate(bullet, spawnLoc, Quaternion.identity);
        currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1) * bulletSpeed;
    }

    public void increaseHealth(int amount)
    {
        if (health < maxHealth)
        {
            health += amount;

            if (health > maxHealth)
            {
                health = maxHealth;
            }

            float currHealthPerc = health / (float)maxHealth;
            healthBar.localScale = new Vector3(maxHealthScale * currHealthPerc, healthBar.localScale.y, healthBar.localScale.z);
            healthBar.position = new Vector3(initBarPos + ((maxHealthScale - healthBar.localScale.x) / 2), healthBar.position.y, healthBar.position.z);
        }
    }

    public void decreaseHealth(int amount)
    {
        if (health > 0)
        {
            health -= amount;

            if (health < 0)
            {
                health = 0;
            }

            float currHealthPerc = health / (float)maxHealth;
            healthBar.localScale = new Vector3(maxHealthScale * currHealthPerc, healthBar.localScale.y, healthBar.localScale.z);
            healthBar.position = new Vector3(initBarPos + ((maxHealthScale - healthBar.localScale.x) / 2), healthBar.position.y, healthBar.position.z);
        }
    }

    //damage from enemy bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "BossBullet")
        {
            GameObject boss = GameObject.Find("Boss");
            Boss bScript = boss.GetComponent<Boss>();
            decreaseHealth(bScript.power);
        }
    }
}
