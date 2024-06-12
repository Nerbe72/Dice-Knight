using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputAndAction : MonoBehaviour
{
    protected string turnName = "";
    private bool turnEnter;

    protected virtual void Awake()
    {
        turnEnter = false;
    }

    protected virtual void Start()
    {
        enabled = false;
        turnEnter = true;
    }

    protected virtual void Update()
    {
        InputStyle();
    }

    //스트립트가 enable 되는것을 신호로 턴을 재개함
    protected virtual void OnEnable()
    {
        if (turnEnter)
        {
            ShowTurnName(StageManager.Instance.TurnNamePanel, StageManager.Instance.TurnNameText);
        }
    }


    //매 턴이 시작되면 턴 이름을 표시하고 다음으로 넘어감
    protected virtual void ShowTurnName(GameObject _namePanel, TMP_Text _turnText)
    {
        _turnText.text = turnName;
        _namePanel.GetComponent<Animator>().SetTrigger("ShowTurn"); //todo
    }

    //턴 이름 표시 후 PreAction을 호출하여 사전 동작을 수행함 (디버프-화상 등 피해 위주의 행동)
    protected virtual void PreAction()
    {

    }

    //매 턴마다 입력 방식을 변경함
    protected virtual void InputStyle()
    {

    }

    //매 턴의 입력이 끝나면 Action을 호출하여 동작을 수행함
    protected virtual void Action()
    {

    }

    //행동할 주사위를 선택함
    protected virtual void SelectDice()
    {

    }

    public void SetEnable(bool _enabled)
    {
        enabled = _enabled;
    }
}
