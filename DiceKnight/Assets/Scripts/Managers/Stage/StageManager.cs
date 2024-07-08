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
    [Tooltip("�ֻ��� Ÿ�Ժ� ���� ����")][SerializeField] private List<Sprite> attackAreaSprites;

    [Header("���� ǥ��")]
    [SerializeField] private GameObject statIndicator;
    [SerializeField] private List<TMP_Text> statTexts;
    [SerializeField] private Image attackAreaImage;

    [Header("����UI")]
    [SerializeField] private TMP_Text warnText;
    [SerializeField] private List<GameObject> tilePrefabs;
    public GameObject TurnNamePanel;
    public TMP_Text TurnNameText;

    [Header("�׸���")]
    [Tooltip("�׸��� �ý��� ����(�÷��̾�)")][SerializeField] private GameObject playerGirdParent;
    [Tooltip("�׸��� �ý��� ����(��)")][SerializeField] private GameObject enemyGridParent;

    [Header("��Ʈ�ѷ�")]
    [SerializeField] private GameObject controllerBar;
    [SerializeField] private RectTransform controllerParent;
    [SerializeField] private GameObject controllerPrefab;
    private ControllerDice[,] controllerGrid;

    [Header("")]

    private Coroutine warnCo;

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

        gameManager = GameManager.Instance;
        currentStageData = gameManager.GetStageData();

        Init();

        ResetAll = CreateGrid;
        ResetAll += PutEnemyDice;
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

        InputManager.TurnActionList.Clear();
        CloseStatUI();
    }

    public (int, int) ClampedPos((int x, int y) _pos)
    {
        return (Math.Clamp(_pos.x, 0, GridXSize), Math.Clamp(_pos.y, 0, GridYSize));
    }

    public void CreateGrid()
    {
        //�÷��̾� �� ����
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

        //�� �� ����
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
        //�� �ֻ��� ó��
        List<Vector2> keys = currentStageData.EnemyDiceSet.Keys.ToList();

        foreach (var key in keys)
        {
            try
            {
                GameObject obj = Instantiate(gameManager.GetEnemyDiceAtType(currentStageData.EnemyDiceSet[key]));
                (int x, int y) enemyPos = (gameManager.XYFromVec2(key));
                AddEnemyWithStart(enemyPos, obj.GetComponent<Dice>());
            } catch (ArgumentException) { }
        }
    }

    //�÷��̾� �ֻ����� ���忡 �߰�
    public bool AddPlayerDiceOnBoard(TileData _tile, Dice _dice)
    {
        //�ֻ����� �̹� ���忡 �ְų�, �ڽ�Ʈ/�ֻ��� ������ ���ѵ��� �ʴ��� Ȯ��
        if (playerDices.Values.Contains(_dice) || SideTrayController.Instance.SetCostDiceCounter(true, _dice))
        {
            //�ֻ����� �̹� ���忡 �ִ°�� ����
            foreach ((int x, int y) key in playerDices.Keys)
            {
                if (playerDices[key] == _dice)
                {
                    playerDices.Remove(key);
                    break;
                }
            }

            //��ǥ�� �߰��ϰ� �ֻ����� �� ��ġ�� �߰�
            if (!playerDices.ContainsKey(_tile.GetXY()))
                playerDices.Add(_tile.GetXY(), _dice);
            else
                playerDices[_tile.GetXY()] = _dice;

            _dice.SetNumberUI();

            return true;
        }

        ShowWarn("��ġ������ �ֻ����� �ڽ�Ʈ�� �����մϴ�!");
        return false;
    }

    //�� �ֻ����� ���忡 �߰�
    public bool AddEnemyDiceOnBoard(TileData _tile, Dice _dice)
    {
        //�ֻ����� �̹� ���忡 �ְų�, �ڽ�Ʈ/�ֻ��� ������ ���ѵ��� �ʴ��� Ȯ��
        if (enemyDices.Values.Contains(_dice))
        {
            //�ֻ����� �̹� ���忡 �ִ°�� ����
            foreach ((int x, int y) key in enemyDices.Keys)
            {
                if (enemyDices[key] == _dice)
                {
                    enemyDices.Remove(key);
                    break;
                }
            }

            //��ǥ�� �߰��ϰ� �ֻ����� �� ��ġ�� �߰�
            if (!enemyDices.ContainsKey(_tile.GetXY()))
                enemyDices.Add(_tile.GetXY(), _dice);
            else
                enemyDices[_tile.GetXY()] = _dice;

            _dice.SetNumberUI();

            return true;
        }

        return false;
    }

    public void AddEnemyWithStart((int x, int y) _pos, Dice _dice)
    {
        enemyDices.Add(_pos, _dice);
        _dice.SetRandomNumber();
        _dice.transform.position = PositionFromXY(_pos);
    }

    public Vector3 PositionFromXY((int x, int y) _pos)
    {
        return new Vector3(0.5f * (_pos.y - _pos.x), 0.215f * (_pos.x + _pos.y), (_pos.x + _pos.y));
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
        //0: ü�� 1: �⺻���� 2: �⺻ ��� 3: �̵���
        statTexts[0].text = _selectedDice.GetHP().ToString();
        statTexts[1].text = _selectedDice.GetDamage().ToString();
        statTexts[2].text = _selectedDice.GetDefense().ToString();
        statTexts[3].text = _selectedDice.GetMovement().ToString();
        attackAreaImage.sprite = _selectedDice.GetAttackAreaSprite();
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
        if (GetPlayerOnBoardCount() == 0)
        {
            ShowWarn("�ϳ� �̻��� �ֻ����� ��ġ���ּ���");
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
                
                controllerGrid[i, j].InitControllerTile(i, j);
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

    public void CloseStatUI()
    {
        statIndicator.SetActive(false);
    }

    public Turn GetTurn()
    {
        return currentTurn;
    }

    public Background GetBackgroundMusicType()
    {
        return currentStageData.BackgroundMusic;
    }

    public bool IsPlayerAlive()
    {
        return (playerDices.Count != 0);
    }

    public bool IsEnemyAlive()
    {
        return (enemyDices.Count != 0);
    }

    public void NextTurn()
    {
        //���� 1ȸ �÷��̾� ���� �� ���� �� �÷��̾� �̵����� �� ���ݱ��� �ݺ�
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

    public Difficulty GetDifficulty()
    {
        return currentStageData.StageDifficulty;
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
        if (_xy.x < 0 || _xy.x >= GridXSize || _xy.y < 0 || _xy.y >= GridYSize)
            return null;

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

    public bool IsHaveDice(bool isPlayers, (int x, int y) _pos)
    {
        if (isPlayers)
            return playerDices.Keys.ToList<(int x, int y)>().Contains(_pos);
        else
            return enemyDices.Keys.ToList<(int x, int y)>().Contains(_pos);
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

        int ecount = enemyDices.Keys.Count;
        List<(int x, int y)> ekeys = enemyDices.Keys.ToList();

        for (int i = 0; i < ecount; i++)
        {
            if (enemyDices[ekeys[i]] == _dice)
            {
                enemyDices.Remove(ekeys[i]);
            }
        }
    }

    public void ShowWarn(string _text)
    {
        if (warnCo != null) StopCoroutine(warnCo);
        warnCo = StartCoroutine(warningCo(_text));
    }

    private IEnumerator warningCo(string _text)
    {
        float time = 0;
        warnText.text = _text;
        warnText.enabled = true;
        while (true)
        {
            time += Time.deltaTime * 0.3f;

            warnText.color = Color.Lerp(Color.red, Color.clear, time);

            if (time >= 1f) break;

            yield return new WaitForEndOfFrame();
        }

        warnText.color = Color.clear;
        warnText.enabled = false;
        warnCo = null;
        yield break;
    }
}
