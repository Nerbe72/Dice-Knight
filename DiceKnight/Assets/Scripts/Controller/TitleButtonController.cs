using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TitleButtonController : MonoBehaviour
{
    //[SerializeField] private GameObject stageListPage;

    UIDocument uiDocument;

    VisualElement frame;

    Button tutorial;
    Button openDifficulty;
    Button openSettings;
    Button exit;

    Button backFromSettings;
    Button backFromDifficulty;

    List<Button> difficultyButtons = new List<Button>();

    Slider mainVolume;
    Slider bgmVolume;
    Slider effectVolume;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        InitUI();
    }

    private void InitUI()
    {
        var root = uiDocument.rootVisualElement;

        frame = root.Q<VisualElement>("frame");

        openDifficulty = root.Q<Button>("difficulty");
        openSettings = root.Q<Button>("settings");
        exit = root.Q<Button>("exit");

        backFromSettings = root.Q<Button>("backFromSettings");
        backFromDifficulty = root.Q<Button>("backFromDifficulty");

        difficultyButtons.Add(root.Q<Button>("Easy"));
        difficultyButtons[0].clicked += () => { Stage(Difficulty.Easy); };
        difficultyButtons.Add(root.Q<Button>("Normal"));
        difficultyButtons[1].clicked += () => { Stage(Difficulty.Normal); };
        difficultyButtons.Add(root.Q<Button>("Hard"));
        //difficultyButtons[2].clicked += () => { Stage(Difficulty.Hard); };

        mainVolume = root.Q<Slider>("mainVol");
        bgmVolume = root.Q<Slider>("bgmVol");
        effectVolume = root.Q<Slider>("effectVol");

        openSettings.clicked += () => { frame.AddToClassList("go-settings"); };
        openDifficulty.clicked += () => { frame.AddToClassList("go-difficulty"); };
        exit.clicked += () => { Application.Quit(); };

        backFromSettings.clicked += () => { frame.RemoveFromClassList("go-settings"); SoundManager.Instance.SaveVolume(); };
        backFromDifficulty.clicked += () => { frame.RemoveFromClassList("go-difficulty"); };

        tutorial = root.Q<Button>("tutorial");
        tutorial.clicked += () => { Stage(Difficulty.Tutorial); };

        mainVolume.SetValueWithoutNotify(PlayerPrefs.GetInt(VolumeType.MainVol.ToString()));
        bgmVolume.SetValueWithoutNotify(PlayerPrefs.GetInt(VolumeType.BGMVol.ToString()));
        effectVolume.SetValueWithoutNotify(PlayerPrefs.GetInt(VolumeType.EffectVol.ToString()));

        mainVolume.RegisterCallback<ChangeEvent<float>>((evt) =>
        {
            SoundManager.Instance.MasterVolume = evt.newValue / 100;
            SoundManager.Instance.SetVolume();
        });

        bgmVolume.RegisterCallback<ChangeEvent<float>>((evt) =>
        {
            SoundManager.Instance.BackgroundVolume = evt.newValue / 100;
            SoundManager.Instance.SetVolume();
        });

        effectVolume.RegisterCallback<ChangeEvent<float>>((evt) =>
        {
            SoundManager.Instance.EffectVolume = evt.newValue / 100;
            SoundManager.Instance.SetVolume();
        });
    }

    public void Stage(Difficulty _diff)
    {
        GameManager.Instance.LoadStageDataFromJson(_diff);
        SoundManager.Instance.StopBackground();
        //¾À ·Îµå
        SceneManager.LoadScene("Loading");
    }


}
