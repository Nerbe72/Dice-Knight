using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private List<DiceController> playerDices;
    private List<EnemyDiceController> enemyDices;

    private DiceController[,] playerGrid;
    private EnemyDiceController[,] enemyGrid;

    private List<(int, int)> nextGridPosition = new List<(int, int)>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        playerDices = new List<DiceController>();
        enemyDices = new List<EnemyDiceController>();

        playerGrid = new DiceController[0,0];
    }


    private void InitGrid()
    {

    }










}
