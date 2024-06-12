using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSet : InputAndAction
{
    public static PlayerSet Instance;

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
        InputManager.TurnActionList.Add(Turn.PlayerSet, this);

        turnName = "PlayerSet";
    }

    protected override void InputStyle()
    {

    }
}
