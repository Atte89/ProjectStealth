using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitMenu : MonoBehaviour {

    public Transform quitMenu;

    private void Start()
    {
        quitMenu.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            showMenu();
        }
	}

    public void noButton()
    {
        showMenu();
    }

    public void yesButton()
    {
        SceneManager.LoadScene(0);
    }

    private void showMenu()
    {
        if (!quitMenu.gameObject.activeInHierarchy)
        {
            quitMenu.gameObject.SetActive(true);
        }
        else
        {
            quitMenu.gameObject.SetActive(false);
        }
    }
}
