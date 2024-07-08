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

    [SerializeField] private GameObject actionBar;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Button okBtn;

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

        if (!InputManager.TurnActionList.ContainsKey(Turn.PlayerMove))
            InputManager.TurnActionList.Add(Turn.PlayerMove, this);

        turnName = "PlayerMove";

        okBtn.onClick.AddListener(ClickOK);
        resetBtn.onClick.AddListener(ClickReset);
    }

    protected override void Tutorial()
    {
        TutorialManager.Instance.ShowTutorial(Turn.PlayerMove);
        firstTimeTutorial = false;
    }

    protected override void PreAction()
    {
        base.PreAction();
        Init();
    }

    protected override void Action()
    {
        //선택한 좌표 갯수만큼 대기 후 턴 종료
        if (nextMovePosition.Count <= movePointer)
        {
            if (selectedDice != null && nextMovePosition.Count != 0)
                stageManager.AddPlayerDiceOnBoard(stageManager.GetTileDataFromXY(true, nextMovePosition[nextMovePosition.Count - 1]), selectedDice);
            PlayerDiceManager.Instance.UnSelectDice();
            stageManager.NextTurn();
            return;
        }

        //선택된대로 이동
        if (!movingChecker[movePointer] && checkMovingCo == null)
        {
            movingChecker[movePointer] = true;
            checkMovingCo = StartCoroutine(MovingCo());
        }
    }

    private void Init()
    {
        movePointer = 0;
        movingChecker.Clear();nextMoveNumber.Clear();
        nextMovePosition.Clear();
        movingTo.Clear();

        actionBar.SetActive(true);
        stageManager.SetContollerField();
        stageManager.OpenController();
    }

    public (int x, int y) GetLatestMove()
    {
        if (nextMovePosition.Count == 0)
            return (-1, -1);

        return nextMovePosition[nextMovePosition.Count - 1];
    }

    /// <summary>
    /// ClickOK, ClickReset함수와 ControllerDice스크립트가 InputStyle대신 동작
    /// </summary>
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
        Vector3 targetPos = stageManager.PlayerGridPosFromXY(nextMovePosition[movePointer]);

        while (true)
        {
            time += Time.deltaTime * 5.9f;

            selectedDice.transform.localPosition = Vector3.Lerp(fromPos, targetPos, time);

            if (time >= 1f)
                break;

            yield return new WaitForEndOfFrame();
        }

        selectedDice.SetCurrentNumbers(nextMoveNumber[movePointer]);
        selectedDice.SetNumberUI();
        checkMovingCo = null;
        movePointer++;
        yield break;
    }
}
