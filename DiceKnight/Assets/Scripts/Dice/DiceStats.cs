using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DiceStats : MonoBehaviour
{
    public DiceType DiceType;
    public int Cost;
    public int Damage;
    public int Defense;
    public int Movement;
    public List<Vector2> AttackArea;
}
