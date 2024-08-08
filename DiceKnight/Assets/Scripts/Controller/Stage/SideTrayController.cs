using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideTrayController : MonoBehaviour
{
    public static SideTrayController Instance;

    private GameManager gameManager;
    private StageManager stageManager;

    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text dice;
    [SerializeField] private List<Image> diceFrames;

    private List<bool> trayHaveDice = new List<bool>();
    private List<Dice> diceList = new List<Dice>();

    private bool FirstEnter = true;


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
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        stageManager = StageManager.Instance;

        StartCoroutine(initUICo());
    }

    private void InitUI()
    {
        for (int i = 0; i < diceFrames.Count; i++)
        {
            trayHaveDice.Add(false);
        }

        SetCostDiceCounter(true, null);
        ListupPlayerDice();
    }

    //주사위를 트레이로 이동
    public void RecallDice(Dice _dice)
    {
        int emptyFrameIndex = 0;
        emptyFrameIndex = diceList.FindIndex(x => x == _dice);
        trayHaveDice[emptyFrameIndex] = false;

        for (int i = 0; i < diceFrames.Count; i++)
        {
            if (!trayHaveDice[i])
            {
                Vector3 pos = diceFrames[i].GetComponent<RectTransform>().position;
                _dice.transform.position = Camera.main.ScreenToWorldPoint(pos) + new Vector3(0, -1, 10);
                trayHaveDice[i] = true;

                if (!FirstEnter)
                    SetCostDiceCounter(false, _dice);

                FirstEnter = false;
                break;
            }
        }
    }

    public bool SetCostDiceCounter(bool _plus, Dice _dice)
    {
        if (_dice == null)
        {
            cost.text = stageManager.GetCurrentCost() + "/" + stageManager.GetStageData().CostLimit;
            dice.text = stageManager.GetPlayerOnBoardCount() + "/" + stageManager.GetStageData().DiceLimit;
            return false;
        }

        if (_plus && _dice.GetCost() + stageManager.GetCurrentCost() > stageManager.GetStageData().CostLimit)
            return false;

        if (_plus && (stageManager.GetPlayerOnBoardCount() + 1) > stageManager.GetStageData().DiceLimit)
            return false;

        if (!_plus && (stageManager.GetPlayerOnBoardCount() + 1) < 0)
            return false;

        if (!_plus)
            stageManager.SetCurrentCost(-_dice.GetCost());
        else
            stageManager.SetCurrentCost(_dice.GetCost());

        cost.text = stageManager.GetCurrentCost() + "/" + stageManager.GetStageData().CostLimit;
        dice.text = (stageManager.GetPlayerOnBoardCount() + (_plus ? 1 : -1)) + "/" + stageManager.GetStageData().DiceLimit;

        return true;
    }

    private void ListupPlayerDice()
    {
        List<GameObject> ownDiceList = gameManager.GetOwnDiceList();
        int count = ownDiceList.Count;

        for (int i = 0; i < count; i++)
        {
            FirstEnter = true;
            GameObject obj = Instantiate(ownDiceList[i]);
            diceList.Add(obj.GetComponent<Dice>());
            RecallDice(obj.GetComponent<Dice>());
        }
    }

    public List<Dice> GetDiceList()
    {
        return diceList;
    }
    
    private IEnumerator initUICo()
    {
        yield return new WaitForEndOfFrame();
        InitUI();
    }
}
