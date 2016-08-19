using UnityEngine;
using System.Collections;

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

    private bool sceneStarted = false;
    private bool bossAppeared = false;
    private bool gamePlaying = false;
    private bool isNormalMode = true;
    private bool pause = false;

	// Use this for initialization
	void Start () {
        bossScript = boss.GetComponent<Boss>();
        playerScript = player.GetComponent<Player>();
        scrollScript = background.GetComponent<BGScroll>();

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

            if (isNormalMode)
            {
                bossScript.decreaseHealth(bossScript.health);
            }
            playerScript.decreaseHealth(playerScript.health);
        }

        //health bar animation
        if(bossAppeared && !gamePlaying)
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

        //boss attack AI
        if(!pause)
        {

        }
        else
        {
            //hack for stopping newly spawned boss bullets
            foreach (GameObject bossBullet in GameObject.FindGameObjectsWithTag("BossBullet"))
            {
                bossBullet.GetComponent<BossBullet>().Pause();
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

    }

    public void Reset()
    {
        //turn everything back to game start up state
        //disable health bar, pause menu, boss
        //prevent player movement and firing
        //reset player position
        //remove any bullets on screen
        //show menu
        //reset any logic bools
        //reset health
        //reset score for endless
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
