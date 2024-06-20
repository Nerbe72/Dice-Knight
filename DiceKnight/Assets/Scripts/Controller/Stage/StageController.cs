using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class StageController : MonoBehaviour
{
    public static StageController Instance;

    private StageManager stageManager;

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
        stageManager = StageManager.Instance;
        stageManager.ResetAll();
    }

    private void Update()
    {
        SwitchTurn();
    }

    private void SwitchTurn()
    {
        if (stageManager.CheckEnterStage())
        {
            InputManager.TurnActionList[Turn.PlayerSet].SetEnable(true);
            stageManager.SetStageEntered();
        }

        if (stageManager.IsChangeTurn())
        {
            switch (stageManager.GetTurn())
            {
                case Turn.PlayerMove:
                    InputManager.TurnActionList[Turn.PlayerSet].SetEnable(false);
                    InputManager.TurnActionList[Turn.EnemyAttack].SetEnable(false);
                    InputManager.TurnActionList[Turn.PlayerMove].SetEnable(true);
                    break;
                case Turn.PlayerAttack:
                    InputManager.TurnActionList[Turn.PlayerMove].SetEnable(false);
                    InputManager.TurnActionList[Turn.PlayerAttack].SetEnable(true);
                    break;
                case Turn.EnemyMove:
                    InputManager.TurnActionList[Turn.PlayerAttack].SetEnable(false);
                    InputManager.TurnActionList[Turn.EnemyMove].SetEnable(true);
                    break;
                case Turn.EnemyAttack:
                    InputManager.TurnActionList[Turn.EnemyMove].SetEnable(false);
                    InputManager.TurnActionList[Turn.EnemyAttack].SetEnable(true);
                    break;
            }
            stageManager.IsChangingTurn(false);
        }
    }
}
