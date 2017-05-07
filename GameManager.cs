using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private GameObject boss;
    public GameObject player;
    
    public GameObject warningText;
    public GameObject gameOverMenu;

    private Player playerScript;
    private bool playerDead = false; //for boss checking whether to stop attacking

    private Vector3 initBossLoc = new Vector3(0, 10, 0); //Boss spawn location

    public bool isNormalMode;
    private bool gameOver = false;
    private bool gamePlaying = false; //boss has to be ready before attacking/taking damage

    private int score = 0;

    public int level;
    private int currentBoss = BOSS_OCULUS;

    //Boss ID
    private const int BOSS_OCULUS = 0;

    //Prefabs
    public GameObject oculus;

    //------------------------------------------------------------

	// Use this for initialization
	void Start () {
    
        playerScript = player.GetComponent<Player>();

        StartCoroutine(BossIntro(BOSS_OCULUS));
	}
	
	// Update is called once per frame
	void Update () {

        if(!gameOver && gamePlaying)
        {
            CheckWinLoss();
        }
        
	}

    public void SetGamePlaying(bool b)
    {
        gamePlaying = b;
    }

    public bool GetIsPlayerDead()
    {
        return playerDead;
    }

    public bool GetIsNormalMode()
    {
        return isNormalMode;
    }

    public void UpdateScore(int amount)
    {
        score += amount;
        GameObject.Find("Score").GetComponent<Text>().text = "Score: " + score;
    }

    //Check for game over
    private void CheckWinLoss()
    {
        int bossHealth = 0;

        switch (currentBoss)
        {
            case BOSS_OCULUS:
                Oculus oculusScript = boss.GetComponent<Oculus>();
                bossHealth = oculusScript.GetHealth();
                break;
            default:
                break;
        }

        if(bossHealth == 0)
        {
            StartCoroutine(GameOver(true));
            gameOver = true;
            return;
        }

        if(playerScript.GetHealth() == 0)
        {
            StartCoroutine(GameOver(false));
            playerDead = true;
            gameOver = true;
            return;
        }
    }

    //Display game over menu
    private IEnumerator GameOver(bool win)
    {
        if (win)
        {
            //wait for death animations before proceeding
            yield return new WaitForSeconds(6f);
            gameOverMenu.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Victory";
        }
        else
        {
            //wait for death animations before proceeding
            yield return new WaitForSeconds(2f);
            gameOverMenu.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Game Over";
        }

        gameOverMenu.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(level);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    //Played before each boss appearance
    private IEnumerator BossIntro(int bossId)
    {
        if (isNormalMode)
        {
            GameObject bossName = GameObject.Find("BossName");
            GameObject bossBarBack = GameObject.Find("BossHealth").transform.GetChild(0).gameObject;
            GameObject bossBarFront = GameObject.Find("BossHealth").transform.GetChild(1).gameObject;

            //Hide from view
            bossName.GetComponent<CanvasRenderer>().SetAlpha(0);
            bossBarBack.GetComponent<CanvasRenderer>().SetAlpha(0);
            bossBarFront.GetComponent<CanvasRenderer>().SetAlpha(0);
        }

        yield return new WaitForSeconds(2f); //give boss some time to spawn

        switch(bossId)
        {
            case BOSS_OCULUS:
                //spawn boss
                boss = Instantiate(oculus, initBossLoc, Quaternion.identity);
                //move boss into position
                StartCoroutine(BossDescent(7));

                if (isNormalMode)
                {
                    //update health bar name, show name and health bar, fill health bar animation
                    GameObject bossName = GameObject.Find("BossName");
                    bossName.GetComponent<Text>().text = "Oculus";
                    StartCoroutine(boss.GetComponent<Oculus>().FillHealth());
                }
                else
                {
                    StartCoroutine(boss.GetComponent<Oculus>().SetIsReady());
                }
                break;
            default:
                break;
        }

        //warning text
        StartCoroutine(WarningFlash());

        currentBoss = bossId;
    }

    //Display warning text that flashes to the rhythm of a klaxon while the boss appears.
    private IEnumerator WarningFlash()
    {
        Text t = warningText.GetComponent<Text>();

        //flash three times
        for (int i = 0; i < 3; i++)
        {
            //fade in
            for (int j = 0; j < 10; j++)
            {
                Color c = new Color(t.color.r, t.color.g, t.color.b, t.color.a + 0.1f);
                t.color = c;

                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(1f);

            //fade out
            for (int j = 0; j < 10; j++)
            {
                Color c = new Color(t.color.r, t.color.g, t.color.b, t.color.a - 0.1f);
                t.color = c;

                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    //Move the boss into position after spawning in.
    private IEnumerator BossDescent(int moveDist)
    {
        //moveDist * 10 as the boss will move 0.1f each interval
        for (int i = 0; i < moveDist * 10; i++)
        {
            boss.transform.position += new Vector3(0, -0.1f, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }

    //Determine boss power for player taking damage
    public int GetBossPower()
    {
        switch(currentBoss)
        {
            case BOSS_OCULUS:
                Oculus oculusScript = boss.GetComponent<Oculus>();
                return oculusScript.power;
            default:
                return 0;
        }
    }
}
