using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem : MonoBehaviour {

    public int maxHealth;
    private int health;
    public int power;

    public int speed;
    public int bulletSpeed;
    private int lineLength = 8; //length of each attack line

    private Transform healthBar;
    private float initBarPos;

    private Animator headAnimator;
    private Animator leftHandAnimator;
    private Animator rightHandAnimator;
    private Player player;
    private GameManager gm;

    private bool canShoot = true;
    private bool ready = false;  //ready for attacking/taking damage
    private bool hardMode = false; //boss on low health
    private bool isDead = false;

    //movement
    private float lastMoved = 0;
    private float moveCooldown = 3f;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero; //for smooth damp

    //attack
    private bool isAttacking = false; //different from animator's "isAttacking": the animator's bool is changed only for laser anims
    private float lastAttacked = 0;
    private float attackCooldown = 5f;
    private int attackCounter = 0; //used for choosing next attack pattern
    //moving hands individually during attack patterns
    private bool moveLeft = false;
    private bool moveRight = false;
    private float moveTime = 0.1f; //move to location within this time in seconds
    private Vector3 targetLeftPos;
    private Vector3 targetRightPos;
    private Vector3 targetVelocityLeft = Vector3.zero; //for smooth damp
    private Vector3 targetVelocityRight = Vector3.zero;

    //Bounds that boss is allowed to move in
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;

    //Golem parts
    private GameObject head;
    private GameObject leftHand; //Golem's left, not left of screen
    private GameObject rightHand;

    //Prefabs
    public GameObject bullet;
    public GameObject laser;
    public GameObject target;
    public GameObject smallExplosion;
    public GameObject mediumExplosion;

	// Use this for initialization
	void Start () {
        head = transform.GetChild(0).gameObject;
        leftHand = transform.GetChild(1).gameObject;
        rightHand = transform.GetChild(2).gameObject;

        headAnimator = head.GetComponent<Animator>();
        leftHandAnimator = leftHand.GetComponent<Animator>();
        rightHandAnimator = rightHand.GetComponent<Animator>();

        player = GameObject.Find("PlayerShip").GetComponent<Player>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        health = maxHealth;

        if (gm.GetIsNormalMode())
        {
            healthBar = GameObject.Find("BossHealth").transform.GetChild(1);
            initBarPos = healthBar.localPosition.x;
        }

        StartCoroutine(IdleHands());
	}
	
	// Update is called once per frame
	void Update () {

        if(!isDead)
        {
            //If player has died, stop attacking
            if (gm.GetIsPlayerDead())
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
                moveCooldown = 2f;
                attackCooldown = 3.7f;
            }

            //Attacking
            if (Time.time - lastAttacked > attackCooldown && canShoot && ready && !isAttacking)
            {
                ChooseAttackPattern();
                lastAttacked = Time.time;
            }

            //Movement
            if (Time.time - lastMoved > moveCooldown && ready)
            {
                float x = Random.Range(leftBound, rightBound);
                targetPosition = new Vector3(x, transform.position.y, 0);

                lastMoved = Time.time;
            }
        }
	}

    //Movement
    void FixedUpdate()
    {
        if (ready && !isDead && !isAttacking)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.5f);
        }

        if(moveLeft)
        {
            leftHand.transform.position = Vector3.SmoothDamp(leftHand.transform.position, targetLeftPos, ref targetVelocityLeft, moveTime);
        }
        if(moveRight)
        {
            rightHand.transform.position = Vector3.SmoothDamp(rightHand.transform.position, targetRightPos, ref targetVelocityRight, moveTime);
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
        for (int i = 0; i < 100; i++)
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
        SpriteRenderer headSprite = head.GetComponent<SpriteRenderer>();
        SpriteRenderer leftHandSprite = leftHand.GetComponent<SpriteRenderer>();
        SpriteRenderer rightHandSprite = rightHand.GetComponent<SpriteRenderer>();

        Color original = new Color(1f, 1f, 1f);
        Color damaged = new Color(1f, 0, 0);

        headSprite.color = damaged;
        leftHandSprite.color = damaged;
        rightHandSprite.color = damaged;
        yield return new WaitForSeconds(0.01f);
        headSprite.color = original;
        leftHandSprite.color = original;
        rightHandSprite.color = original;
    }

    private void Death()
    {
        isDead = true;

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
        SpriteRenderer headSprite = head.GetComponent<SpriteRenderer>();
        SpriteRenderer leftHandSprite = leftHand.GetComponent<SpriteRenderer>();
        SpriteRenderer rightHandSprite = rightHand.GetComponent<SpriteRenderer>();
        Color prevColor = headSprite.color;

        for (int i = 0; i < 5; i++)
        {
            prevColor = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a - 0.2f);
            headSprite.color = prevColor;
            leftHandSprite.color = prevColor;
            rightHandSprite.color = prevColor;
            yield return new WaitForSeconds(0.2f);
        }

        //despawn
        Destroy(gameObject);
    }

    //damage from player bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        if (ready && !isDead)
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

    //Idle hand circling motion
    private IEnumerator IdleHands()
    {
        while(!isAttacking && !isDead)
        {
            //boths hands go down
            for (int i = 0; i < 10; i++)
            {
                leftHand.transform.position += new Vector3(0, -0.04f, 0);
                rightHand.transform.position += new Vector3(0, -0.04f, 0);
                yield return new WaitForSeconds(0.01f);

            }
            //left hand goes right, right goes left
            for (int i = 0; i < 10; i++)
            {
                leftHand.transform.position += new Vector3(0.04f, 0, 0);
                rightHand.transform.position -= new Vector3(0.04f, 0, 0);
                yield return new WaitForSeconds(0.01f);

            }
            //both hands go up
            for (int i = 0; i < 20; i++)
            {
                leftHand.transform.position += new Vector3(0, 0.04f, 0);
                rightHand.transform.position += new Vector3(0, 0.04f, 0);
                yield return new WaitForSeconds(0.01f);

            }
            //hands return to original x coord
            for (int i = 0; i < 10; i++)
            {
                leftHand.transform.position += new Vector3(-0.04f, 0, 0);
                rightHand.transform.position -= new Vector3(-0.04f, 0, 0);
                yield return new WaitForSeconds(0.01f);

            }
            //both hands go down to original position
            for (int i = 0; i < 10; i++)
            {
                leftHand.transform.position += new Vector3(0, -0.04f, 0);
                rightHand.transform.position += new Vector3(0, -0.04f, 0);
                yield return new WaitForSeconds(0.01f);

            }
            
        }
    }

    /*-- attack patterns ------------------------------------- */

    //helper method for firing pattern
    public IEnumerator FireStraight(int lines, bool fromLeft, bool isHalf)
    {
        Vector3 spawnLoc;
        if(fromLeft)
        {
           spawnLoc = leftHand.transform.position + new Vector3(0.1f, -0.6f, -1);
        }
        else
        {
            spawnLoc = rightHand.transform.position + new Vector3(-0.1f, -0.6f, -1);
        }

        Vector3 direction = new Vector3(1, 0, 0);
        float angleSlice = 0;
        if (isHalf)
        {
            angleSlice = 180f / (lines + 1);
        }
        else
        {
            angleSlice = 360f / lines;
        }

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
    }

    //fire bullets from neutral palms, palms move to random position before firing
    private IEnumerator FireBullets()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.5f);

        //move hands to random spot within bounds
        moveTime = 0.4f; //slower moveTime for fairness
        Vector3 origLeftPos = leftHand.transform.position;
        Vector3 origRightPos = rightHand.transform.position;

        float xCoord = Random.Range(leftBound, rightBound);
        float yCoord = Random.Range(bottomBound, topBound);
        targetLeftPos = new Vector3(xCoord, yCoord, 0);

        xCoord = Random.Range(leftBound, rightBound);
        yCoord = Random.Range(bottomBound, topBound);
        targetRightPos = new Vector3(xCoord, yCoord, 0);

        moveLeft = true;
        moveRight = true;
        yield return new WaitForSeconds(1.5f);

        //fire
        StartCoroutine(FireStraight(20, true, false));
        StartCoroutine(FireStraight(20, false, false));
        yield return new WaitForSeconds(1.75f);

        //move hands back
        targetLeftPos = origLeftPos;
        targetRightPos = origRightPos;

        yield return new WaitForSeconds(1.5f);
        moveLeft = false;
        moveRight = false;
        isAttacking = false;
        StartCoroutine(IdleHands());
    }

    //fire alternating waves of bullets from neutral palms
    private IEnumerator FireWaves()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < 3; i++)
        {
            if(isDead)
            {
                break;
            }

            StartCoroutine(FireStraight(17, true, true));
            yield return new WaitForSeconds(2f);
            StartCoroutine(FireStraight(17, false, true));
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(1.75f);
        isAttacking = false;
        StartCoroutine(IdleHands());
    }

    //target and punch with fists
    private IEnumerator TargetPunch()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.5f);

        //change hands to fists and change box collider size
        leftHandAnimator.SetBool("isFist", true);
        rightHandAnimator.SetBool("isFist", true);
        BoxCollider2D leftColl = leftHand.GetComponent<BoxCollider2D>();
        BoxCollider2D rightColl = rightHand.GetComponent<BoxCollider2D>();
        Vector2 origLeftColl = leftColl.size;
        Vector2 origRightColl = rightColl.size;
        leftColl.size = new Vector2(origLeftColl.x, 0.25f);
        rightColl.size = new Vector2(origRightColl.x, 0.25f);

        //spawn target reticle to track player
        GameObject target1 = (GameObject)Instantiate(target, player.gameObject.transform.position, Quaternion.identity);
        GameObject target2 = (GameObject)Instantiate(target, player.gameObject.transform.position, Quaternion.identity);

        //store original hand positions
        Vector3 leftOrigPos = leftHand.transform.position;
        Vector3 rightOrigPos = rightHand.transform.position;

        moveTime = 0.1f;

        //stop tracking player and set target locations
        yield return new WaitForSeconds(1f);
        target1.GetComponent<Target>().SetTracking(false);
        targetLeftPos = target1.transform.position;

        yield return new WaitForSeconds(0.5f);
        moveLeft = true;  //allow fist to move
        Destroy(target1);  //remove target
        target2.GetComponent<Target>().SetTracking(false);
        targetRightPos = target2.transform.position;

        yield return new WaitForSeconds(0.5f);
        moveRight = true;
        Destroy(target2);

        //return to original position
        yield return new WaitForSeconds(1f);
        targetLeftPos = leftOrigPos;
        targetRightPos = rightOrigPos;
        yield return new WaitForSeconds(1.5f);
        moveLeft = false;
        moveRight = false;

        //change fists and collider back
        leftHandAnimator.SetBool("isFist", false);
        rightHandAnimator.SetBool("isFist", false);
        leftColl.size = origLeftColl;
        rightColl.size = origRightColl;
        isAttacking = false;
        StartCoroutine(IdleHands());
    }

    //clap hands together
    private IEnumerator ClapHands()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.5f);

        //change hands and change box collider size
        leftHandAnimator.SetBool("isClap", true);
        rightHandAnimator.SetBool("isClap", true);
        BoxCollider2D leftColl = leftHand.GetComponent<BoxCollider2D>();
        BoxCollider2D rightColl = rightHand.GetComponent<BoxCollider2D>();
        Vector2 origLeftColl = leftColl.size;
        Vector2 origRightColl = rightColl.size;
        leftColl.size = new Vector2(0.1f, origLeftColl.y);
        rightColl.size = new Vector2(0.1f, origRightColl.y);

        //store original hand positions
        Vector3 leftOrigPos = leftHand.transform.position;
        Vector3 rightOrigPos = rightHand.transform.position;

        moveTime = 0.1f;

        //coordinates for attack pattern
        Vector3 leftEdgePos = new Vector3(7, player.gameObject.transform.position.y, 0);
        Vector3 rightEdgePos = new Vector3(-7, player.gameObject.transform.position.y, 0);
        Vector3 leftMidPos = new Vector3(0.3f, player.gameObject.transform.position.y, 0);
        Vector3 rightMidPos = new Vector3(-0.3f, player.gameObject.transform.position.y, 0);

        //move to edge positions
        targetLeftPos = leftEdgePos;
        targetRightPos = rightEdgePos;
        yield return new WaitForSeconds(0.5f);
        moveLeft = true;
        moveRight = true;

        //move to middle for clap
        yield return new WaitForSeconds(0.5f);
        targetLeftPos = leftMidPos;
        targetRightPos = rightMidPos;

        //return to original position
        yield return new WaitForSeconds(0.5f);
        targetLeftPos = leftOrigPos;
        targetRightPos = rightOrigPos;
        yield return new WaitForSeconds(1.5f);
        moveLeft = false;
        moveRight = false;

        //change hands and collider back
        leftHandAnimator.SetBool("isClap", false);
        rightHandAnimator.SetBool("isClap", false);
        leftColl.size = origLeftColl;
        rightColl.size = origRightColl;
        isAttacking = false;
        StartCoroutine(IdleHands());
    }

    //fire lasers
    private IEnumerator FireLasers()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.5f);

        //charge body components
        leftHandAnimator.SetBool("isCharging", true);
        rightHandAnimator.SetBool("isCharging", true);
        headAnimator.SetBool("isOpening", true);

        //charge length
        yield return new WaitForSeconds(2.5f);
        //fire beams
        headAnimator.SetBool("isOpening", false);
        headAnimator.SetBool("isAttacking", true);
        leftHandAnimator.SetBool("isAttacking", true);
        rightHandAnimator.SetBool("isAttacking", true);
        leftHandAnimator.SetBool("isCharging", false);
        rightHandAnimator.SetBool("isCharging", false);

        GameObject leftHandLaser = (GameObject)Instantiate(laser, leftHand.transform.position + new Vector3(0.075f, -5.5f, 0), Quaternion.identity);
        GameObject rightHandLaser = (GameObject)Instantiate(laser, rightHand.transform.position + new Vector3(-0.075f, -5.5f, 0), Quaternion.identity);
        GameObject headLaser = (GameObject)Instantiate(laser, head.transform.position + new Vector3(0, -5.5f, 0), Quaternion.identity);
        headLaser.transform.localScale = new Vector3(8, 4, 1);

        //hold beam length
        yield return new WaitForSeconds(1.5f);

        //laser decay
        leftHandLaser.GetComponent<Animator>().SetBool("isDone", true);
        rightHandLaser.GetComponent<Animator>().SetBool("isDone", true);
        headLaser.GetComponent<Animator>().SetBool("isDone", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(leftHandLaser);
        Destroy(rightHandLaser);
        Destroy(headLaser);

        //return to original animations
        headAnimator.SetBool("isAttacking", false);
        leftHandAnimator.SetBool("isAttacking", false);
        rightHandAnimator.SetBool("isAttacking", false);
        isAttacking = false;
        StartCoroutine(IdleHands());
    }

    //fire lasers while moving palms
    private IEnumerator MoveLasers()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1.5f);

        //charge hands
        leftHandAnimator.SetBool("isCharging", true);
        rightHandAnimator.SetBool("isCharging", true);

        //charge length
        yield return new WaitForSeconds(2.5f);
        //fire beams
        leftHandAnimator.SetBool("isAttacking", true);
        rightHandAnimator.SetBool("isAttacking", true);
        leftHandAnimator.SetBool("isCharging", false);
        rightHandAnimator.SetBool("isCharging", false);

        GameObject leftHandLaser = (GameObject)Instantiate(laser, leftHand.transform.position + new Vector3(0.075f, -5.5f, 0), Quaternion.identity);
        GameObject rightHandLaser = (GameObject)Instantiate(laser, rightHand.transform.position + new Vector3(-0.075f, -5.5f, 0), Quaternion.identity);
        //make lasers child of hands so lasers will follow when hands move
        leftHandLaser.transform.parent = leftHand.transform;
        rightHandLaser.transform.parent = rightHand.transform;

        moveTime = 1;
        Vector3 leftOrigPos = leftHand.transform.position;
        Vector3 rightOrigPos = rightHand.transform.position;

        targetLeftPos = new Vector3(7, leftOrigPos.y, 0);
        targetRightPos = new Vector3(-7, rightOrigPos.y, 0);
        yield return new WaitForSeconds(0.5f);
        moveLeft = true;
        moveRight = true;

        yield return new WaitForSeconds(2f);
        moveTime = 1.75f;
        targetRightPos = new Vector3(5, rightOrigPos.y, 0);
        yield return new WaitForSeconds(3f);
        targetLeftPos = new Vector3(-5, leftOrigPos.y, 0);
        targetRightPos = new Vector3(-7, rightOrigPos.y, 0);
        yield return new WaitForSeconds(3f);

        //laser decay
        leftHandLaser.GetComponent<Animator>().SetBool("isDone", true);
        rightHandLaser.GetComponent<Animator>().SetBool("isDone", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(leftHandLaser);
        Destroy(rightHandLaser);
        yield return new WaitForSeconds(0.5f);

        //return to original animations
        leftHandAnimator.SetBool("isAttacking", false);
        rightHandAnimator.SetBool("isAttacking", false);
        moveTime = 0.1f;
        targetLeftPos = leftOrigPos;
        targetRightPos = rightOrigPos;
        yield return new WaitForSeconds(1.5f);
        moveLeft = false;
        moveRight = false;
        isAttacking = false;
        StartCoroutine(IdleHands());
    }

    private void ChooseAttackPattern()
    {
        switch(attackCounter)
        {
            case 0:
                StartCoroutine(FireBullets());
                break;
            case 1:
                StartCoroutine(FireWaves());
                break;
            case 2:
                StartCoroutine(TargetPunch());
                break;
            case 3:
                StartCoroutine(FireLasers());
                break;
            case 4:
                StartCoroutine(ClapHands());
                break;
            case 5:
                StartCoroutine(MoveLasers());
                break;
            default:
                break;
        }

        attackCounter++; //attack patterns are cycled through
        if(hardMode)
        {
            //last two attack patterns only used when boss is half health
            if(attackCounter > 5)
            {
                attackCounter = 0;
            }
        }
        else
        {
            if(attackCounter > 3)
            {
                attackCounter = 0;
            }
        }
    }
}
