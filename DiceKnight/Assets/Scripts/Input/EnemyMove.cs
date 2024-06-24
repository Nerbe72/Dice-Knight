using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EnemyMove : InputAndAction
{
    public static EnemyMove Instance;

    #region SELECT
    private Dictionary<(int x, int y), Dice> dices;
    private Dictionary<(int x, int y), Dice> players;
    private List<Dice> tempSelectedDices = new List<Dice>();
    private Dice selectedDice = null;

    private bool[,] visited;
    #endregion

    #region MOVE
    private List<MoveDirection> movingTo = new List<MoveDirection>();
    private List<(int c, int r, int b)> nextMoveNumber = new List<(int c, int r, int b)>();
    private List<bool> movingChecker = new List<bool>();
    private int movePointer;
    private PathData selectedPath;

    private Coroutine checkMovingCo;
    #endregion
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
        InputManager.TurnActionList.Add(Turn.EnemyMove, this);

        turnName = "EnemyMove";
    }

    protected override void PreAction()
    {
        StartCoroutine(waitBeforeSearch(StageManager.Instance.GetStageData().EnemyThinkingTime));
        Init();
        preActionHolder = true;
    }

    //입력 대신 적이 주사위와 경로를 지정하는 함수로 사용
    protected override void InputAction()
    {
        //대상 설정
        Searching();

        //랜덤 대상 설정
        if (selectedDice == null)
            selectedDice = dices.Values.ToList<Dice>()[Random.Range(0, dices.Count)];

        //경로 설정
        selectedPath = SetBestPath(new PathData(selectedDice.GetNumbers()), MoveDirection.Stay, selectedDice.GetMovement());



        //경로를 따라 이동 방향 및 숫자 설정
        //nextmoveNumber, path는 본인부터, movingTo는 다음 이동방향부터
        nextMoveNumber.Add(selectedDice.GetNumbers());
        int pathCount = selectedPath.path.Count;
        for (int i = 0; i < pathCount - 1; i++)
        {
            movingTo.Add(stageManager.GetDirectionFromXY(selectedPath.path[i], selectedPath.path[i + 1]));
            movingChecker.Add(false);
            nextMoveNumber.Add(stageManager.MoveTo(movingTo[i], nextMoveNumber[i]));
        }

        selectedDice.SetFrameBlinking();
        StartCoroutine(waitBeforeMove(stageManager.GetStageData().EnemyThinkingTime));
        inputHolder = true;
    }

    protected override void Action()
    {
        if (selectedPath.path.Count - 1 <= movePointer || movingChecker.Count == 0)
        {
            //턴 종료
            if (selectedDice != null)
                stageManager.AddDiceOnBoard(stageManager.GetTileFromXY(selectedPath.path[selectedPath.path.Count - 1]), selectedDice);
            PlayerDiceManager.Instance.UnSelectDice();
            StageManager.Instance.NextTurn();
            return;
        }

        if (!movingChecker[movePointer] && checkMovingCo == null)
        {
            movingChecker[movePointer] = true;
            checkMovingCo = StartCoroutine(MovingCo());
        }
    }

    private void Init()
    {
        dices = StageManager.Instance.GetEnemiesOnBoard();
        players = StageManager.Instance.GetPlayersOnBoard();

        visited = new bool[stageManager.GridXSize, stageManager.GridYSize];
        tempSelectedDices.Clear();
        selectedDice = null;
        movingTo.Clear();
        movePointer = 0;

        System.GC.Collect();
    }

    private void Searching()
    {
        int enemyCount = dices.Count;
        int playerCount = players.Count;
        List<(int x, int y)> diceXs = dices.Keys.ToList<(int x, int y)>();
        List<(int x, int y)> playerXs = players.Keys.ToList<(int x, int y)>();

        for (int e = 0; e < enemyCount; e++)
        {
            for(int p = 0; p < playerCount; p++)
            {
                if (diceXs[e].x != playerXs[p].x)
                    tempSelectedDices.Add(dices[diceXs[e]]);
            }
        }

        
        if (tempSelectedDices.Count == 0)
        {
            //모든 주사위가 타깃을 가진 경우
            TargetedAll();
        }
        else
        {
            if (tempSelectedDices.Count == enemyCount)
            {
                //모든 주사위가 타깃을 가지지 않은 경우
                TargetedNoting();
            }
            else
            {
                //반반
                TargetedSome();
            }
        }
    }

    //눈이 가장 낮은 대상을 선착순 선택
    private void TargetedAll()
    {
        List<Dice> tempDices = dices.Values.ToList<Dice>();
        selectedDice = tempDices[0];
        
        for (int i = 1; i < tempDices.Count; i++)
        {
            if (selectedDice.GetCurrentNumber().c > tempDices[i].GetCurrentNumber().c)
            {
                selectedDice = tempDices[i];
            }
        }
    }

    //플레이어 주사위와 가장 가까운 대상을 선착순 선택
    private void TargetedNoting()
    {
        selectedDice = tempSelectedDices[0];

        for (int i = 0; i < tempSelectedDices.Count; i++)
        {
            (int x, int y) tempPos = stageManager.GetXYFromEnemyDice(tempSelectedDices[i]);
            int movement = tempSelectedDices[i].GetMovement();

            for (int j = 0; j < movement; j++)
            {
                if (stageManager.HavePlayerInX(tempPos.x + j))
                {
                    selectedDice = tempSelectedDices[i];
                    return;
                }    

                if (stageManager.HavePlayerInX(tempPos.x - j))
                {
                    selectedDice = tempSelectedDices[i];
                    return;
                }
            }
        }
    }

    //tempselectedDices중 눈이 가장 낮은 주사위를 선착순 선택
    private void TargetedSome()
    {
        for (int i = 1; i < tempSelectedDices.Count; i++)
        {
            if (selectedDice.GetCurrentNumber().c > tempSelectedDices[i].GetCurrentNumber().c)
            {
                selectedDice = tempSelectedDices[i];
            }
        }
    }

    //재귀함수로 targeted 상태에 숫자가 높은 경로로 덮어쓰기
    private PathData SetBestPath(PathData _path, MoveDirection _dir, int _movement)
     {
        if (_movement == 0)
            return _path;

        PathData tempPath = _path;

        if (tempPath.path.Count == 0)
            tempPath.path.Add(stageManager.GetXYFromEnemyDice(selectedDice));

        visited[_path.LatestPath().x, _path.LatestPath().y] = true;

        (int x, int y) latestPath = tempPath.LatestPath();
        if (latestPath.x < 0 || latestPath.x >= stageManager.GridXSize ||
            latestPath.y < 0 || latestPath.y >= stageManager.GridYSize)
            return tempPath;

        tempPath.targeted = stageManager.HavePlayerInX(tempPath.LatestPath().x);

        PathData bestPath = tempPath;
        PathData[] paths = new PathData[4];

        //타깃이 잡힌 경우 더 이상 좌우로는 이동하지 않음
        if (!tempPath.targeted)
        {
            paths[2] = SetBestPath(CreateNewPath(tempPath, MoveDirection.Left), MoveDirection.Left, _movement - 1);
            paths[3] = SetBestPath(CreateNewPath(tempPath, MoveDirection.Right), MoveDirection.Right, _movement - 1);
        }

        paths[0] = SetBestPath(CreateNewPath(tempPath, MoveDirection.Up), MoveDirection.Up, _movement - 1);
        paths[1] = SetBestPath(CreateNewPath(tempPath, MoveDirection.Down), MoveDirection.Down, _movement - 1);

        for (int i = 0; i < 4; i++)
        {
            if (paths[i].targeted && paths[i].num.c > bestPath.num.c)
                bestPath = paths[i];
        }

        if (!bestPath.targeted)
        {
            foreach (var path in paths)
            {
                if (path.num.c > bestPath.num.c)
                    bestPath = path;
            }
        }

        return bestPath;
    }

    private PathData CreateNewPath(PathData _path, MoveDirection _dir)
    {
        PathData newPath = new PathData(_path.num, _path.targeted);
        newPath.path = new List<(int x, int y)>(_path.path);
        (int x, int y) currentPos = newPath.LatestPath();
        (int x, int y) newPos = moveToNewPosition(currentPos, _dir);

        newPath.path.Add(newPos);
        newPath.num = stageManager.MoveTo(_dir, newPath.num);
        return newPath;
    }

    private (int x, int y) moveToNewPosition((int x, int y) _xy, MoveDirection _dir)
    {
        switch (_dir)
        {
            case MoveDirection.Up:
                return (_xy.x, _xy.y + 1);
            case MoveDirection.Down:
                return (_xy.x, _xy.y - 1);
            case MoveDirection.Left:
                return (_xy.x + 1, _xy.y);
            case MoveDirection.Right:
                return (_xy.x - 1, _xy.y);
            default:
                return _xy;
        }
    }

    /// <summary>
    /// 적이 고민하는 시간(대기시간)
    /// </summary>
    /// <returns></returns>
    private IEnumerator waitBeforeSearch(float _wait)
    {
        yield return new WaitForSeconds(Random.Range(1, _wait / 4));
        inputHolder = false;
        yield break;
    }

    private IEnumerator waitBeforeMove(float _wait)
    {
        yield return new WaitForSeconds(Random.Range(1f, _wait / 2));
        actionHolder = false;
        selectedDice.UnSetFrameBlinking();
        yield break;
    }

    private IEnumerator MovingCo()
    {
        selectedDice.RunAnimation(movingTo[movePointer]);
        float time = 0;
        Vector3 fromPos = selectedDice.transform.position;
        Vector3 targetPos = stageManager.EnemyGridPosFromXY(selectedPath.path[movePointer + 1]);

        while (true)
        {
            time += Time.deltaTime * 6.44f;

            selectedDice.transform.localPosition = Vector3.Lerp(fromPos, targetPos, time);

            if (time >= 1f)
                break;

            yield return new WaitForEndOfFrame();
        }

        selectedDice.SetCurrentNumbers(nextMoveNumber[movePointer + 1]);
        selectedDice.SetNumberUI();
        checkMovingCo = null;
        movePointer++;
        yield break;
    }
}
