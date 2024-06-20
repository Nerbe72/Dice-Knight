using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMove : InputAndAction
{
    public static PlayerMove Instance;

    private StageManager stageManager;

    [SerializeField] private Button resetBtn;
    [SerializeField] private Button okBtn;

    private int plusPos = 96+16;

    private Dice selectedDice;
    private List<(int x, int y)> nextMovePosition = new List<(int x, int y)>();
    private List<(int c, int r, int b)> nextMoveNumber = new List<(int c, int r, int b)>();
    private List<bool> movingChecker = new List<bool>();
    private List<MoveDirection> movingTo = new List<MoveDirection>();
    private int movePointer;

    private Coroutine checkMovingCo;

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
        InputManager.TurnActionList.Add(Turn.PlayerMove, this);

        turnName = "PlayerMove";

        okBtn.onClick.AddListener(ClickOK);
        resetBtn.onClick.AddListener(ClickReset);
    }

    protected override void Start()
    {
        stageManager = StageManager.Instance;
        base.Start();
    }

    
    protected override void OnEnable()
    {
        movePointer = 0;
        base.OnEnable();
    }

    protected override void PreAction()
    {
        stageManager.SetContollerField();
        stageManager.OpenController();
        preActionHolder = true;
        inputHolder = false;
    }

    protected override void Action()
    {
        if (nextMovePosition.Count <= movePointer)
        {
            //턴 종료
            stageManager.AddDiceOnBoard(stageManager.GetTileFromXY(nextMovePosition[nextMovePosition.Count - 1]), selectedDice);
            PlayerDiceManager.Instance.UnSelectDice();
            StageManager.Instance.NextTurn();
            return;
        }

        //선택된대로 이동
        if (!movingChecker[movePointer] && checkMovingCo == null)
        {
            movingChecker[movePointer] = true;
            checkMovingCo = StartCoroutine(MovingCo());
        }
    }

    public (int x, int y) GetLatestMove()
    {
        if (nextMovePosition.Count == 0)
            return (-1, -1);

        return nextMovePosition[nextMovePosition.Count - 1];
    }

    //InputStyle대신 동작
    private void ClickOK()
    {
        selectedDice = PlayerDiceManager.Instance.SelectedDice();
        PlayerDiceManager.Instance.UnSelectDice();
        stageManager.CloseController();
        inputHolder = true;
        actionHolder = false;
    }

    public void ClickReset()
    {
        stageManager.SetContollerField();

        if (PlayerDiceManager.Instance.SelectedDice() != null)
            PlayerDiceManager.Instance.UnSelectDice();

        nextMovePosition.Clear();
        nextMoveNumber.Clear();
        movingChecker.Clear();
        movingTo.Clear();
        movePointer = 0;
    }

    public bool CheckContainMove((int x, int y) _xy)
    {
        return nextMovePosition.Contains(_xy);
    }

    public MoveDirection AddMove((int x, int y) _from, (int x, int y) _next)
    {
        nextMovePosition.Add(_next);
        movingChecker.Add(false);

        if (_from.x == _next.x)
        {
            if (_from.y < _next.y)
            {
                movingTo.Add(MoveDirection.Up);
                return MoveDirection.Up;
            }
            else
            {
                movingTo.Add(MoveDirection.Down);
                return MoveDirection.Down;
            }
        }

        if (_from.y == _next.y)
        {
            if (_from.x < _next.x)
            {
                movingTo.Add(MoveDirection.Left);
                return MoveDirection.Left;
            }
            else
            {
                movingTo.Add(MoveDirection.Right);
                return MoveDirection.Right;
            }
        }

        //여기까지 올 일 없음
        return MoveDirection.Up;
    }

    public void AddMovingNumber((int c, int r, int b) _tempNumber)
    {
        nextMoveNumber.Add(_tempNumber);
    }

    public int MoveListSize()
    {
        return nextMovePosition.Count;
    }

    private IEnumerator MovingCo()
    {
        selectedDice.RunAnimation(movingTo[movePointer]);
        float time = 0;
        Vector3 fromPos = selectedDice.transform.position;
        Vector3 targetPos = stageManager.PositionFromGridXY(nextMovePosition[movePointer]);

        while (true)
        {
            time += Time.deltaTime * 6.44f;

            selectedDice.transform.localPosition = Vector3.Lerp(fromPos, targetPos, time);

            if (time >= 1f)
                break;

            yield return new WaitForEndOfFrame();
        }

        selectedDice.SetCurrentNumbers(nextMoveNumber[movePointer]);
        selectedDice.SetNumberUI();
        checkMovingCo = null;
        movePointer++;
        //이동이 끝나면 해당 위치의 xy로 주사위를 이동(다시 집어넣음)
        yield break;
    }
}
