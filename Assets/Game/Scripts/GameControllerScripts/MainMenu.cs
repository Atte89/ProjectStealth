using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void VarAButton()
    {
        SceneManager.LoadScene(1);
    }

    public void VarBButton()
    {
        SceneManager.LoadScene(2);
    }

    public void VarCButton()
    {
        SceneManager.LoadScene(3);
    }

    public void quitButton()
    {
        Application.Quit();
    }

}
