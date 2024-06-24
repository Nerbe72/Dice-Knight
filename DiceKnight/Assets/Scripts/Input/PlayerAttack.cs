using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : InputAndAction
{
    public static PlayerAttack Instance;

    [SerializeField] private Button okBtn;

    private Dice selectedDice;
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
        InputManager.TurnActionList.Add(Turn.PlayerAttack, this);

        turnName = "PlayerAttack";

        okBtn.onClick.AddListener(DoAttack);
    }

    protected override void Start()
    {
        stageManager = StageManager.Instance;
        base.Start();
    }

    protected override void InputAction()
    {
        if (!okBtn.gameObject.activeSelf) okBtn.gameObject.SetActive(true);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, LayerMask.GetMask("Dice"));

            if (hit.collider == null || hit.collider.tag != "Dice") return;

            //필요한 값 초기화
            if (selectedDice != null)
                InitTargets();

            selectedDice = hit.collider.GetComponent<Dice>();
            selectedDice.SetFrameBlinking();

            SetTarget();
        }
    }

    protected override void Action()
    {
        StartCoroutine(attackCo());

        actionHolder = true;
    }

    private void SetTarget()
    {
        selectedAttackArea = selectedDice.GetAttackArea();
        selectedXY = stageManager.GetXYFromPlayerDice(selectedDice);

        if (selectedXY == (-1, -1))
            return;


        for (int yPos = 0; yPos < stageManager.GridYSize; yPos++)
        {
            //selectedAttackArea의 첫 칸은 무조건 처음 탐색할 좌표를 가리킴
            //attackPos.x =  선택한 주사위.x + 첫 공격좌표.x
            (int x, int y) attackPos = (selectedXY.x + (int)selectedAttackArea[0].x, yPos);

            //해당 좌표에 주사위가 없으면 다음 y값 탐색
            if (stageManager.GetDiceFromXY(false, attackPos) == null) continue;

            //탐색중 최초 1회 주사위 조우시 해당 위치정보 습득
            targetDice.Add(stageManager.GetDiceFromXY(false, attackPos));
            targetXY = attackPos;
            break;
        }

        //타깃이 없다면 타일의 가장 앞칸을 공격
        if (targetDice.Count == 0)
        {
            targetXY = (selectedXY.x + (int)selectedAttackArea[0].x, 0);
            targetTile.Add(stageManager.GetTileDataFromXY(false, targetXY));
        }

        //이후 나머지 공격 범위도 체크
        int attackCount = selectedAttackArea.Count;
        for (int i = 1; i < attackCount; i++)
        {
            //첫 타깃을 기점으로 attackArea의 좌표를 더하여 체크
            (int x, int y) attackPos = (targetXY.x + (int)selectedAttackArea[i].x, targetXY.y + (int)selectedAttackArea[i].y);

            if (stageManager.GetDiceFromXY(false, attackPos) == null)
                targetTile.Add(stageManager.GetTileDataFromXY(false, attackPos));
            else
                targetDice.Add(stageManager.GetDiceFromXY(false, attackPos));
        }

        //공격 타깃 시각적 표시
        BlinkTargets();
    }

    private void DoAttack()
    {
        //공격 대상 지정 안됨 체크
        if (selectedDice == null) return;

        okBtn.gameObject.SetActive(false);
        inputHolder = true;
        actionHolder = false;
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

    private void BlinkTargets()
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

        InitTargets();
        StartCoroutine(waitDestroy());
        yield break;
    }

    /// <summary>
    /// 파괴될 주사위가 있는 경우가 존재하기 때문에 2초간의 여유 시간을 제공
    /// </summary>
    /// <returns></returns>
    private IEnumerator waitDestroy()
    {
        //주사위가 파괴되는 시간 : 0.5초
        yield return new WaitForSeconds(0.5f);
        stageManager.NextTurn();
        yield break;
    }
}
