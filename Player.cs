using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public int maxHealth;
    private int health;

    public int moveSpeed;
    public int power;

    public Transform healthBar;
    private float initBarPos;

    public int bulletSpeed;

    private Rigidbody2D rigidBody;
    private Vector2 movementVector;

    private float lastShot = 0;
    private float fireCooldown = 0.2f;

    private float lastDamaged = 0;
    private float invinCooldown = 0.8f;

    private bool canMove = true;
    private bool isDead = false;

    //Prefabs
    public GameObject bullet;
    public GameObject smallExplosion;

	// Use this for initialization
	void Start () {
        health = maxHealth;
        rigidBody = GetComponent<Rigidbody2D>();
        initBarPos = healthBar.localPosition.x;
	}
	
	// Update is called once per frame
	void Update () {

        //Death
        if(health == 0 && !isDead)
        {
            isDead = true;
            canMove = false;
            StartCoroutine(DeathExplosion());
        }

        //Movement and attacking
        if (canMove)
        {
            movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            if (Input.GetKey(KeyCode.Space))
            {
                if (Time.time - lastShot > fireCooldown)
                {
                    lastShot = Time.time;
                    FireBullet();
                }
            }
        }
	}

    //Movement
    void FixedUpdate()
    {
        if (canMove)
        {
            rigidBody.MovePosition(rigidBody.position + movementVector * moveSpeed * Time.deltaTime);       
        }
    }

    public int GetHealth()
    {
        return health;
    }

    private void FireBullet()
    {
        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 0.5f, -1);

        GameObject currBullet = (GameObject) Instantiate(bullet, spawnLoc, Quaternion.identity);
        currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1) * bulletSpeed;
    }

    private void IncreaseHealth(int amount)
    {
        if (health < maxHealth)
        {
            health += amount;

            if (health > maxHealth)
            {
                health = maxHealth;
            }

            float currHealthPerc = health / (float)maxHealth;
            healthBar.localScale = new Vector3(currHealthPerc, healthBar.localScale.y, healthBar.localScale.z);
            healthBar.localPosition = new Vector3(initBarPos + (((1 - currHealthPerc) * ((RectTransform)healthBar).rect.width) / 2), healthBar.localPosition.y, healthBar.localPosition.z);
        }
    }

    private void DecreaseHealth(int amount)
    {
        if (health > 0)
        {
            health -= amount;

            if (health < 0)
            {
                health = 0;
            }

            float currHealthPerc = health / (float)maxHealth;
            healthBar.localScale = new Vector3(currHealthPerc, healthBar.localScale.y, healthBar.localScale.z);
            healthBar.localPosition = new Vector3(initBarPos + (((1 - currHealthPerc) * ((RectTransform)healthBar).rect.width) / 2), healthBar.localPosition.y, healthBar.localPosition.z);
        }
    }

    //damage from enemy and bullets
    void OnTriggerStay2D(Collider2D other)
    {
        if ((other.tag == "Enemy" || other.tag == "EnemyBullet") && health != 0)
        {
            if (Time.time - lastDamaged > invinCooldown)
            {
                GameObject gm = GameObject.Find("GameManager");
                GameManager gmScript = gm.GetComponent<GameManager>();
                DecreaseHealth(gmScript.GetBossPower());
                StartCoroutine(DamageAnimation());
                lastDamaged = Time.time;
            }
        }
    }

    //Change sprite color to show player has been damaged
    //Note: if invincCooldown is too low, original color will become damaged color
    private IEnumerator DamageAnimation()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color original = sprite.color;
        Color damaged = new Color(1f, 0, 0);

        for (int i = 0; i < 3; i++)
        {
            sprite.color = damaged;
            yield return new WaitForSeconds(0.1f);
            sprite.color = original;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DeathExplosion()
    {
        yield return new WaitForSeconds(1f);
        Instantiate(smallExplosion, transform.position + new Vector3(0.1f, -0.1f, 0), Quaternion.identity);
        Instantiate(smallExplosion, transform.position + new Vector3(-0.2f, 0, 0), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        Instantiate(smallExplosion, transform.position + new Vector3(0.1f, 0.1f, 0), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        Instantiate(smallExplosion, transform.position + new Vector3(-0.1f, 0.1f, 0), Quaternion.identity);
        Instantiate(smallExplosion, transform.position + new Vector3(0.1f, 0, 0), Quaternion.identity);

        Destroy(gameObject);
    }
}
