using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private GameManager gameManager;

    public delegate void InitStageSetting();
    public InitStageSetting ResetAll;

    public int GridXSize = 5;
    public int GridYSize = 4;

    private StageData currentStageData;
    private bool enterStage;
    private bool changeTurn;
    private Turn currentTurn;
    private int currentCost;

    private bool isPlayerSolo;
    private bool isEnemySolo;

    private Dictionary<(int x, int y), Dice> playerDices;
    private Dictionary<(int x, int y), Dice> enemyDices;

    private TileData[,] playerGrid;
    private TileData[,] enemyGrid;

    [Header("Sprite")]
    [Tooltip("주사위 타입별 공격 범위")][SerializeField] private List<Sprite> attackAreaSprites;

    [Header("스탯 표시")]
    [SerializeField] private GameObject statIndicator;
    [SerializeField] private List<TMP_Text> statTexts;
    [SerializeField] private Image attackAreaImage;

    [Header("게임UI")]
    [SerializeField] private List<GameObject> tilePrefabs;
    public GameObject TurnNamePanel;
    public TMP_Text TurnNameText;

    [Header("그리드")]
    [Tooltip("그리드 시스템 묶기(플레이어)")][SerializeField] private GameObject playerGirdParent;
    [Tooltip("그리드 시스템 묶기(적)")][SerializeField] private GameObject enemyGridParent;

    [Header("컨트롤러")]
    [SerializeField] private GameObject controllerBar;
    [SerializeField] private RectTransform controllerParent;
    [SerializeField] private GameObject controllerPrefab;
    private ControllerDice[,] controllerGrid;

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void asdf()
    //{
       
    //}

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            Destroy(this);
        }
        Init();

        ResetAll = CreateGrid;
        ResetAll += PutEnemyDice;

        gameManager = GameManager.Instance;
        currentStageData = gameManager.GetStageData();
    }

    private void Init()
    {
        playerDices = new Dictionary<(int, int), Dice>();
        enemyDices = new Dictionary<(int, int), Dice>();

        playerGrid = new TileData[GridXSize, GridYSize];
        enemyGrid = new TileData[GridXSize, GridYSize];

        controllerGrid = new ControllerDice[GridXSize, GridYSize];

        enterStage = true;
        changeTurn = false;
        currentCost = 0;
        currentTurn = Turn.PlayerSet;


        CloseStatUI();
    }

    public (int, int) ClampedPos((int x, int y) _pos)
    {
        return (Math.Clamp(_pos.x, 0, GridXSize), Math.Clamp(_pos.y, 0, GridYSize));
    }

    public void CreateGrid()
    {
        //플레이어 판 생성
        for (int yPos = 0; yPos < GridYSize; yPos++)
        {
            for (int xPos = 0; xPos < GridXSize; xPos++)
            {
                GameObject obj = Instantiate(tilePrefabs[0]);
                obj.transform.parent = playerGirdParent.transform;
                obj.transform.localPosition = PositionFromXY((xPos, yPos));
                obj.GetComponent<TileData>().x = xPos;
                obj.GetComponent<TileData>().y = yPos;
                playerGrid[xPos, yPos] = obj.GetComponent<TileData>();
            }
        }

        //적 판 생성
        for (int yPos = 0; yPos < GridYSize; yPos++)
        {
            for (int xPos = 0; xPos < GridXSize; xPos++)
            {
                GameObject obj = Instantiate(tilePrefabs[1]);
                obj.transform.parent = enemyGridParent.transform;
                obj.transform.localPosition = PositionFromXY((xPos, yPos));
                obj.GetComponent<TileData>().x = xPos;
                obj.GetComponent<TileData>().y = yPos;
                enemyGrid[xPos, yPos] = obj.GetComponent<TileData>();
            }
        }
    }

    private void PutEnemyDice()
    {
        //적 주사위 처리
        List<Vector2> keys = currentStageData.EnemyDiceSet.Keys.ToList();

        foreach (var key in keys)
        {
            try
            {
                GameObject obj = Instantiate(gameManager.GetEnemyDiceAtType(currentStageData.EnemyDiceSet[key]));
                (int x, int y) enemyPos = (gameManager.XYFromVec2(key));
                AddEnemyOnBoard(enemyPos, obj.GetComponent<Dice>());
            } catch (ArgumentException) { }
        }


    }

    //플레이어 주사위를 보드에 추가
    public bool AddDiceOnBoard(TileData _tile, Dice _dice)
    {
        //주사위가 이미 보드에 있거나, 코스트/주사위 갯수에 제한되지 않는지 확인
        if (playerDices.Values.Contains(_dice) || SideTrayController.Instance.SetCostDiceCounter(true, _dice))
        {
            //주사위가 이미 보드에 있는경우 제거
            foreach ((int x, int y) key in playerDices.Keys)
            {
                if (playerDices[key] == _dice)
                {
                    playerDices.Remove(key);
                    break;
                }
            }

            //좌표를 추가하고 주사위를 그 위치에 추가
            if (!playerDices.ContainsKey(_tile.GetXY()))
                playerDices.Add(_tile.GetXY(), _dice);
            else
                playerDices[_tile.GetXY()] = _dice;

            _dice.SetNumberUI();

            return true;
        }

        return false;
    }

    public void AddEnemyOnBoard((int x, int y) _pos, Dice _dice)
    {
        enemyDices.Add(_pos, _dice);
        _dice.SetRandomNumber();
        _dice.transform.position = PositionFromXY(_pos);
    }


    public Vector3 PositionFromXY((int x, int y) _pos)
    {
        return new Vector3(0.5f * (_pos.y - _pos.x), 0.215f * (_pos.x + _pos.y), 0);
    }

    public Vector3 PlayerGridPosFromXY((int x, int y) _pos)
    {
        return playerGrid[_pos.x, _pos.y].transform.position;
    }

    public Vector3 EnemyGridPosFromXY((int x, int y) _pos)
    {
        return enemyGrid[_pos.x, _pos.y].transform.position;
    }

    public void ShowStatUI(Dice _selectedDice)
    {
        //0: 체력 1: 기본공격 2: 기본 방어 3: 이동력
        statTexts[0].text = _selectedDice.GetHP().ToString();
        statTexts[1].text = _selectedDice.GetDamage().ToString();
        statTexts[2].text = _selectedDice.GetDefense().ToString();
        statTexts[3].text = _selectedDice.GetMovement().ToString();
        //attackAreaImage.sprite = attackAreaSprites[(int)_selectedDice.GetID()];
        statIndicator.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(_selectedDice.transform.position + new Vector3(1.5f, 0, 0));
        statIndicator.SetActive(true);
        ChooseDice(false);
    }

    public void ChooseDice(bool _select)
    {
        if (PlayerDiceManager.Instance.SelectedDice() == null) return;

        if (_select)
        {
            PlayerDiceManager.Instance.SelectedDice().isHolding = true;
            PlayerDiceManager.Instance.SelectedDice().SetFrameBlinking();
        }
        else
        {
            PlayerDiceManager.Instance.UnSelectDice();
        }
    }

    public bool HideList(GameObject _sideTray)
    {
        if (GetPlayerOnBoardCount() != currentStageData.DiceLimit)
        {
            //갯수가 부족하면 재차 질문하는 창을 띄움
            print("배치하지 않음");
            return false;
        }

        _sideTray.GetComponent<Animator>().SetTrigger("Hide");
        return true;

    }

    public void SetContollerField()
    {
        for (int i = 0; i < GridXSize; i++)
        {
            for (int j = 0; j < GridYSize; j++)
            {
                if (controllerGrid[i, j] == null)
                {
                    GameObject obj = Instantiate(controllerPrefab);
                    obj.GetComponent<RectTransform>().parent = controllerParent.GetComponent<RectTransform>();
                    controllerGrid[i, j] = obj.GetComponent<ControllerDice>();
                }
                
                controllerGrid[i, j].Init(i, j);
            }
        }

        int count = playerDices.Count;
        List<(int x, int y)> keys = playerDices.Keys.ToList();
        for (int i = 0; i < count; i++)
        {
            int x = keys[i].x;
            int y = keys[i].y;
            controllerGrid[x, y].SetHaveDice();
        }
    }

    public void OpenController()
    {
        controllerBar.SetActive(true);
    }

    public void CloseController()
    {
        controllerBar.SetActive(false);
    }

    public void RedrawControllerField()
    {

    }

    public void CloseStatUI()
    {
        statIndicator.SetActive(false);
    }

    public Turn GetTurn()
    {
        return currentTurn;
    }

    public void NextTurn()
    {
        //최초 1회 플레이어 세팅 턴 진행 후 플레이어 이동부터 적 공격까지 반복
        currentTurn = (Turn)(((int)currentTurn + (((currentTurn == (Turn.Count - 1)) ? 2 : 1))) % (int)Turn.Count);
        IsChangingTurn(true);
    }

    public bool CheckEnterStage()
    {
        return enterStage;
    }

    public void SetStageEntered()
    {
        enterStage = false;
    }

    public void IsChangingTurn(bool _changed)
    {
        changeTurn = _changed;
    }

    public bool IsChangeTurn()
    {
        return changeTurn;
    }

    public StageData GetStageData()
    {
        return currentStageData;
    }

    public int GetCurrentCost()
    {
        return currentCost;
    }

    public void SetCurrentCost(int _sum)
    {
        currentCost += _sum;
    }

    public int GetPlayerOnBoardCount()
    {
        return playerDices.Count;
    }

    public TileData GetTileFromXY((int x, int y) _xy)
    {
        return playerGrid[_xy.x, _xy.y];
    }

    public (int x, int y) GetGridSize()
    {
        return (GridXSize, GridYSize);
    }

    public Dice GetDiceFromXY(bool _isPlayers, (int x, int y) _xy)
    {
        if (_isPlayers)
        {
            if (!playerDices.ContainsKey(_xy)) return null;
            return playerDices[_xy];
        }
        else
        {
            if (!enemyDices.ContainsKey(_xy)) return null;
            return enemyDices[_xy];
        }
    }

    public ControllerDice GetControllerFromXY((int x, int y) _xy)
    {
        return controllerGrid[_xy.x, _xy.y];
    }

    public TileData GetTileDataFromXY(bool _isPlayers, (int x, int y) _xy)
    {
        if (_isPlayers)
            return playerGrid[_xy.x, _xy.y];
        else
            return enemyGrid[_xy.x, _xy.y];
    }

    public (int x, int y) GetXYFromPlayerDice(Dice _dice)
    {
        int count = playerDices.Keys.Count;
        List<(int x, int y)> keys = playerDices.Keys.ToList();

        for (int i = 0; i < count; i++)
        {
            if (playerDices[keys[i]] == _dice)
            {
                return keys[i];
            }
        }

        return (-1, -1);
    }

    public (int x, int y) GetXYFromEnemyDice(Dice _dice)
    {
        int count = enemyDices.Keys.Count;
        List<(int x, int y)> keys = enemyDices.Keys.ToList();

        for (int i = 0; i < count; i++)
        {
            if (enemyDices[keys[i]] == _dice)
            {
                return keys[i];
            }
        }

        return (-1, -1);
    }

    public MoveDirection GetDirectionFromXY((int x, int y) from, (int x, int y) to)
    {
        if (from.x < to.x)
            return MoveDirection.Left;
        else if (from.x > to.x)
            return MoveDirection.Right;

        if (from.y < to.y)
            return MoveDirection.Up;
        else if (from.y > to.y)
            return MoveDirection.Down;

        return MoveDirection.Stay;
    }

    public bool IsPlayerSolo()
    {
        return isPlayerSolo;
    }

    public bool IsEnemySolo()
    {
        return isPlayerSolo;
    }

    public Dictionary<(int x, int y), Dice> GetEnemiesOnBoard()
    {
        return enemyDices;
    }

    public Dictionary<(int x, int y), Dice> GetPlayersOnBoard()
    {
        return playerDices;
    }

    public bool HavePlayerInX(int _x)
    {
        List<(int x, int y)> keys = playerDices.Keys.ToList<(int x, int y)>();
        for (int i = 0; i < playerDices.Keys.Count; i++)
        {
            if (keys[i].x == _x)
                return true;
        }
        return false;
    }

    public bool HaveEnemyInX(int _x)
    {
        List<(int x, int y)> keys = enemyDices.Keys.ToList<(int x, int y)>();
        for (int i = 0; i < enemyDices.Keys.Count; i++)
        {
            if (keys[i].x == _x)
                return true;
        }
        return false;
    }

    public (int c, int r, int b) MoveTo(MoveDirection _dir, (int c, int r, int b) _nums)
    {
        switch (_dir)
        {
            case MoveDirection.Up:
                _nums = (_nums.b, _nums.r, 7 - _nums.c);
                break;
            case MoveDirection.Down:
                _nums = (7 - _nums.b, _nums.r, _nums.c);
                break;
            case MoveDirection.Left:
                _nums = (_nums.r, 7 - _nums.c, _nums.b);
                break;
            case MoveDirection.Right:
                _nums = (7 - _nums.r, _nums.c, _nums.b);
                break;
        }

        return _nums;
    }

    public void BreakDice(Dice _dice)
    {
        int pcount = playerDices.Keys.Count;
        List<(int x, int y)> pkeys = playerDices.Keys.ToList();

        for (int i = 0; i < pcount; i++)
        {
            if (playerDices[pkeys[i]] == _dice)
            {
                playerDices.Remove(pkeys[i]);
            }
        }

        int ecount = playerDices.Keys.Count;
        List<(int x, int y)> ekeys = playerDices.Keys.ToList();

        for (int i = 0; i < ecount; i++)
        {
            if (playerDices[ekeys[i]] == _dice)
            {
                playerDices.Remove(ekeys[i]);
            }
        }

        Destroy(_dice.gameObject);
        Destroy(_dice);
    }
}
