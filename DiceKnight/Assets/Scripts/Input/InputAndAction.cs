using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputAndAction : MonoBehaviour
{
    protected string turnName = "";
    private bool turnEnter;

    //홀더가 true인 경우 스킵
    protected bool preActionHolder;
    protected bool inputHolder;
    protected bool actionHolder;

    protected virtual void Awake()
    {
        turnEnter = false;
    }

    /// <summary>
    /// 기본 설정: preActionHolder = false; inputHolder = true; actionHolder = true;
    /// </summary>
    protected virtual void Start()
    {
        enabled = false;
        turnEnter = true;
        preActionHolder = false;
        inputHolder = true;
        actionHolder = true;
    }

    protected virtual void Update()
    {
        if (!preActionHolder && inputHolder)
            PreAction();
        
        if (!inputHolder && actionHolder)
            InputStyle();

        if (!actionHolder)
            Action();
    }

    /// <summary>
    /// 턴 시작 트리거. 다시 돌아온 턴에서 초기화가 필요한 경우 이 위치에서 초기화
    /// </summary>
    protected virtual void OnEnable()
    {
        if (Application.isPlaying == false) return;

        if (turnEnter)
        {
            ShowTurnName(StageManager.Instance.TurnNamePanel, StageManager.Instance.TurnNameText);
        }
    }


    //진입시 맵 이름 표시
    protected virtual void ShowTurnName(GameObject _namePanel, TMP_Text _turnText)
    {
        _turnText.text = turnName;
        _namePanel.GetComponent<Animator>().SetTrigger("ShowTurn"); //todo
    }

    //턴 시작시 곧 바로 작동. preactionholder가 on인 경우만 동작
    /// <summary>
    /// 조건 만족시 preActionHolder = true;  inputHolder = false;
    /// </summary>
    protected virtual void PreAction()
    {
        preActionHolder = true;
        inputHolder = false;
    }

    //사전 동작 후 입력을 통해 진행
    /// <summary>
    /// 조건 만족시 inputHolder = true;  actionHolder = false;
    /// </summary>
    protected virtual void InputStyle()
    {

    }

    //입력을 진행한 후 입력한대로 동작을 수행
    /// <summary>
    /// 조건 만족시 next turn
    /// </summary>
    protected virtual void Action()
    {

    }

    //주사위를 선택함
    protected virtual void SelectDice()
    {

    }

    public void SetEnable(bool _enabled)
    {
        enabled = _enabled;
    }
}
