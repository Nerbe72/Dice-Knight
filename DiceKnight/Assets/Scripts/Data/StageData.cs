using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct StageData
{
    public Difficulty StageDifficulty;
    public Background BackgroundMusic;
    public SerializedDictionary<Vector2, DiceType> EnemyDiceSet;
    public float PlayerHp;
    public float EnemyHp;
    public int CostLimit;
    public int DiceLimit;
    [Tooltip("적이 고민하는 시간(초)\n스테이지 시작시 (1 ~ 지정 시간)중 랜덤 배정")][Range(1, 10)]public float EnemyThinkingTime;

}
