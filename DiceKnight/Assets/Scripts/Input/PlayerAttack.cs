using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : InputAndAction
{
    public static PlayerAttack Instance;

    private StageManager stageManager;

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

    protected override void InputStyle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, LayerMask.GetMask("Dice"));

            if (hit.collider == null || hit.collider.tag != "Dice")
            {
                if (selectedDice != null)
                    selectedDice.UnSetFrameBlinking();
                InitTargets();
                return;
            }

            selectedDice = hit.collider.GetComponent<Dice>();
            selectedDice.SetFrameBlinking();

            SetTarget();
        }
    }

    protected override void Action()
    {
        //플레이어가 혼자 남은 경우 타일 공격 가능
        if (stageManager.GetPlayerIsSolo())
        {

        }
        //run Animation (코루틴)
        //애니메이션이 끝날 때 데미지를 넣고 데미지 표시
        //
        //피격된 적 주사위 체력 감소
        //피격된 본체 체력 감소
        //nexturn

        //데미지 넣는 반복문
        //int diceCount = targetDice.Count;
        //for (int i = 0; i < diceCount; i++)
        //{

        //}

        //int tileCount = targetTile.Count;
        //for (int i = 0; i < tileCount; i++)
        //{

        //}


    }

    private void SetTarget()
    {
        //필요한 값 초기화
        InitTargets();

        selectedAttackArea = selectedDice.GetAttackArea();
        selectedXY = stageManager.GetXYFromDice(selectedDice);

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
        if (targetXY == (-1, -1))
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

        //선택된 내용에 맞춰 공격 범위 및 대상 강조
        //setblinkenemy - 공격 대상 위치에 있는 enemyDices[(x, y)]의 이미지를 빨갛게 강조하도록 설정
    }

    private void DoAttack()
    {
        //공격 대상 지정 안됨 체크

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
            targetDice[i].UnSetBlinking();
        }

        targetDice.Clear();
        targetTile.Clear();
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

    private void AttackTile()
    {

    }

    private void AttackEnemy()
    {

    }
}
