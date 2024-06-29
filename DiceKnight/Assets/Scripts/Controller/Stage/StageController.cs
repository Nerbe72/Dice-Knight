using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    public static StageController Instance;

    private StageManager stageManager;

    [SerializeField] private Image backPanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Button goTitleBtn;
    [SerializeField] private TMP_Text goTitleText;

    private bool GameEnd;
    private bool enterWaiting;
    private bool isWaiting;

    Color textTargetColor;
    Color panelTargetColor = new Color(0, 0, 0, 100);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }

        backPanel.color = Color.clear;
        winnerText.color = Color.clear;

        backPanel.gameObject.SetActive(false);
        winnerText.enabled = false;

        goTitleBtn.onClick.AddListener(ClickGoTitle);
        goTitleBtn.gameObject.SetActive(false);
        goTitleText.enabled = false;
    }

    private void Start()
    {
        stageManager = StageManager.Instance;
        stageManager.ResetAll();

        GameEnd = false;
        enterWaiting = true;
        isWaiting = false;
    }

    private void Update()
    {
        SwitchTurn();
    }

    private void SwitchTurn()
    {
        if (GameEnd) return;

        if (stageManager.CheckEnterStage())
        {
            InputManager.TurnActionList[Turn.PlayerSet].SetEnable(true);
            stageManager.SetStageEntered();
        }

        if (stageManager.IsChangeTurn())
        {
            InputManager.TurnActionList[Turn.PlayerSet].SetEnable(false);
            InputManager.TurnActionList[Turn.PlayerMove].SetEnable(false);
            InputManager.TurnActionList[Turn.PlayerAttack].SetEnable(false);
            InputManager.TurnActionList[Turn.EnemyMove].SetEnable(false);
            InputManager.TurnActionList[Turn.EnemyAttack].SetEnable(false);

            if (enterWaiting)
            {
                StartCoroutine(waitChangeTurn());
                enterWaiting = false;
            }

            if (isWaiting) return;

            //LOSE
            if (!stageManager.IsPlayerAlive() && stageManager.GetTurn() != Turn.PlayerSet)
            {
                GameEnd = true;
                StartCoroutine(GameOver(true));
                return;
            }

            //WIN
            if (!stageManager.IsEnemyAlive() && stageManager.GetTurn() != Turn.PlayerSet)
            {
                GameEnd = true;
                StartCoroutine(GameOver(false));
                return;
            }

            switch (stageManager.GetTurn())
            {
                case Turn.PlayerMove:
                    InputManager.TurnActionList[Turn.PlayerMove].SetEnable(true);
                    break;
                case Turn.PlayerAttack:
                    InputManager.TurnActionList[Turn.PlayerAttack].SetEnable(true);
                    break;
                case Turn.EnemyMove:
                    InputManager.TurnActionList[Turn.EnemyMove].SetEnable(true);
                    break;
                case Turn.EnemyAttack:
                    InputManager.TurnActionList[Turn.EnemyAttack].SetEnable(true);
                    break;
            }
            stageManager.IsChangingTurn(false);
            enterWaiting = true;
            isWaiting = false;
            return;
        }

        EscapeAction();
    }

    private void DestroyTiles(bool _isPlayers)
    {
        for (int i = 0; i < stageManager.GridXSize; i++)
        {
            for(int j = 0; j < stageManager.GridYSize; j++)
            {
                stageManager.GetTileDataFromXY(_isPlayers, (i, j)).Shatter(new Vector3( 0.1f + Random.Range(-1f, 1f), 0.1f + Random.Range(-1f, 1f), 0));
            }
        }
    }

    private IEnumerator GameOver(bool _isPlayer)
    {
        //해당 타일에 포커싱
        //카메라 진동
        CameraManager.Instance.ShakeCamera(_time:0.8f, _lerpTime:0.01f);
        yield return new WaitForSeconds(0.78f);
        //타일 파괴
        DestroyTiles(_isPlayer);
        yield return new WaitForSeconds(0.8f);
        //승리표시
        float time = 0;
        if (_isPlayer)
        {
            winnerText.text = "패배";
            textTargetColor = Color.red;
        }
        else
        {
            winnerText.text = "승리!";
            textTargetColor = Color.green;
        }

        backPanel.gameObject.SetActive(true);
        winnerText.enabled = true;

        while (true)
        {
            time += Time.deltaTime * 4;

            backPanel.color = Color.Lerp(Color.clear, panelTargetColor, time);
            winnerText.color = Color.Lerp(Color.clear, textTargetColor, time);

            if (time >= 1f) break;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.15f);

        //타이틀로 돌아가기
        //클릭으로
        goTitleBtn.gameObject.SetActive(true);
        goTitleText.enabled = true;
        yield break;
    }

    private IEnumerator waitChangeTurn()
    {
        isWaiting = true;

        yield return new WaitForSeconds(0.7f);

        isWaiting = false;

        yield break;
    }

    private void ClickGoTitle()
    {
        SceneManager.LoadScene("Title");
    }

    private void EscapeAction()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //show esc
            Debug.Log("ESC");

            //test
            SceneManager.LoadScene("Title");
        }
    }
}
