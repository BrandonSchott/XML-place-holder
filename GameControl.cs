using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TMPro;

public class GameControl : MonoBehaviour
{
    [SerializeField]
    TMP_InputField name;

    [SerializeField]
    GameObject HighScoreDisplay, ScoreDisplay;

    [SerializeField]
    Button HitButton, StartButton;

    [SerializeField]
    Score[] highScores = new Score[5];

    string currentName;
    int currentScore;

    float startTimeStamp, gameStartedTimeStamp;

    SaveContainer myContainer = new SaveContainer();

    string filePath = "HighScoreData";
    string savedScore = "savedScore.xml";

    enum State
    {
        ReadyToStart,
        CountDown,
        Playing,
        Ending
    }
    State gameState;

    void Start()
    {
        gameState = State.ReadyToStart;
        LoadData();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        UpdateHighScore();

        if (gameState == State.ReadyToStart)
        {
            name.interactable = true;
            StartButton.interactable = true;
        }
        else
        {
            StartButton.interactable = false;
            name.interactable = false;
        }

        switch (gameState)
        {
            case State.ReadyToStart:
                HitButton.GetComponentInChildren<TextMeshProUGUI>().text = "";
                if (name.text == "")
                {
                    StartButton.interactable = false;
                }
                else
                {
                    StartButton.interactable = true;
                }
                break;
            case State.CountDown:
                ScoreDisplay.GetComponent<TextMeshProUGUI>().text = $"";
                currentName = name.text;
                if ((Time.time > startTimeStamp) && (Time.time < startTimeStamp + 1))
                {
                    HitButton.GetComponentInChildren<TextMeshProUGUI>().text = "3";
                }
                else if ((Time.time > startTimeStamp + 1) && (Time.time < startTimeStamp + 2))
                {
                    HitButton.GetComponentInChildren<TextMeshProUGUI>().text = "2";
                }
                else if ((Time.time > startTimeStamp + 2) && (Time.time < startTimeStamp + 3))
                {
                    HitButton.GetComponentInChildren<TextMeshProUGUI>().text = "1";
                }
                else if ((Time.time > startTimeStamp + 3) && (Time.time < startTimeStamp + 3.5))
                {
                    HitButton.GetComponentInChildren<TextMeshProUGUI>().text = "GO!";
                }

                if (Time.time > startTimeStamp + 3.5)
                {
                    HitButton.GetComponentInChildren<TextMeshProUGUI>().text = "HIT";
                    gameState = State.Playing;
                    gameStartedTimeStamp = Time.time;
                }
                break;
            case State.Playing:
                ScoreDisplay.GetComponent<TextMeshProUGUI>().text = $"{currentScore}";
                if (Time.time > gameStartedTimeStamp + 10)
                {
                    gameState = State.Ending;
                }
                break;
            case State.Ending:
                Score AquiredScore = new Score();
                AquiredScore.name = currentName;
                AquiredScore.score = currentScore;
                CheckHighScore(AquiredScore);
                SaveData();
                gameState = State.ReadyToStart;
                break;
        }
    }

    public void UpdateHighScore()
    {
        string highScoreDisplayText = $"High Score MF";
        foreach (Score scores in highScores)
        {
            if(scores != null)
            {
                highScoreDisplayText += $"\n{scores.name}: {scores.score}";
            }
        }
        HighScoreDisplay.GetComponent<TextMeshProUGUI>().text = highScoreDisplayText;
    }

    public void CheckHighScore(Score currentScore)
    {
        Score holdingScore = new Score();
        Score changingScore = new Score();

        bool recordBroken = false;

        for (int i = 0; i < highScores.Length; i++)
        {
            if (recordBroken)
            {
                if (highScores[i] != null)
                {
                    changingScore = holdingScore;
                    holdingScore = highScores[i];

                    highScores[i] = changingScore;
                }
                else
                {
                    highScores[i] = holdingScore;
                    i = 10;
                }
            }
            else
            {
                if (highScores[i] == null || currentScore.score > highScores[i].score)
                {
                    recordBroken = true;
                    if (highScores[i] != null)
                    {
                        holdingScore = highScores[i];
                    }
                    highScores[i] = currentScore;
                }
            }
        }
    }

    public void StartGame()
    {
        if (gameState == State.ReadyToStart && name.text != "")
        {
            startTimeStamp = Time.time;
            currentScore = 0;
            gameState = State.CountDown;
        }
    }

    public void Punch()
    {
        if (gameState == State.Playing)
        {
            currentScore++;
        }
    }

    public void LoadData()
    {
        if (Directory.Exists(filePath))
        {
            Stream stream = File.Open(filePath + '\\' + savedScore, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveContainer));
            myContainer = (SaveContainer)serializer.Deserialize(stream);
            stream.Close();

            highScores = myContainer.highScores;
        }
    }

    public void SaveData()
    {
        myContainer.highScores = highScores;

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        Stream stream = File.Open(filePath + '\\' + savedScore, FileMode.Create);
        XmlSerializer serializer = new XmlSerializer(typeof(SaveContainer));
        serializer.Serialize(stream, myContainer);
        stream.Close();
    }
}

public class Score
{
    public string name;
    public int score;
}


public class SaveContainer
{
    public Score[] highScores = new Score[5];
}

