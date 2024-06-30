using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject frame;
    [SerializeField] private Button resume;
    [SerializeField] private Button exit;

    [SerializeField] private GameObject reallyExitFrame;
    [SerializeField] private Button reallyExitBtn;

    private void Awake()
    {
        resume.onClick.AddListener(Resume);
        exit.onClick.AddListener(Exit);

        reallyExitBtn.onClick.AddListener(ReallyExit);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (frame.activeSelf)
                Resume();
            else
            {
                frame.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    private void Resume()
    {
        Time.timeScale = 1f;
        reallyExitFrame.SetActive(false);
        frame.SetActive(false);
    }

    private void Exit()
    {
        reallyExitFrame.SetActive(true);
    }

    private void ReallyExit()
    {
        SoundManager.Instance.PlayBackground(Background.Title);
        SceneManager.LoadScene("Title");
    }
}
