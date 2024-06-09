using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DiceStats : MonoBehaviour
{
    public string Name;
    public int Damage;
    public int MoveCount;
    public List<Vector2> attackArea;
}
