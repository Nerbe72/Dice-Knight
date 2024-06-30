using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[Serializable]
public class VideoText
{
    public string name;
    public VideoClip clip;
    [TextArea] public string text;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject tutorialFrame;
    [SerializeField] private TMP_Text turnName;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button tutorialCloseBtn;
    [SerializeField] private List<VideoText> tutorialVideos;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            Destroy(this);
        }

        tutorialCloseBtn.onClick.AddListener(CloseTutorial);
    }

    public void ShowTutorial(Turn _turn)
    {
        Time.timeScale = 0f;
        tutorialFrame.SetActive(true);

        switch(_turn)
        {
            case Turn.PlayerSet:
                turnName.text = tutorialVideos[0].name;
                videoPlayer.clip = tutorialVideos[0].clip;
                tutorialText.text = tutorialVideos[0].text;
                break;
            case Turn.PlayerMove:
                turnName.text = tutorialVideos[1].name;
                videoPlayer.clip = tutorialVideos[1].clip;
                tutorialText.text = tutorialVideos[1].text;
                break;
            case Turn.PlayerAttack:
                turnName.text = tutorialVideos[2].name;
                videoPlayer.clip = tutorialVideos[2].clip;
                tutorialText.text = tutorialVideos[2].text;
                break;
            case Turn.EnemyMove:
                turnName.text = tutorialVideos[2].name;
                videoPlayer.clip = tutorialVideos[2].clip;
                tutorialText.text = tutorialVideos[2].text;
                break;
        }
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    public void CloseTutorial()
    {
        Time.timeScale = 1f;
        tutorialFrame.SetActive(false);
        videoPlayer.Stop();
    }
}
