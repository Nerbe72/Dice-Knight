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
    /// <summary>
    /// 타깃을 가지지 못한 경우를 저장함 (EnemyAttack의 것과 반대)
    /// </summary>
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

        if (!InputManager.TurnActionList.ContainsKey(Turn.EnemyMove))
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

        selectedPath.num = selectedDice.GetNumbers();
        selectedPath.path.Add(stageManager.GetXYFromEnemyDice(selectedDice));

        ////경로 설정
        //selectedPath = SearchBestPath(selectedPath, MoveDirection.Stay, selectedDice.GetMovement(), true);
        selectedPath = SearchBestPathBFS(selectedPath, selectedDice.GetMovement());


        //경로를 따라 이동 방향 및 숫자 설정
        //nextmoveNumber, path는 본인부터, movingTo는 다음 이동방향부터 저장
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
                stageManager.AddEnemyDiceOnBoard(stageManager.GetTileDataFromXY(false, selectedPath.path[selectedPath.path.Count - 1]), selectedDice);
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
        selectedPath = new PathData((0, 0, 0));
        tempSelectedDices.Clear();
        nextMoveNumber.Clear();
        movingChecker.Clear();
        movingTo.Clear();
        selectedDice = null;
        movePointer = 0;

        for (int i = 0; i < stageManager.GridXSize; i++)
        {
            for (int j = 0; j < stageManager.GridYSize; j++)
            {
                visited[i, j] = false;
            }
        }

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
                //타깃이 없는 경우를 저장
                if (diceXs[e].x != playerXs[p].x)
                    tempSelectedDices.Add(dices[diceXs[e]]);
            }
        }

        
        
        if (tempSelectedDices.Count == 0)
            TargetedAll(); //모든 주사위가 타깃을 가진 경우
        else
            TargetedSome(); //그 외
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
    private void TargetedSome()
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

    private PathData SearchBestPathBFS(PathData _start, int _movement)
    {
        Queue<PathData> queue = new Queue<PathData>();
        queue.Enqueue(_start);

        //본인을 bestPath로 설정
        PathData bestPath = _start;

        while (queue.Count > 0)
        {
            PathData currentPath = queue.Dequeue();
            (int x, int y) latestPos = currentPath.LatestPath();

            //주사위가 이미 있는 위치로 이동한 경우 스킵
            if (currentPath.path.Count > 1 && stageManager.IsHaveDice(false, latestPos)) continue;

            //이미 들어있는지 체크
            if (currentPath.path.Count > 1)
            {
                bool alreadySearched = false;
                for (int i = 0; i < currentPath.path.Count - 2; i++)
                {
                    if (currentPath.path[i] == latestPos)
                    {
                        alreadySearched = true;
                        break;
                    }
                }
                if (alreadySearched) continue;
            }

            // x, y값이 범위를 벗어난 경우 스킵
            if (latestPos.x < 0 || latestPos.x >= stageManager.GridXSize || latestPos.y < 0 || latestPos.y >= stageManager.GridYSize) continue;

            // 이미 방문한 경우 스킵
            if (visited[latestPos.x, latestPos.y]) continue;

            // 방문 표시
            visited[latestPos.x, latestPos.y] = true;

            // 경로가 1인 경우 강제로 targeted를 false로 설정하여 이동을 유도함.
            if (currentPath.path.Count != 1)
                currentPath.targeted = stageManager.HavePlayerInX(latestPos.x);

            // 가장 높은 숫자를 가진 경로 선택
            if (currentPath.targeted)
            {
                if (!bestPath.targeted || (currentPath.targeted && currentPath.num.c > bestPath.num.c))
                {
                    bestPath = currentPath;
                }
            }
            else
            {
                if (!bestPath.targeted && currentPath.num.c > bestPath.num.c)
                {
                    bestPath = currentPath;
                }
            }

            // 최대 이동 횟수에 도달하지 않았으면 다음 이동을 큐에 추가
            if (currentPath.path.Count - 1 < _movement)
            {
                queue.Enqueue(addPath(currentPath, MoveDirection.Left));
                queue.Enqueue(addPath(currentPath, MoveDirection.Right));
                queue.Enqueue(addPath(currentPath, MoveDirection.Up));
                queue.Enqueue(addPath(currentPath, MoveDirection.Down));
            }

            visited[latestPos.x, latestPos.y] = false;
        }

        return bestPath;
    }

    //(재귀) targeted 상태에 숫자가 높은 경로로 덮어쓰기
    //private PathData SearchBestPath(PathData _path, MoveDirection _dir, int _movement, bool _isFirstMove)
    // {
    //    //재귀 탈출 트리거
    //    if (_movement == 0)
    //    {
    //        _path.path.RemoveAt(_path.path.Count - 1);
    //        return _path;
    //    }

    //    //이미 주사위가 있는 위치로 이동한 경우 제거 후 반환
    //    if (_path.path.Count > 1 && stageManager.IsHaveDice(false, _path.LatestPath()))
    //    {
    //        _path.path.RemoveAt(_path.path.Count - 1);
    //        return _path;
    //    }

    //    //이미 배열에 탐색한 위치가 있는 경우 제거 후 반환
    //    if (_path.path.Count > 2)
    //    {
    //        for (int i = 0;  i < _path.path.Count - 2; i++)
    //        {
    //            if(_path.path[i] == _path.path[_path.path.Count - 1])
    //            {
    //                _path.path.RemoveAt(_path.path.Count - 1);
    //                return _path;
    //            }    
    //        }
    //    }

    //    //x, y값이 범위를 벗어난 경우 제거 후 반환
    //    if (_path.LatestPath().x < 0 || _path.LatestPath().x >= stageManager.GridXSize ||
    //        _path.LatestPath().y < 0 || _path.LatestPath().y >= stageManager.GridYSize)
    //    {
    //        _path.path.RemoveAt(_path.path.Count - 1);
    //        return _path;
    //    }

    //    //이미 방문한 경우 제거 후 반환
    //    if (visited[_path.LatestPath().x, _path.LatestPath().y])
    //    {
    //        _path.path.RemoveAt(_path.path.Count - 1);
    //        return _path;
    //    }

    //    //방문 표시
    //    visited[_path.LatestPath().x, _path.LatestPath().y] = true;

    //    //path가 1인경우 강제로 targeted를 false로 설정하여 이동을 유도함.
    //    _path.targeted = stageManager.HavePlayerInX(_path.LatestPath().x);

    //    PathData bestPath = _path;
    //    PathData[] paths = new PathData[4];
    //    paths[1] = SearchBestPath(CreateNewPath(_path, MoveDirection.Up), MoveDirection.Up, _movement - 1, false);
    //    paths[2] = SearchBestPath(CreateNewPath(_path, MoveDirection.Down), MoveDirection.Down, _movement - 1, false);
    //    paths[3] = SearchBestPath(CreateNewPath(_path, MoveDirection.Left), MoveDirection.Left, _movement - 1, false);
    //    paths[4] = SearchBestPath(CreateNewPath(_path, MoveDirection.Right), MoveDirection.Right, _movement - 1, false);

    //    for (int i = 0; i < 4; i++)
    //    {
    //        if (paths[i].targeted && paths[i].num.c > bestPath.num.c)
    //            bestPath = paths[i];
    //    }

    //    if (!bestPath.targeted)
    //    {
    //        for (int i = 0; i < 4; i++)
    //        {
    //            if (paths[i].num.c > bestPath.num.c)
    //                bestPath = paths[i];
    //        }
    //    }

    //    for (int i = 0; i < stageManager.GridXSize; i++)
    //    {
    //        for (int j = 0;  j < stageManager.GridYSize; j++)
    //        {
    //            visited[i, j] = false;
    //        }
    //    }

    //    visited[_path.path[0].x, _path.path[0].y] = true;

    //    return bestPath;
    //}

    private PathData addPath(PathData _path, MoveDirection _dir)
    {
        PathData newPath = new PathData(_path.num, _path.targeted);
        newPath.path = new List<(int x, int y)>(_path.path);
        (int x, int y) newPos = moveTo(newPath.LatestPath(), _dir);

        newPath.path.Add(newPos);
        newPath.num = stageManager.MoveTo(_dir, newPath.num);
        return newPath;
    }

    private (int x, int y) moveTo((int x, int y) _xy, MoveDirection _dir)
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
