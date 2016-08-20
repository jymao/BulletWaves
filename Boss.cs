using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour {

    public int health;
    public int maxHealth;
    public int power;

    public GameObject bullet;
    public int bulletSpeed;

    public Transform healthBar;
    private float maxHealthScale;
    private float initBarPos;

    private Animator animate;

    private bool isNormalMode = true;
    private bool canShoot = true;

    public GameObject gameManager;
    private GameManager gameScript;

	// Use this for initialization
	void Start () {
        animate = GetComponent<Animator>();
        maxHealthScale = healthBar.localScale.x;
        initBarPos = healthBar.position.x;
        gameScript = gameManager.GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
	    
        if (health <= 0)
        {
            animate.SetBool("isDead", true);
        }
        else
        {
            animate.SetBool("isDead", false);
        }   
        
	}

    public void EnableAnimation(bool b)
    {
        animate.enabled = b;
    }

    public void ResetIdle(bool b)
    {
        animate.SetBool("resetBoss", b);
    }

    public void SetIsNormalMode(bool b)
    {
        isNormalMode = b;
    }

    public void SetCanShoot(bool b)
    {
        canShoot = b;
    }

    public void increaseHealth(int amount)
    {
        if (health < maxHealth)
        {
            health += amount;

            if(health > maxHealth)
            {
                health = maxHealth;
            }

            float currHealthPerc = health / (float)maxHealth;
            healthBar.localScale = new Vector3(maxHealthScale * currHealthPerc, healthBar.localScale.y, healthBar.localScale.z);
            healthBar.position = new Vector3(initBarPos - ((maxHealthScale - healthBar.localScale.x) / 2), healthBar.position.y, healthBar.position.z);
        }
    }

    public void decreaseHealth(int amount)
    {
        if(health > 0)
        {
            health -= amount;

            if(health < 0)
            {
                health = 0;
            }

            float currHealthPerc = health / (float)maxHealth;
            healthBar.localScale = new Vector3(maxHealthScale * currHealthPerc, healthBar.localScale.y, healthBar.localScale.z);
            healthBar.position = new Vector3(initBarPos - ((maxHealthScale - healthBar.localScale.x) / 2), healthBar.position.y, healthBar.position.z);
        }
    }

    //damage from player bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PlayerBullet" && isNormalMode)
        {
            GameObject player = GameObject.Find("Player");
            Player pScript = player.GetComponent<Player>();
            decreaseHealth(pScript.power);
        }
        else if (other.tag == "PlayerBullet" && !isNormalMode)
        {
            GameObject player = GameObject.Find("Player");
            Player pScript = player.GetComponent<Player>();
            gameScript.UpdateScore(pScript.power);
        }
    }

    /*-- attack patterns ------------------------------------- */

    public IEnumerator FireStraight(int lines)
    {
        animate.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 0, -1);

        Vector3 direction = new Vector3(1, 0, 0);
        float angleSlice = 180.0f / (lines + 1);

        int length = 8; //how long each line is
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < lines; j++)
            {
                direction = Quaternion.AngleAxis(-angleSlice, new Vector3(0, 0, 1)) * direction;

                if (canShoot)
                {
                    GameObject currBullet = (GameObject)Instantiate(bullet, spawnLoc, Quaternion.identity);
                    currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x, direction.y) * bulletSpeed;
                }
            }

            //reset direction for next wave
            direction.x = 1;
            direction.y = 0;

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1);

        animate.SetBool("isAttacking", false);
    }

    public IEnumerator FireCurved(int lines, bool curveLeft)
    {
        animate.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 0, -1);

        Vector3 direction = new Vector3(1, 0, 0);
        float angleSlice = 180.0f / (lines + 1);

        int length = 8; //how long each line is
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < lines; j++)
            {
                direction = Quaternion.AngleAxis(-angleSlice, new Vector3(0, 0, 1)) * direction;

                if (canShoot)
                {
                    GameObject currBullet = (GameObject)Instantiate(bullet, spawnLoc, Quaternion.identity);
                    currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x, direction.y) * bulletSpeed;

                    BossBullet bScript = currBullet.GetComponent<BossBullet>();
                    bScript.SetInitDirection(direction);
                    if (curveLeft)
                    {
                        bScript.SetType(2);
                    }
                    else
                    {
                        bScript.SetType(3);
                    }
                }
            }

            //reset direction for next wave
            direction.x = 1;
            direction.y = 0;

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1);

        animate.SetBool("isAttacking", false);
    }

    public IEnumerator FirePathY()
    {
        animate.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        Vector3 spawnLoc1 = gameObject.transform.position + new Vector3(100, 0, -1);
        Vector3 spawnLoc2 = gameObject.transform.position + new Vector3(-100, 0, -1);

        int length = 15; //how long each line is
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3 spawnLoc;

                if(j % 2 == 0)
                {
                    spawnLoc = spawnLoc1;
                }
                else
                {
                    spawnLoc = spawnLoc2;
                }

                if (canShoot)
                {
                    GameObject currBullet = (GameObject)Instantiate(bullet, spawnLoc, Quaternion.identity);
                    currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(-1, -1) * bulletSpeed;

                    BossBullet bScript = currBullet.GetComponent<BossBullet>();
                    bScript.SetType(4);
                }
            }

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1);

        animate.SetBool("isAttacking", false);
    }

    public IEnumerator FirePathX()
    {
        animate.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        Vector3 spawnLoc1 = gameObject.transform.position + new Vector3(300, -150, -1);
        Vector3 spawnLoc2 = gameObject.transform.position + new Vector3(-300, -350, -1);

        int length = 25; //how long each line is
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameObject currBullet;

                if (canShoot)
                {
                    if (j % 2 == 0)
                    {
                        currBullet = (GameObject)Instantiate(bullet, spawnLoc1, Quaternion.identity);
                        currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(-1, -1) * bulletSpeed;
                    }
                    else
                    {
                        currBullet = (GameObject)Instantiate(bullet, spawnLoc2, Quaternion.identity);
                        currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(1, 1) * bulletSpeed;
                    }


                    BossBullet bScript = currBullet.GetComponent<BossBullet>();
                    bScript.SetType(5);
                }
            }

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1);

        animate.SetBool("isAttacking", false);
    }

    public IEnumerator FireCircle(int waves)
    {
        animate.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 0, -1);

        Vector3 direction = new Vector3(1, 0, 0);
        float angleGap = 40; //gap in each waves
        int bulletsPerWave = 70;

        for (int i = 0; i < waves; i++)
        {
            bool gapDone = false;

            for (int j = 0; j < bulletsPerWave; j++)
            {
                if (!gapDone && Random.Range(0, bulletsPerWave) == 1)
                {
                    direction = Quaternion.AngleAxis(-angleGap, new Vector3(0, 0, 1)) * direction;
                    gapDone = true;
                }
                else if(!gapDone && j == bulletsPerWave - 1)
                {
                    direction = Quaternion.AngleAxis(-angleGap, new Vector3(0, 0, 1)) * direction;
                }
                else
                {
                    direction = Quaternion.AngleAxis(-2, new Vector3(0, 0, 1)) * direction;
                }

                if (canShoot)
                {
                    GameObject currBullet = (GameObject)Instantiate(bullet, spawnLoc, Quaternion.identity);
                    currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x, direction.y) * bulletSpeed;
                }
            }

            //reset direction for next wave
            direction.x = 1;
            direction.y = 0;

            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(1);

        animate.SetBool("isAttacking", false);
    }

}
