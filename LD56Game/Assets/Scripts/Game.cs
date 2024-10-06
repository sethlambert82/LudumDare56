using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour,ISerializationCallbackReceiver
{
    public static int turn = 0;
    public int _turn;
    public static AudioSource audioSource;
    public static int difficulty;
    public int _difficulty;
    static int maxDifficulty;
    public int _maxDifficulty;
    public static bool continueTurnOrder=true;

    static Character[] characters;
    public Character[] _characters;

    [SerializeField] HideableCG menu;
    [SerializeField] HideableCG levelCompleteScreen;

    public static int maxLevelReached=1;
    public int _maxLevelReached;
    public static bool levelComplete = false;
    public static bool overrideTurn = false;



    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        characters = FindObjectsOfType<Character>();

        System.Array.Sort(characters, new TurnComparator());

        if(continueTurnOrder)characters[0].TakeTurn();
        Application.targetFrameRate = 60;
        continueTurnOrder = true;
        levelComplete = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menu.Toggle();
        }
        if (!levelCompleteScreen.isVisible && levelComplete)
        {
            levelCompleteScreen.Show();
        }
    }

    public static void EndCurrentTurn()
    {
        turn++;
        turn = turn % characters.Length;
        foreach(Character character in characters)
        {
            if (character.turnOrder == turn && continueTurnOrder) character.TakeTurn();
        }
    }



    public static Vector2 SpiceOpponentAiming(Vector2 toTarget)
    {
        return toTarget + Random.insideUnitCircle * 3f / (float)(maxDifficulty - difficulty);
    }


    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void InterruptTurnOrder()
    {
        continueTurnOrder = false;
    }


    static public void CompleteLevel()
    {
        if(SceneManager.GetActiveScene().buildIndex >= maxLevelReached) maxLevelReached++;
        levelComplete=true;
    }

    public void NextLevel()
    {
        if (SceneManager.sceneCountInBuildSettings > SceneManager.GetActiveScene().buildIndex + 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        
    }

    public void OnBeforeSerialize()
    {
        _turn = turn;
        _difficulty = difficulty;
        _maxDifficulty = maxDifficulty;
        _characters = characters;
        _maxLevelReached = maxLevelReached;
    }

    public void OnAfterDeserialize()
    {
        turn = _turn;
        difficulty = _difficulty;
        maxDifficulty = _maxDifficulty;
#if UNITY_EDITOR
        maxLevelReached = _maxLevelReached;
#endif
    }

}



public class TurnComparator : IComparer<Character>
{
    public int Compare(Character x, Character y)
    {
        return x.turnOrder <= y.turnOrder ? -1 : 1;
    }
}