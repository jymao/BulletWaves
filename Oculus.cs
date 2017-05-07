using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oculus : MonoBehaviour {

    public int maxHealth;
    private int health;
    public int power;

    public int bulletSpeed;
    private int lineLength = 8; //length of each attack line

    private Transform healthBar;
    private float initBarPos;

    private Animator animate;
    private Player player;
    private GameManager gm;

    private bool canShoot = true;
    private bool ready = false;  //ready for attacking/taking damage
    private bool hardMode = false; //boss on low health

    //movement
    private float lastMoved = 0;
    private float moveCooldown = 5f;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero; //for smooth damp

    //afterimage trail effect
    private float lastEmitted = 0;
    private float emitCooldown = 0.05f;

    //attack
    private float lastAttacked = 0;
    private float attackCooldown = 5.7f;

    //Bounds that boss is allowed to move in
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;

    //Prefabs
    public GameObject bullet;
    public GameObject smallExplosion;
    public GameObject mediumExplosion;
    public GameObject afterimage;
    public GameObject minion;

    // Use this for initialization
    void Start()
    {
        animate = GetComponent<Animator>();
        player = GameObject.Find("PlayerShip").GetComponent<Player>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        health = maxHealth;

        if (gm.GetIsNormalMode())
        {
            healthBar = GameObject.Find("BossHealth").transform.GetChild(1);
            initBarPos = healthBar.localPosition.x;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!animate.GetBool("isDead"))
        {
            //If player has died, stop firing, otherwise minions will keep spawning eventually
            if(gm.GetIsPlayerDead())
            {
                canShoot = false;
            }

            //Death
            if (health <= 0 && ready)
            {
                Death();
            }

            //Low health AI mode, or on endless mode
            if ((health < maxHealth / 2 || !gm.GetIsNormalMode()) && ready)
            {
                hardMode = true;
                lineLength = 4;
                moveCooldown = 3f;
                attackCooldown = 3.7f;
            }

            //Attacking
            if(Time.time - lastAttacked > attackCooldown && canShoot && ready)
            {
                ChooseAttackPattern();
                lastAttacked = Time.time;
            }

            //Movement
            if (Time.time - lastMoved > moveCooldown && ready)
            {
                float x = Random.Range(leftBound, rightBound);
                float y = Random.Range(bottomBound, topBound);
                targetPosition = new Vector3(x, y, 0);

                lastMoved = Time.time;
            }

            //Afterimage effect
            if (Time.time - lastEmitted > emitCooldown)
            {
                Instantiate(afterimage, transform.position, Quaternion.identity);
                lastEmitted = Time.time;
            }
        }
    }

    //Movement
    void FixedUpdate()
    {
        if (ready && !animate.GetBool("isDead"))
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.5f);
        }
    }

    public void SetCanShoot(bool b)
    {
        canShoot = b;
    }

    public int GetHealth()
    {
        return health;
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
            healthBar.localPosition = new Vector3(initBarPos - (((1 - currHealthPerc) * ((RectTransform)healthBar).rect.width) / 2), healthBar.localPosition.y, healthBar.localPosition.z);
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
            healthBar.localPosition = new Vector3(initBarPos - (((1 - currHealthPerc) * ((RectTransform)healthBar).rect.width) / 2), healthBar.localPosition.y, healthBar.localPosition.z);
        }
    }

    //Boss intro health filling and setting ready state
    public IEnumerator FillHealth()
    {
        //this call seems to allow the script time to initialize
        yield return new WaitForSeconds(0.5f);
        
        //Empty bar before filling animation
        DecreaseHealth(maxHealth);

        GameObject bossName = GameObject.Find("BossName");
        GameObject bossBarBack = GameObject.Find("BossHealth").transform.GetChild(0).gameObject;
        GameObject bossBarFront = GameObject.Find("BossHealth").transform.GetChild(1).gameObject;

        //Show name and health bar
        bossName.GetComponent<CanvasRenderer>().SetAlpha(1);
        bossBarBack.GetComponent<CanvasRenderer>().SetAlpha(1);
        bossBarFront.GetComponent<CanvasRenderer>().SetAlpha(1);

        //Fill bar
        for(int i = 0; i < 100; i++)
        {
            IncreaseHealth(maxHealth / 100);
            yield return new WaitForSeconds(0.01f);
        }

        targetPosition = transform.position; //init target pos
        ready = true;

        gm.SetGamePlaying(true);
    }

    //Boss is ready for the game to start. Called in Endless mode as opposed to FillHealth
    public IEnumerator SetIsReady()
    {
        yield return new WaitForSeconds(3f);

        targetPosition = transform.position; //init target pos
        ready = true;

        gm.SetGamePlaying(true);
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

    private void Death()
    {
        animate.SetBool("isDead", true);

        //destroy all minions
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if(enemy != gameObject)
            {
                Destroy(enemy);
            }
        }

        //destroy all bullets currently on screen
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("EnemyBullet"))
        {
            Destroy(bullet);
        }

        StartCoroutine(DeathExplosion());
    }

    private IEnumerator DeathExplosion()
    {
        //let death animation play a little
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < 2; i++)
        {
            //spawn explosions with pause in between
            Instantiate(smallExplosion, transform.position + new Vector3(0.5f, 0, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
            Instantiate(mediumExplosion, transform.position + new Vector3(-0.5f, 0.5f, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
            Instantiate(smallExplosion, transform.position + new Vector3(0.2f, 0.2f, 0), Quaternion.identity);
            Instantiate(mediumExplosion, transform.position + new Vector3(-0.3f, -0.5f, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
            Instantiate(smallExplosion, transform.position + new Vector3(0.1f, -0.3f, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
            Instantiate(mediumExplosion, transform.position + new Vector3(0, 0.2f, 0), Quaternion.identity);
            Instantiate(smallExplosion, transform.position + new Vector3(-0.5f, -0.2f, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }

        //fade out of view
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color prevColor = sprite.color;

        for(int i = 0; i < 5; i++)
        {
            prevColor = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a - 0.2f);
            sprite.color = prevColor;
            yield return new WaitForSeconds(0.3f);
        }

        //despawn
        Destroy(gameObject);
    }

    //damage from player bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        if (ready && !animate.GetBool("isDead"))
        {
            if (other.tag == "PlayerBullet" && gm.GetIsNormalMode())
            {
                DecreaseHealth(player.power);
                StartCoroutine(DamageAnimation());
            }
            else if (other.tag == "PlayerBullet" && !gm.GetIsNormalMode())
            {
                gm.UpdateScore(player.power);
                StartCoroutine(DamageAnimation());
            }
        }
    }

    /*-- attack patterns ------------------------------------- */

    public IEnumerator FireStraight(int lines)
    {
        animate.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        Vector3 spawnLoc = gameObject.transform.position + new Vector3(0, 0, -1);

        Vector3 direction = new Vector3(1, 0, 0);
        float angleSlice = 360.0f / lines;

        for (int i = 0; i < lineLength; i++)
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
        float angleSlice = 360.0f / lines;

        for (int i = 0; i < lineLength; i++)
        {
            for (int j = 0; j < lines; j++)
            {
                direction = Quaternion.AngleAxis(-angleSlice, new Vector3(0, 0, 1)) * direction;

                if (canShoot)
                {
                    GameObject currBullet = (GameObject)Instantiate(bullet, spawnLoc, Quaternion.identity);
                    currBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x, direction.y) * bulletSpeed;

                    BossBullet bScript = currBullet.GetComponent<BossBullet>();
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

    private void ChooseAttackPattern()
    {
        int pattern = Random.Range(0, 6);
        int lines = Random.Range(5, 11);

        switch(pattern)
        {
            //fire straight
            case 0:
                StartCoroutine(FireStraight(lines));
                break;
            //fire curve left
            case 1:
                StartCoroutine(FireCurved(lines, true));
                break;
            //fire curve right
            case 2:
                StartCoroutine(FireCurved(lines, false));
                break;
            //spawn minion
            case 3:
                GameObject currMinion = (GameObject)Instantiate(minion, transform.position, Quaternion.identity);

                if(hardMode)
                {
                    OculusMinion script = currMinion.GetComponent<OculusMinion>();
                    script.SetAttackLine(20);
                }
                break;
            //fire straight plus curve
            case 4:
                StartCoroutine(FireStraight(lines));
                StartCoroutine(FireCurved(lines, true));
                break;
            //fire both curves
            case 5:
                StartCoroutine(FireCurved(lines, true));
                StartCoroutine(FireCurved(lines, false));
                break;
            default:
                break;
        }
    }

}
