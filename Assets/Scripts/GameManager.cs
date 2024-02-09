using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{


    public static GameManager Instance;
    [Header("Player Reference")]
    public Player player;
    [Header("UI  Elements")]
    public GameObject playButton;
    public GameObject gameOver;
    public GameObject HighScore;
    public Text bestScoreText;
    public Text scoreLabel;
    private int score;
    public bool paused;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        Application.targetFrameRate = 60;
        Pause();

    }

    public void Play()
    {

        score = 0;
        scoreLabel.text = score + "";
        playButton.SetActive(false);
        gameOver.SetActive(false);
        HighScore.SetActive(false);
        paused = false;
        player.enabled = true;

        Pipes[] pipes = FindObjectsOfType<Pipes>();

        for (int i = 0; i < pipes.Length; i++)
        {
            Destroy(pipes[i].gameObject);
        }
    }

    public void IncreaseScore()
    {
        score++;
        scoreLabel.text = score + "";

        int hs = PlayerPrefs.GetInt("hs", 0);

        if (score > hs)
        {
            PlayerPrefs.SetInt("hs", score);
        }

        bestScoreText.text = score + "";
    }

    public void GameOver()
    {

        bestScoreText.text = PlayerPrefs.GetInt("hs", 0) + "";
        playButton.SetActive(true);
        gameOver.SetActive(true);
        HighScore.SetActive(true);


        Pause();

    }

    public void Pause()
    {
        paused = true;
        player.enabled = false;
    }


}


