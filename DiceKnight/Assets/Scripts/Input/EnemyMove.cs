using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : InputAndAction
{
    public static EnemyMove Instance;

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
        InputManager.TurnActionList.Add(Turn.EnemyMove, this);
    }
}
