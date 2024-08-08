using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAttack : InputAndAction
{
    public static EnemyAttack Instance;

    private Dictionary<(int x, int y), Dice> dices;
    /// <summary>
    /// Ÿ���� ���� ��츦 ������ ( EnemyMove�� �Ͱ� �ݴ�)
    /// </summary>
    private List<Dice> tempSelectedDices = new List<Dice>();
    private Dice selectedDice = null;
    private (int x, int y) selectedXY;

    private List<Dice> targetDice = new List<Dice>();
    private List<TileData> targetTile = new List<TileData>();

    private (int x, int y) targetXY;

    /// <summary>
    /// ù ������ǥ�� 0���� ����, �� ��ǥ�κ��� ������ �Ÿ��� �����ϰ� ����
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
        //Ÿ�� �տ� �ִ� �ֻ��� ���� Ž��
        SearchingDice();

        //�ֻ��� ����
        SelectingDice();

        //Ÿ�� ����
        SelectingTarget();

        //Ÿ�� ǥ��
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
        //�ֻ��� (�� * ���ݷ�)�� ���� ���� �ֻ��� ����
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
            //selectedAttackArea�� ù ĭ�� ������ ó�� Ž���� ��ǥ�� ����Ŵ
            //attackPos.x =  ������ �ֻ���.x + ù ������ǥ.x
            (int x, int y) attackPos = (selectedXY.x + (int)selectedAttackArea[0].x, yPos);

            //�ش� ��ǥ�� �ֻ����� ������ ���� y�� Ž��
            if (stageManager.GetDiceFromXY(true, attackPos) == null) continue;

            //Ž���� ���� 1ȸ �ֻ��� ����� �ش� ��ġ���� ����
            targetDice.Add(stageManager.GetDiceFromXY(true, attackPos));
            targetXY = attackPos;
            break;
        }

        //Ÿ���� ���ٸ� Ÿ���� ���� ��ĭ�� ����
        if (targetDice.Count == 0)
        {
            targetXY = (selectedXY.x + (int)selectedAttackArea[0].x, stageManager.GridYSize - 1);
            targetTile.Add(stageManager.GetTileDataFromXY(true, targetXY));
        }

        //���� ������ ���� ������ üũ
        int attackCount = selectedAttackArea.Count;
        for (int i = 1; i < attackCount; i++)
        {
            //ù Ÿ���� �������� attackArea�� ��ǥ�� ���Ͽ� üũ
            (int x, int y) attackPos = (targetXY.x + (int)selectedAttackArea[i].x, targetXY.y + (int)selectedAttackArea[i].y);

            if (stageManager.GetDiceFromXY(true, attackPos) == null)
            {
                if (stageManager.GetTileDataFromXY(true, attackPos) == null) continue;

                targetTile.Add(stageManager.GetTileDataFromXY(true, attackPos));
            }
            else
                targetDice.Add(stageManager.GetDiceFromXY(true, attackPos));
        }

        //���� �� �ֻ����� ������ null�� Ÿ���� �߰��Ǵ� ��츦 ����
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
                //�ǰ� ������ ���
                //�÷��̾ ȥ�� ���� ��� Ÿ�� ���� ����
                if (stageManager.IsPlayerSolo())
                {
                    int tileCount = targetTile.Count;
                    //������ŭ �� ü�� ����
                }

                int diceCount = targetDice.Count;
                for (int i = 0; i < diceCount; i++)
                {
                    //������ ��� ���ݷ�*�ֻ��� �� - ����/2
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
        //�ֻ����� �ı��Ǵ� �ð� : 0.5��
        yield return new WaitForSeconds(0.5f);
        stageManager.NextTurn();
        yield break;
    }
}
