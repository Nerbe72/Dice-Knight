using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMove : InputAndAction
{
    public static EnemyMove Instance;

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
        InputManager.TurnActionList.Add(Turn.EnemyMove, this);

        turnName = "EnemyMove";
    }

    
    protected override void PreAction()
    {
        StartCoroutine(waitBeforeMove());
        preActionHolder = true;
    }

    protected override void InputStyle()
    {
        inputHolder = true;
        actionHolder = false;
    }

    protected override void Action()
    {
        //여기 알고리즘 작성
        //경로 지정
        //경로를 따라 이동
        //턴 종료
    }

    /// <summary>
    /// 적이 고민하는 시간
    /// </summary>
    /// <returns></returns>
    private IEnumerator waitBeforeMove()
    {
        float time = StageManager.Instance.GetStageData().EnemyThinkingTime;
        yield return new WaitForSeconds(Random.Range(2, time));
        inputHolder = false;
        yield break;
    }
}
