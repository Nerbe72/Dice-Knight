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
    /// ������ �ƴ� isFrame�� �����ϸ� �̵� �Ұ����ϵ��� �����ϴ� ����
    /// </summary>
    public bool isHaveDice;

    /// <summary>
    /// ������ ǥ���ϰ� �ִ°��� ���̵� �������� Ȯ��
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

    //������ġ(64, 64) 96: Ÿ�� ��ĭ�� ũ��, 16: ��輱 ����
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

        //�ش� �׸��忡 �ֻ����� �ִ��� Ȯ��
        if (eventData.pointerCurrentRaycast.gameObject.tag == "DiceController")
        {
            holdingTarget = eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>();

            //������=�ֻ��� �� �ƴ� ��� ��ŵ
            if (!holdingTarget.isHaveDice) return;

            //���� �ʱ�ȭ
            PlayerDiceManager.Instance.UnSelectDice();
            holdingDice = null;

            //Ÿ�� �ʱ�ȭ �� �׸���
            PlayerMove.Instance.ClickReset();

            PlayerDiceManager.Instance.SelectDice(StageManager.Instance.GetDiceFromXY(true, holdingTarget.GetXY()));

            //���õ� �ֻ����� �������� ����� ������ ����
            if (PlayerDiceManager.Instance.SelectedDice() == null) return;

            holdingMovement = PlayerDiceManager.Instance.SelectedDice().GetMovement();
            holdingDice = PlayerDiceManager.Instance.SelectedDice();
            currentMovement = 0;
            isDragging = true;

            holdingDice.tempNumber = holdingDice.GetCurrentNumber();

            ShowNumberAround(holdingTarget);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null || eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>() == null) return;

            ControllerDice nextTarget = eventData.pointerCurrentRaycast.gameObject.GetComponent<ControllerDice>();

            //������ ��� ����ó��
            if (nextTarget == holdingTarget) return;

            //�̹� ���õ� ��θ� �ٽ� �缱���ϴ� ���
            if (PlayerMove.Instance.CheckContainMove(nextTarget.GetXY())) return;

            //�̵��Ϸ��� ��ġ�� �ٸ� �ֻ����� �����ϴ� ��� 
            if (nextTarget.isHaveDice) return;

            (int x, int y) nextXY = nextTarget.GetXY();

            //�⺻������ �밢���� �Է¹����� ����
            //�ֱ� �Է¹��� ��ǥ�� ���ٸ� ���� ���� �Է��� ��ǥ�� holding�� �ֻ����� ��ǥ�� ���Ͽ� ��ŵ
            //���� �ֱ� �Էµ� ��ǥ�� Ȯ��
            if (PlayerMove.Instance.MoveListSize() == 0)
            {
                if (nextXY.x != holdingTarget.GetXY().x && nextXY.y != holdingTarget.GetXY().y) return;
            }
            else if (nextXY.x != PlayerMove.Instance.GetLatestMove().x && nextXY.y != PlayerMove.Instance.GetLatestMove().y) return;

            if (holdingMovement <= currentMovement) return;

            //�迭�� ������ Ÿ�ٺ��� �̵���ġ����.
            //�迭�� ������ ������ ��ġ���� �̵���ġ����.
            if (PlayerMove.Instance.MoveListSize() == 0)
            {
                //���� ���� �ʱ�ȭ
                HideNumberAround(holdingTarget);
                moveDirection = PlayerMove.Instance.AddMove(holdingTarget.GetXY(), nextXY);
                //tempNumber�� ������ �������� �̵�
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
