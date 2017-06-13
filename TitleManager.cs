using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {

    public GameObject title;
    public GameObject menu;
    public GameObject endlessMenu;
    public GameObject description;

    private const int NORMAL_MODE = 1;
    private const int ENDLESS_OCULUS = 2;
    private const int ENDLESS_GOLEM = 3;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NormalMode()
    {
        SceneManager.LoadScene(NORMAL_MODE);
    }

    public void EndlessMode()
    {
        menu.SetActive(false);
        endlessMenu.SetActive(true);
        description.SetActive(false);
    }

    public void OculusFight()
    {
        SceneManager.LoadScene(ENDLESS_OCULUS);
    }

    public void GolemFight()
    {
        SceneManager.LoadScene(ENDLESS_GOLEM);
    }

    public void EndlessCancel()
    {
        menu.SetActive(true);
        endlessMenu.SetActive(false);
    }

    public void NormalDescription(bool active)
    {
        description.SetActive(active);

        if (active)
        {
            string text = "Defeat the boss!";
            description.GetComponent<Text>().text = text;
        }
        
    }

    public void EndlessDescription(bool active)
    {
        description.SetActive(active);

        if (active)
        {
            string text = "Survive as long as possible while dealing damage!";
            description.GetComponent<Text>().text = text;
        }
        
    }
}
