using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : InputAndAction
{
    public static EnemyAttack Instance;

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            return;
        }

        base.Awake();
        InputManager.TurnActionList.Add(Turn.EnemyAttack, this);

        turnName = "EnemyAttack";
    }

    protected override void PreAction()
    {

    }

    protected override void InputAction()
    {
        //타깃이 있으면 그중 숫자가 가장 높은 주사위가 공격

        //타깃이 없으면 그중 숫자가 가장 높은 주사위가 공격

        //선택하면 표시하고 1초 대기 후 Action함수로 이동
        inputHolder = true;
    }

    protected override void Action()
    {

    }
}
