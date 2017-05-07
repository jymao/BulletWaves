using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusMinion : MonoBehaviour {

    public int maxHealth;
    private int health;

    public int bulletSpeed;
    private bool isDead = false;

    private float lastShot = 0;
    private float attackCooldown = 2f;
    private int attackLines = 10; //how many lines for one wave of attack

    //Prefabs
    public GameObject bullet;

	// Use this for initialization
	void Start () {
        health = maxHealth;	
	}
	
	// Update is called once per frame
	void Update () {
		
        if(!isDead && health == 0)
        {
            isDead = true;
            StartCoroutine(Death());
        }

        if(!isDead && Time.time - lastShot > attackCooldown)
        {
            StartCoroutine(FireStraight(attackLines));
            lastShot = Time.time;
        }
	}

    public void SetAttackLine(int lines)
    {
        attackLines = lines;
    }

    private void DecreaseHealth(int amount)
    {
        health -= amount;
        if (health < 0)
            health = 0;
    }

    private IEnumerator Death()
    {
        //fade out of view
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color prevColor = sprite.color;

        for (int i = 0; i < 5; i++)
        {
            prevColor = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a - 0.2f);
            sprite.color = prevColor;
            yield return new WaitForSeconds(0.3f);
        }

        //despawn
        Destroy(gameObject);
    }

    //Change sprite color to show boss has been damaged
    private IEnumerator DamageAnimation()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color original = sprite.color;
        Color damaged = new Color(1f, 0, 0);

        sprite.color = damaged;
        yield return new WaitForSeconds(0.01f);
        sprite.color = original;
    }

    //damage from player bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.tag == "PlayerBullet" && !isDead)
        {
            GameObject player = GameObject.Find("PlayerShip");
            Player pScript = player.GetComponent<Player>();
            DecreaseHealth(pScript.power);
            StartCoroutine(DamageAnimation());
        }
        
    }

    /*-- attack patterns ------------------------------------- */

    public IEnumerator FireStraight(int lines)
    {
        yield return new WaitForSeconds(1);

        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 0, -1);

        Vector3 direction = new Vector3(1, 0, 0);
        float angleSlice = 360.0f / lines;

        int length = 1; //how long each line is
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < lines; j++)
            {
                direction = Quaternion.AngleAxis(-angleSlice, new Vector3(0, 0, 1)) * direction;

                GameObject currBullet = (GameObject)Instantiate(bullet, spawnLoc, Quaternion.identity);
                currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x, direction.y) * bulletSpeed;           
            }

            //reset direction for next wave
            direction.x = 1;
            direction.y = 0;

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1);
    }
}
