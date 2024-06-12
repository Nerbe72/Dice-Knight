using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;


public class Dice : MonoBehaviour
{
    [SerializeField] private TMP_Text currentNumber;
    [SerializeField] private GameObject dice;
    [SerializeField] private Animator attackAnimator;

    protected DiceStats stats;
    //현재 주사위 눈, 앞, 오른쪽에 위치한 주사위의 눈을 저장합니다
    private (int current, int front, int right) diceNumber;


    public void MoveTo(MoveDirection _direction)
    {

    }

    

    public DiceType GetID()
    {
        return stats.DiceType;
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
}
