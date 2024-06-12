using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : InputAndAction
{
    public static PlayerAttack Instance;

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
        InputManager.TurnActionList.Add(Turn.PlayerAttack, this);

        turnName = "PlayerAttack";
    }
}
