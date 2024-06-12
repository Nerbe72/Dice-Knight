using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : InputAndAction
{
    public static PlayerMove Instance;

    protected void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            return;
        }
        base.Awake();
        InputManager.TurnActionList.Add(Turn.PlayerMove, this);

        turnName = "PlayerMove";
    }

    protected override void InputStyle()
    {

    }

    //주사위를 선택함

    //이동 경로를 마우스로 한번에 그림 -> 대각선 불가

    //확인을 누르면 action 함수 실행
}
