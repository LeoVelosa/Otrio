using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour
{

    [SerializeField] GameObject[] screens;

    public void NavigationClick(GameObject activeScreen)
    {
        for(int i=0; i<screens.Length; i++)
        {
            screens[i].SetActive(false);
        }
        activeScreen.SetActive(true);
    }
    public void FourPlayerGame()
    {
        SceneManager.LoadScene("4-player Scene");
    }

    public void SinglePlayerGame()
    {
        SceneManager.LoadScene("AI Scene");
    }

    public void backHome()
    {
        SceneManager.LoadScene("Main Scene");
    }
}
