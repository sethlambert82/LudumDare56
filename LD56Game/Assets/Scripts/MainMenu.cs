using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void LoadScene(int i)
    {
        if(Game.maxLevelReached >= i) SceneManager.LoadScene(i);
    }


    public void LoadHighestUnlockedLevel()
    {
        if(SceneManager.sceneCountInBuildSettings > Game.maxLevelReached)
        {
            SceneManager.LoadScene(Game.maxLevelReached);
        }
    }


    public void Quit()
    {
        Application.Quit();
    }
}
