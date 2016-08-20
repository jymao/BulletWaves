using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject boss;
    public GameObject player;
    public GameObject background;

    public GameObject canvas;
    public GameObject playerBarFront;
    public GameObject playerBarBack;
    public GameObject bossBarFront;
    public GameObject bossBarBack;

    private Boss bossScript;
    private Player playerScript;
    private BGScroll scrollScript;

    private Vector3 initBossLoc;
    private Vector3 initPlayerLoc;

    private bool sceneStarted = false;
    private bool bossAppeared = false;
    private bool gamePlaying = false;
    private bool isNormalMode = true;
    private bool pause = false;
    private bool bossDead = false;
    private bool playerDead = false;

    private int score = 0;
    private float lastShot = 0;

	// Use this for initialization
	void Start () {
        bossScript = boss.GetComponent<Boss>();
        playerScript = player.GetComponent<Player>();
        scrollScript = background.GetComponent<BGScroll>();


        initPlayerLoc = player.transform.position;
        initBossLoc = boss.transform.position;
        boss.transform.position += new Vector3(0, 80, 0);

        playerScript.SetMove(false);

        playerBarFront.GetComponent<MeshRenderer>().enabled = false;
        bossBarFront.GetComponent<MeshRenderer>().enabled = false;
        playerBarBack.GetComponent<MeshRenderer>().enabled = false;
        bossBarBack.GetComponent<MeshRenderer>().enabled = false;
        canvas.transform.GetChild(0).gameObject.SetActive(false);
        canvas.transform.GetChild(1).gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	    
        //boss descending animation
        if(boss.transform.position != initBossLoc && sceneStarted)
        {
            boss.transform.position += new Vector3(0, -0.2f, 0);

            if(boss.transform.position.y < initBossLoc.y)
            {
                boss.transform.position = initBossLoc;
            }
        }

        if (boss.transform.position == initBossLoc && !bossAppeared)
        {
            bossAppeared = true;
            //reset to idle animation from game reset, set it to false here to prevent idle anim after dead anim
            bossScript.ResetIdle(false); 

            if (isNormalMode)
            {
                bossScript.decreaseHealth(bossScript.health);
            }
            playerScript.decreaseHealth(playerScript.health);
        }

        //health bar animation
        if(bossAppeared && !gamePlaying && !bossDead && !playerDead)
        {
            playerBarFront.GetComponent<MeshRenderer>().enabled = true;
            playerBarBack.GetComponent<MeshRenderer>().enabled = true;
            canvas.transform.GetChild(1).gameObject.SetActive(true);

            if (isNormalMode)
            {
                canvas.transform.GetChild(0).gameObject.SetActive(true);
                bossBarFront.GetComponent<MeshRenderer>().enabled = true;
                bossBarBack.GetComponent<MeshRenderer>().enabled = true;
                bossScript.increaseHealth(bossScript.maxHealth / 100);
            }
            else
            {
                canvas.transform.GetChild(10).gameObject.SetActive(true);
            }

            playerScript.increaseHealth(playerScript.maxHealth / 100);

            if (bossScript.health == bossScript.maxHealth && playerScript.health == playerScript.maxHealth)
            {
                gamePlaying = true;
                playerScript.SetMove(true);
                bossScript.SetCanShoot(true);
            }
        }

        if(Input.GetKeyDown(KeyCode.P) && gamePlaying)
        {
            pause = !pause;

            if(pause)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        //score update
        if (!isNormalMode && canvas.transform.GetChild(10).gameObject.activeSelf)
        {
            canvas.transform.GetChild(10).gameObject.GetComponent<Text>().text = "Score: " + score;
        }

        if(playerScript.health == 0 && !bossDead && gamePlaying)
        {
            playerDead = true;
            //game over: loss
            GameOver(false);
        }
        else if(bossScript.health == 0 && !playerDead && gamePlaying)
        {
            bossDead = true;
            //game over: win
            GameOver(true);
        }

        //boss attack AI
        if (!pause && gamePlaying)
        {
            if(Time.time - lastShot > 5)
            {
                lastShot = Time.time;

                //boss gets harder once half its health is gone
                if(bossScript.health > bossScript.maxHealth / 2 && isNormalMode)
                {
                    int pattern = Random.Range(0, 6);
                    int lines;

                    switch(pattern)
                    {
                        case 0:
                            //straight pattern
                            lines = Random.Range(1, 21);
                            StartCoroutine(bossScript.FireStraight(lines));
                            break;
                        case 1:
                            //curved left
                            lines = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines, true));
                            break;
                        case 2:
                            //curved right
                            lines = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines, false));
                            break;
                        case 3:
                            //pathY
                            StartCoroutine(bossScript.FirePathY());
                            break;
                        case 4:
                            //pathX
                            StartCoroutine(bossScript.FirePathX());
                            break;
                        case 5:
                            //circle
                            lines = Random.Range(1, 5);
                            StartCoroutine(bossScript.FireCircle(lines));
                            break;
                    }
                }
                //boss is low health or is endless mode
                else if(bossScript.health <= bossScript.maxHealth / 2 || !isNormalMode)
                {
                    int pattern = Random.Range(0, 16);
                    int lines1;
                    int lines2;

                    switch (pattern)
                    {
                        case 0:
                            //straight pattern
                            lines1 = Random.Range(1, 21);
                            StartCoroutine(bossScript.FireStraight(lines1));
                            break;
                        case 1:
                            //curved left
                            lines1 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, true));
                            break;
                        case 2:
                            //curved right
                            lines1 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, false));
                            break;
                        case 3:
                            //pathY
                            StartCoroutine(bossScript.FirePathY());
                            break;
                        case 4:
                            //pathX
                            StartCoroutine(bossScript.FirePathX());
                            break;
                        case 5:
                            //circle
                            lines1 = Random.Range(1, 5);
                            StartCoroutine(bossScript.FireCircle(lines1));
                            break;
                        case 6:
                            //straight pattern + curve left
                            lines1 = Random.Range(1, 21);
                            lines2 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireStraight(lines1));
                            StartCoroutine(bossScript.FireCurved(lines2, true));
                            break;
                        case 7:
                            //straight + curve right
                            lines1 = Random.Range(1, 21);
                            lines2 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireStraight(lines1));
                            StartCoroutine(bossScript.FireCurved(lines2, false));
                            break;
                        case 8:
                            //straight + pathY
                            lines1 = Random.Range(1, 21);
                            StartCoroutine(bossScript.FireStraight(lines1));
                            StartCoroutine(bossScript.FirePathY());
                            break;
                        case 9:
                            //straight + pathX
                            lines1 = Random.Range(1, 21);
                            StartCoroutine(bossScript.FireStraight(lines1));
                            StartCoroutine(bossScript.FirePathX());
                            break;
                        case 10:
                            //curve left + right
                            lines1 = Random.Range(1, 18);
                            lines2 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, true));
                            StartCoroutine(bossScript.FireCurved(lines2, false));
                            break;
                        case 11:
                            //curve left + pathX
                            lines1 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, true));
                            StartCoroutine(bossScript.FirePathX());
                            break;
                        case 12:
                            //curve right + pathX
                            lines1 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, false));
                            StartCoroutine(bossScript.FirePathX());
                            break;
                        case 13:
                            //curve left + pathY
                            lines1 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, true));
                            StartCoroutine(bossScript.FirePathY());
                            break;
                        case 14:
                            //curve right + pathY
                            lines1 = Random.Range(1, 18);
                            StartCoroutine(bossScript.FireCurved(lines1, false));
                            StartCoroutine(bossScript.FirePathY());
                            break;
                        case 15:
                            //pathX + pathY
                            StartCoroutine(bossScript.FirePathX());
                            StartCoroutine(bossScript.FirePathY());
                            break;
                    }
                }
            }
        }
	}

    public void SetIsNormalMode(bool b)
    {
        isNormalMode = b;
        canvas.transform.GetChild(2).gameObject.SetActive(false);
        canvas.transform.GetChild(3).gameObject.SetActive(false);
        canvas.transform.GetChild(4).gameObject.SetActive(false);
        sceneStarted = true;
    }

    public void UpdateScore(int amount)
    {
        score += amount;
    }

    public void PauseGame()
    {
        pause = true;
        //  pause background
        scrollScript.Pause(true);

        //  boss animation
        bossScript.EnableAnimation(false);

        //  pause player animation
        playerScript.EnableAnimation(false);

        //  stop player movement
        playerScript.SetMove(false);

        //stop bullet movement and store their velocities
        bossScript.SetCanShoot(false);

        foreach(GameObject bossBullet in GameObject.FindGameObjectsWithTag("BossBullet"))
        {
            bossBullet.GetComponent<BossBullet>().Pause();
        }

        foreach (GameObject playerBullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            playerBullet.GetComponent<PlayerBullet>().Pause();
        }

        //show pause screen
        canvas.transform.GetChild(5).gameObject.SetActive(true);
        canvas.transform.GetChild(6).gameObject.SetActive(true);
        canvas.transform.GetChild(7).gameObject.SetActive(true);

    }

    public void ResumeGame()
    {
        pause = false;
        //  resume background
        scrollScript.Pause(false);

        //  boss animation
        bossScript.EnableAnimation(true);

        //  resume player animation
        playerScript.EnableAnimation(true);

        //  allow player movement
        playerScript.SetMove(true);

        //allow bullet movement and restore their velocities
        bossScript.SetCanShoot(true);

        foreach (GameObject bossBullet in GameObject.FindGameObjectsWithTag("BossBullet"))
        {
            bossBullet.GetComponent<BossBullet>().Resume();
        }

        foreach (GameObject playerBullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            playerBullet.GetComponent<PlayerBullet>().Resume();
        }

        //hide pause screen
        canvas.transform.GetChild(5).gameObject.SetActive(false);
        canvas.transform.GetChild(6).gameObject.SetActive(false);
        canvas.transform.GetChild(7).gameObject.SetActive(false);

    }

    public void GameOver(bool win)
    {
        gamePlaying = false;
        bossScript.SetCanShoot(false);
        playerScript.SetMove(false);
        
        if(win)
        {
            canvas.transform.GetChild(11).gameObject.SetActive(true);
        }
        else
        {
            canvas.transform.GetChild(12).gameObject.SetActive(true);
        }

        canvas.transform.GetChild(5).gameObject.SetActive(true);
        canvas.transform.GetChild(7).gameObject.SetActive(true);
    }

    public void Reset()
    {
        //turn everything back to game start up state
        //disable health bar, pause menu, boss
        //player bar
        playerBarFront.GetComponent<MeshRenderer>().enabled = false;
        playerBarBack.GetComponent<MeshRenderer>().enabled = false;
        canvas.transform.GetChild(1).gameObject.SetActive(false);

        //enemy bar
        canvas.transform.GetChild(0).gameObject.SetActive(false);
        bossBarFront.GetComponent<MeshRenderer>().enabled = false;
        bossBarBack.GetComponent<MeshRenderer>().enabled = false;

        //score
        canvas.transform.GetChild(10).gameObject.SetActive(false);

        //pause menu
        canvas.transform.GetChild(5).gameObject.SetActive(false);
        canvas.transform.GetChild(6).gameObject.SetActive(false);
        canvas.transform.GetChild(7).gameObject.SetActive(false);

        //game over menu
        canvas.transform.GetChild(5).gameObject.SetActive(false);
        canvas.transform.GetChild(7).gameObject.SetActive(false);
        canvas.transform.GetChild(11).gameObject.SetActive(false);
        canvas.transform.GetChild(12).gameObject.SetActive(false);
        
        //boss position
        boss.transform.position += new Vector3(0, 80, 0);

        //prevent player movement and firing
        playerScript.SetMove(false);

        //reset player position
        player.transform.position = initPlayerLoc;

        //remove any bullets on screen
        foreach (GameObject bossBullet in GameObject.FindGameObjectsWithTag("BossBullet"))
        {
            Destroy(bossBullet);
        }

        foreach (GameObject playerBullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            Destroy(playerBullet);
        }

        //show menu
        canvas.transform.GetChild(2).gameObject.SetActive(true);
        canvas.transform.GetChild(3).gameObject.SetActive(true);
        canvas.transform.GetChild(4).gameObject.SetActive(true);

        //reset any logic bools
        sceneStarted = false;
        bossAppeared = false;
        gamePlaying = false;
        pause = false;
        bossDead = false;
        playerDead = false;

        //  resume background
        scrollScript.Pause(false);

        //  resume boss animation
        bossScript.EnableAnimation(true);
        bossScript.ResetIdle(true);

        //  resume player animation
        playerScript.EnableAnimation(true);

        //reset health
        bossScript.increaseHealth(bossScript.maxHealth);
        playerScript.increaseHealth(playerScript.maxHealth);

        //reset score for endless
        score = 0;

    }

    public void EnableNormalExplanation()
    {
        canvas.transform.GetChild(8).gameObject.SetActive(true);
    }

    public void DisableNormalExplanation()
    {
        canvas.transform.GetChild(8).gameObject.SetActive(false);
    }

    public void EnableEndlessExplanation()
    {
        canvas.transform.GetChild(9).gameObject.SetActive(true);
    }

    public void DisableEndlessExplanation()
    {
        canvas.transform.GetChild(9).gameObject.SetActive(false);
    }
}
