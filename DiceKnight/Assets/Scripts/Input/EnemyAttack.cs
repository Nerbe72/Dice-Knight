using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EnemyAttack : InputAndAction
{
    public static EnemyAttack Instance;

    private Dictionary<(int x, int y), Dice> dices;
    /// <summary>
    /// 타깃을 가진 경우를 저장함 ( EnemyMove의 것과 반대)
    /// </summary>
    private List<Dice> tempSelectedDices = new List<Dice>();
    private Dice selectedDice = null;
    private (int x, int y) selectedXY;

    private List<Dice> targetDice = new List<Dice>();
    private List<TileData> targetTile = new List<TileData>();

    private (int x, int y) targetXY;

    /// <summary>
    /// 첫 공격좌표가 0번에 들어가며, 그 좌표로부터 떨어진 거리를 포함하고 있음
    /// </summary>
    private List<Vector2> selectedAttackArea = new List<Vector2>();

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

        if (!InputManager.TurnActionList.ContainsKey(Turn.EnemyAttack))
            InputManager.TurnActionList.Add(Turn.EnemyAttack, this);

        turnName = "EnemyAttack";
    }

    protected override void PreAction()
    {
        StartCoroutine(waitBeforeSearch(StageManager.Instance.GetStageData().EnemyThinkingTime));
        Init();
        preActionHolder = true;
    }

    protected override void InputAction()
    {
        //타깃 앞에 있는 주사위 사전 탐색
        SearchingDice();

        //주사위 선택
        SelectingDice();

        //타깃 선택
        SelectingTarget();

        //타깃 표시
        BlinkingTarget();
        selectedDice.SetFrameBlinking();

        StartCoroutine(waitBeforeAttack(stageManager.GetStageData().EnemyThinkingTime));
        inputHolder = true;
    }

    protected override void Action()
    {
        StartCoroutine(attackCo());

        actionHolder = true;
    }

    private void Init()
    {
        dices = StageManager.Instance.GetEnemiesOnBoard();

        tempSelectedDices.Clear();
        selectedDice = null;

        System.GC.Collect();
    }

    private void InitTargets()
    {
        int tileCount = targetTile.Count;
        int diceCount = targetDice.Count;

        for (int i = 0; i < tileCount; i++)
        {
            targetTile[i].UnsetBlinking();
        }

        for (int i = 0; i < diceCount; i++)
        {
            if (targetDice[i] != null)
                targetDice[i].UnSetBlinking();
        }

        targetDice.Clear();
        targetTile.Clear();
        selectedDice.UnSetFrameBlinking();
        selectedDice = null;
        targetXY = (-1, -1);
    }

    private void SearchingDice()
    {
        int enemyCount = dices.Count;
        List<(int x, int y)> diceXs = dices.Keys.ToList<(int x, int y)>();

        for (int e = 0; e < enemyCount; e++)
        {
            if (stageManager.HavePlayerInX(diceXs[e].x))
                tempSelectedDices.Add(dices[diceXs[e]]);
        }
    }

    private void SelectingDice()
    {
        //주사위 (눈 * 공격력)이 가장 높은 주사위 선택
        if (tempSelectedDices.Count != 0)
        {
            selectedDice = tempSelectedDices[0];
            for (int i = 0; i < tempSelectedDices.Count; i++)
            {
                if (selectedDice.GetNumbers().c * selectedDice.GetDamage() < tempSelectedDices[i].GetNumbers().c * selectedDice.GetDamage())
                    selectedDice = tempSelectedDices[i];
            }

            if (selectedDice != null)
                return;
        }

        List<(int x, int y)> keys = dices.Keys.ToList<(int x, int y)>();
        selectedDice = dices[keys[0]];
        for (int i = 0; i < dices.Count; i++)
        {
            if (selectedDice.GetNumbers().c * selectedDice.GetDamage() < dices[keys[i]].GetNumbers().c * selectedDice.GetDamage())
                selectedDice = dices[keys[i]];
        }
    }

    private void SelectingTarget()
    {
        selectedAttackArea = selectedDice.GetAttackArea();
        selectedXY = stageManager.GetXYFromEnemyDice(selectedDice);

        if (selectedXY == (-1, -1))
            return;

        for (int yPos = stageManager.GridYSize - 1; yPos >= 0; yPos--)
        {
            //selectedAttackArea의 첫 칸은 무조건 처음 탐색할 좌표를 가리킴
            //attackPos.x =  선택한 주사위.x + 첫 공격좌표.x
            (int x, int y) attackPos = (selectedXY.x + (int)selectedAttackArea[0].x, yPos);

            //해당 좌표에 주사위가 없으면 다음 y값 탐색
            if (stageManager.GetDiceFromXY(true, attackPos) == null) continue;

            //탐색중 최초 1회 주사위 조우시 해당 위치정보 습득
            targetDice.Add(stageManager.GetDiceFromXY(true, attackPos));
            targetXY = attackPos;
            break;
        }

        //타깃이 없다면 타일의 가장 앞칸을 공격
        if (targetDice.Count == 0)
        {
            targetXY = (selectedXY.x + (int)selectedAttackArea[0].x, stageManager.GridYSize - 1);
            targetTile.Add(stageManager.GetTileDataFromXY(true, targetXY));
        }

        //이후 나머지 공격 범위도 체크
        int attackCount = selectedAttackArea.Count;
        for (int i = 1; i < attackCount; i++)
        {
            //첫 타깃을 기점으로 attackArea의 좌표를 더하여 체크
            (int x, int y) attackPos = (targetXY.x + (int)selectedAttackArea[i].x, targetXY.y + (int)selectedAttackArea[i].y);

            if (stageManager.GetDiceFromXY(true, attackPos) == null)
            {
                if (stageManager.GetTileDataFromXY(true, attackPos) == null) continue;

                targetTile.Add(stageManager.GetTileDataFromXY(true, attackPos));
            }
            else
                targetDice.Add(stageManager.GetDiceFromXY(true, attackPos));
        }

        //가장 뒷 주사위를 선택해 null로 타일이 추가되는 경우를 제거
        targetTile.RemoveAll(x => x == null);
    }

    private void BlinkingTarget()
    {
        int tileCount = targetTile.Count;
        int diceCount = targetDice.Count;

        for (int i = 0; i < tileCount; i++)
        {
            targetTile[i].SetBlinking();
        }

        for (int i = 0; i < diceCount; i++)
        {
            targetDice[i].SetBlinking();
        }
    }

    private IEnumerator waitBeforeSearch(float _wait)
    {
        yield return new WaitForSeconds(Random.Range(1, _wait / 4));
        inputHolder = false;
        yield break;
    }

    private IEnumerator waitBeforeAttack(float _wait)
    {
        yield return new WaitForSeconds(Random.Range(0.8f, _wait / 2));
        actionHolder = false;
        selectedDice.UnSetFrameBlinking();
        yield break;
    }

    private IEnumerator attackCo()
    {
        selectedDice.RunAttackAnimation(selectedDice.GetDiceType());
        float time = 0;
        bool enterHalf = true;

        while (true)
        {
            time += Time.deltaTime * 4.5f;

            if (time >= 0.5f && enterHalf)
            {
                //피격 데미지 출력
                //플레이어가 혼자 남은 경우 타일 공격 가능
                if (stageManager.IsPlayerSolo())
                {
                    int tileCount = targetTile.Count;
                    //갯수만큼 적 체력 감소
                }

                int diceCount = targetDice.Count;
                for (int i = 0; i < diceCount; i++)
                {
                    //데미지 계산 공격력*주사위 눈 - 방어력/2
                    targetDice[i].Hurt(Mathf.Clamp((selectedDice.GetDamage() * selectedDice.GetCurrentNumber().c) - (targetDice[i].GetDefense() * (7 - targetDice[i].GetCurrentNumber().c) * 0.5f), 0, 20) * DebugMode.Instance.MultiplyDMG);
                }
                enterHalf = false;
            }

            if (time >= 1f)
                break;

            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(waitDestroy());
        InitTargets();
        yield break;
    }

    private IEnumerator waitDestroy()
    {
        //주사위가 파괴되는 시간 : 0.5초
        yield return new WaitForSeconds(0.5f);
        stageManager.NextTurn();
        yield break;
    }
}
