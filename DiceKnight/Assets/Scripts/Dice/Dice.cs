using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class Dice : MonoBehaviour
{
    [SerializeField] private SpriteRenderer selectedIndicator;
    [SerializeField] private SpriteRenderer attackTargetedIndicator;
    [SerializeField] private TMP_Text currentNumberText;
    [SerializeField] private TMP_Text bottomNumberText;
    [SerializeField] private TMP_Text rightNumberText;
    public Animator movingAnimator;
    public Animator attackAnimator;

    protected DiceStats stats;

    //현재 체력을 저장합니다
    private int currentHp;
    //현재 주사위 눈, 앞, 오른쪽에 위치한 주사위의 눈을 저장합니다
    private (int current, int right, int bottom) diceNumber;
    public (int current, int right, int bottom) tempNumber;

    private Vector3 holdedPos;
    private bool isSelected;
    public bool isHolding;
    private bool isTargeted;

    private Coroutine blinkCo;
    private Coroutine targetedCo;

    private void Awake()
    {
        stats = GetComponent<DiceStats>();
        currentHp = stats.Hp;
        diceNumber = (0, 0, 0);
        isSelected = false;
        isHolding = false;
    }

    private void OnMouseDown()
    {
        holdedPos = MouseWorldPosition();
        isHolding = true;
        StartCoroutine(CheckHoldingCo());
    }

    private void OnMouseUp()
    {
        isHolding = false;
    }

    #region GET
    public DiceType GetDiceType()
    {
        stats = GetComponent<DiceStats>();
        return stats.DiceType;
    }

    public int GetHP()
    {
        return currentHp;
    }

    public int GetDamage()
    {
        return stats.Damage;
    }

    public int GetDefense()
    {
        return stats.Defense;
    }

    public int GetCost()
    {
        return stats.Cost;
    }

    public int GetMovement()
    {
        return stats.Movement;
    }

    public (int c, int r, int b) GetCurrentNumber()
    {
        return diceNumber;
    }

    public int GetBottomNumber()
    {
        return diceNumber.bottom;
    }

    public int GetRightNumber()
    {
        return diceNumber.right;
    }

    public (int c, int r, int b) GetNumbers()
    {
        return diceNumber;
    }

    public int GetOppositeNumber(int _number)
    {
        return 7 - _number;
    }

    public List<Vector2> GetAttackArea()
    {
        return stats.AttackArea;
    }

    #endregion

    #region SET
    public void SetRandomNumber()
    {
        List<int> alreadySet = new List<int>();

        diceNumber.current = UnityEngine.Random.Range(1, 7);
        alreadySet.Add(diceNumber.current);
        alreadySet.Add(GetOppositeNumber(diceNumber.current));

        while (true)
        {
            diceNumber.right = UnityEngine.Random.Range(1, 7);

            if (!alreadySet.Contains(diceNumber.right)) break;
        }

        alreadySet.Add(diceNumber.right);
        alreadySet.Add(GetOppositeNumber(diceNumber.right));

        while (true)
        {
            diceNumber.bottom = UnityEngine.Random.Range(1, 7);

            if (!alreadySet.Contains(diceNumber.bottom)) break;
        }

        tempNumber = diceNumber;

        SetNumberUI();
    }

    public void SetNumberUI()
    {
        currentNumberText.text = diceNumber.current.ToString();

        if (bottomNumberText != null)
            bottomNumberText.text = diceNumber.bottom.ToString();

        if (rightNumberText != null)
            rightNumberText.text = diceNumber.right.ToString();
    }

    public void SetTempNumber((int c, int r, int b) _num)
    {
        tempNumber = _num;
    }

    public void SetCurrentNumbers((int c, int r, int b) _tempNumbers)
    {
        diceNumber = _tempNumbers;
    }

    #endregion

    private Vector3 MouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0.0f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public void RollingTempTo(MoveDirection _direction)
    {
        (int c, int r, int b) temp = (0, 0, 0);
        switch (_direction)
        {
            case MoveDirection.Up:
                temp = (tempNumber.bottom, tempNumber.right, 7 - tempNumber.current);
                break;
            case MoveDirection.Down:
                temp = (7 - tempNumber.bottom, tempNumber.right, tempNumber.current);
                break;
            case MoveDirection.Left:
                temp = (tempNumber.right, 7 - tempNumber.current, tempNumber.bottom);
                break;
            case MoveDirection.Right:
                temp = (7 - tempNumber.right, tempNumber.current, tempNumber.bottom);
                break;
        }

        SetTempNumber(temp);
    }

    public void SetFrameBlinking()
    {
        isSelected = true;
        if (blinkCo != null) StopCoroutine(blinkCo);
        blinkCo = StartCoroutine(blinkingCo());
    }

    public void UnSetFrameBlinking()
    {
        isSelected = false;
        if (blinkCo != null)
        {
            StopCoroutine(blinkCo);
            blinkCo = null;
        }
        selectedIndicator.color = Color.clear;
    }

    public void SetBlinking()
    {
        isTargeted = true;
        if (targetedCo != null) StopCoroutine(targetedCo);
        targetedCo = StartCoroutine(attackTargetedCo());
    }

    public void UnSetBlinking()
    {
        isTargeted = false;
        if (targetedCo != null)
        {
            StopCoroutine(targetedCo);
            targetedCo = null;
        }
        attackTargetedIndicator.color = Color.white;
    }

    public void RunAnimation(MoveDirection _direction)
    {
        movingAnimator.SetTrigger(_direction.ToString());
    }

    //주사위 길게 누른채 이동범위를 벗어나지 않으면 스탯 ui를 표시함 
    private IEnumerator CheckHoldingCo()
    {
        float time = 0;
        while (true)
        {

            if (!isHolding) break;
            if (Vector3.Distance(holdedPos, MouseWorldPosition()) >= 0.2f) break;

            time += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();

            if (time >= 1f)
            {
                StageManager.Instance.ShowStatUI(this);
                if (PlayerDiceManager.Instance.SelectedDice() != null)
                    PlayerDiceManager.Instance.UnSelectDice();
                isSelected = false;
                break;
            }
        }
        
        isHolding = false;
        yield break;
    }

    //주사위가 선택됨을 표시(깜박임)
    private IEnumerator blinkingCo()
    {
        float time = 0f;
        bool switcher = true;
        while (isSelected)
        {
            time += Time.deltaTime * 1.5f;

            if (switcher)
            {
                selectedIndicator.color = Color.Lerp(Color.clear, Color.white, time);
            }
            else
            {
                selectedIndicator.color = Color.Lerp(Color.white, Color.clear, time);
            }

            if (time >= 1f)
            {
                time = 0f;
                switcher = !switcher;
            }

            yield return new WaitForEndOfFrame();
        }

        blinkCo = null;
        yield break;
    }

    private IEnumerator attackTargetedCo()
    {
        float time = 0f;
        bool switcher = true;
        while (isTargeted)
        {
            time += Time.deltaTime * 2f;

            if (switcher)
            {
                attackTargetedIndicator.color = Color.Lerp(Color.white, Color.red, time);
            }
            else
            {
                attackTargetedIndicator.color = Color.Lerp(Color.red, Color.white, time);
            }

            if (time >= 1f)
            {
                time = 0f;
                switcher = !switcher;
            }

            yield return new WaitForEndOfFrame();
        }

        targetedCo = null;
        yield break;
    }
}
