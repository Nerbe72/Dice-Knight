using JetBrains.Annotations;
using System;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerDice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform;

    [SerializeField] private Image frame;
    [SerializeField] private Image select;
    [SerializeField] private TMP_Text current;

    private (int x, int y) position;
    /// <summary>
    /// 본인이 아닌 isFrame이 존재하면 이동 불가능하도록 조절하는 변수
    /// </summary>
    public bool isHaveDice;

    /// <summary>
    /// 본인이 표시하고 있는것이 사이드 숫자인지 확인
    /// </summary>
    private bool isNumber;
    private bool isCurrentNumber;

    //UI
    private bool isDragging;
    private ControllerDice holdingTarget;
    private Dice holdingDice;
    private MoveDirection moveDirection;
    private int holdingMovement;
    private int currentMovement;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        isDragging = false;
    }

    private void Update()
    {
        if (isHaveDice && frame.color == Color.clear)
        {
            frame.color = Color.white;
        }
    }

    private void Init()
    {

    }

    public void InitControllerTile(int x = 0, int y = 0)
    {
        position = (x, y);
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = GetPosFromXY(position);
        frame.color = Color.clear;
        select.color = Color.clear;
        current.color = Color.clear;
        isNumber = false;
        isCurrentNumber = false;
        isHaveDice = false;
    }

    //시작위치(64, 64) 96: 타일 한칸의 크기, 16: 경계선 굵기
    public Vector3 GetPosFromXY((int x, int y)_pos)
    {
        return new Vector3(64 + (_pos.y * (96+16)), 64 + (_pos.x * (96+16)), 0);
    }

    public (int x, int y) GetXY()
    {
        return position;
    }

    public void SetHaveDice()
    {
        isHaveDice = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null) return;

        //해당 그리드에 주사위가 있는지 확인
        if (eventData.pointerCurrentRaycast.gameObject.tag == "DiceController")
        {
            holdingTarget = eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>();

            //프레임=주사위 가 아닌 경우 스킵
            if (!holdingTarget.isHaveDice) return;

            //선택 초기화
            PlayerDiceManager.Instance.UnSelectDice();
            holdingDice = null;

            //타일 초기화 후 그리기
            PlayerMove.Instance.ClickReset();

            PlayerDiceManager.Instance.SelectDice(StageManager.Instance.GetDiceFromXY(true, holdingTarget.GetXY()));

            //그럴일은 없지만 혹시나 선택된 주사위가 버그로 없어지는 경우 꼬임을 방지
            if (PlayerDiceManager.Instance.SelectedDice() == null) return;

            holdingMovement = PlayerDiceManager.Instance.SelectedDice().GetMovement();
            holdingDice = PlayerDiceManager.Instance.SelectedDice();
            currentMovement = 0;
            isDragging = true;
            ShowNumberAround(holdingTarget);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null || eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>() == null) return;

            ControllerDice nextTarget = eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>();

            //본인인 경우 예외처리
            if (nextTarget == holdingTarget) return;

            //이미 선택된 경로를 다시 재선택하는 경우
            if (PlayerMove.Instance.CheckContainMove(nextTarget.GetXY())) return;

            //이동하려는 위치에 다른 주사위가 존재하는 경우 
            if (nextTarget.isHaveDice) return;

            (int x, int y) nextXY = nextTarget.GetXY();

            //기본적으로 대각선을 입력받으면 무시
            //최근 입력받은 좌표가 없다면 내가 지금 입력한 좌표와 holding된 주사위의 좌표를 비교하여 스킵
            //가장 최근 입력된 좌표를 확인
            if (PlayerMove.Instance.MoveListSize() == 0)
            {
                if (nextXY.x != holdingTarget.GetXY().x && nextXY.y != holdingTarget.GetXY().y) return;
            }
            else if (nextXY.x != PlayerMove.Instance.GetLatestMove().x && nextXY.y != PlayerMove.Instance.GetLatestMove().y) return;

            if (holdingMovement <= currentMovement) return;

            //배열이 없으면 타겟부터 이동위치까지.
            //배열이 있으면 마지막 위치부터 이동위치까지.
            if (PlayerMove.Instance.MoveListSize() == 0)
            {
                //이전 숫자 초기화
                HideNumberAround(holdingTarget);
                moveDirection = PlayerMove.Instance.AddMove(holdingTarget.GetXY(), nextXY);
                //tempNumber를 정해진 방향으로 이동
                holdingDice.RollingTempTo(moveDirection);
            }
            else
            {
                HideNumberAround(StageManager.Instance.GetControllerFromXY(PlayerMove.Instance.GetLatestMove()));
                moveDirection = PlayerMove.Instance.AddMove(PlayerMove.Instance.GetLatestMove(), nextXY);
                holdingDice.RollingTempTo(moveDirection);
            }

            Color newColor = new Color(Math.Clamp(0 + (currentMovement * 60), 0, 255), 0, Math.Clamp(255 - (currentMovement * 60), 0, 255), 255);
            eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>().select.color = newColor;
            PlayerMove.Instance.AddMovingNumber(holdingDice.tempNumber);
            ShowNumberAround(nextTarget);

            currentMovement++;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    public void ShowNumberAround(ControllerDice _target)
    {
        (int x, int y) xy = _target.GetXY();

        _target.isNumber = true;
        _target.current.text = holdingDice.tempNumber.current.ToString();
        _target.current.color = Color.white;

        if (xy.x - 1 >= 0)
        {
            StageManager.Instance.GetControllerFromXY((xy.x - 1, xy.y)).isNumber = true;
            StageManager.Instance.GetControllerFromXY((xy.x - 1, xy.y)).current.text = (7 - holdingDice.tempNumber.right).ToString();
            StageManager.Instance.GetControllerFromXY((xy.x - 1, xy.y)).current.color = Color.white;
        }

        if (xy.x + 1 < StageManager.Instance.GridXSize)
        {
            StageManager.Instance.GetControllerFromXY((xy.x + 1, xy.y)).isNumber = true;
            StageManager.Instance.GetControllerFromXY((xy.x + 1, xy.y)).current.text = holdingDice.tempNumber.right.ToString();
            StageManager.Instance.GetControllerFromXY((xy.x + 1, xy.y)).current.color = Color.white;
        }

        if (xy.y - 1 >= 0)
        {
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y - 1)).isNumber = true;
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y - 1)).current.text = (7 - holdingDice.tempNumber.bottom).ToString();
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y - 1)).current.color = Color.white;
        }

        if (xy.y + 1 < StageManager.Instance.GridYSize)
        {
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y + 1)).isNumber = true;
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y + 1)).current.text = holdingDice.tempNumber.bottom.ToString();
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y + 1)).current.color = Color.white;
        }
    }

    public void HideNumberAround(ControllerDice _target)
    {
        (int x, int y) xy = _target.GetXY();

        _target.isNumber = false;
        _target.current.color = Color.clear;

        if (xy.x - 1 >= 0)
        {
            StageManager.Instance.GetControllerFromXY((xy.x - 1, xy.y)).isNumber = false;
            StageManager.Instance.GetControllerFromXY((xy.x - 1, xy.y)).current.color = Color.clear;
        }

        if (xy.x + 1 < StageManager.Instance.GridXSize)
        {
            StageManager.Instance.GetControllerFromXY((xy.x + 1, xy.y)).isNumber = false;
            StageManager.Instance.GetControllerFromXY((xy.x + 1, xy.y)).current.color = Color.clear;
        }

        if (xy.y - 1 >= 0)
        {
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y - 1)).isNumber = false;
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y - 1)).current.color = Color.clear;
        }

        if (xy.y + 1 < StageManager.Instance.GridYSize)
        {
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y + 1)).isNumber = false;
            StageManager.Instance.GetControllerFromXY((xy.x, xy.y + 1)).current.color = Color.clear;
        }
    }
}
