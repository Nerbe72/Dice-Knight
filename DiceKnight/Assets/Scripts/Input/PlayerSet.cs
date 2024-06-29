using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.RestService;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSet : InputAndAction
{
    public static PlayerSet Instance;

    private PlayerDiceManager playerDiceManager;

    [SerializeField] private GameObject tray;
    [SerializeField] private GameObject recallTray;
    [SerializeField] private Button nextBtn;

    private delegate Dice DelegateSelectedDice();
    private DelegateSelectedDice selectedDice;

    private Vector3 trayDicePos;
    private Vector3 holdedDicePos;

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

        if (!InputManager.TurnActionList.ContainsKey(Turn.PlayerSet))
            InputManager.TurnActionList.Add(Turn.PlayerSet, this);

        turnName = "PlayerSet";
        nextBtn.onClick.AddListener(ClickEnd);
    }

    protected override void Start()
    {
        stageManager = StageManager.Instance;
        playerDiceManager = PlayerDiceManager.Instance;
        selectedDice = playerDiceManager.SelectedDice;
        base.Start();
    }

    //사전 동작 없음
    protected override void PreAction()
    {
        stageManager.CloseController();
        nextBtn.gameObject.SetActive(true);
        recallTray.SetActive(true);

        preActionHolder = true;
        inputHolder = false;
    }

    protected override void InputAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (playerDiceManager.SelectedDice() == null)
                SelectTarget();
            else
                PutTarget();
        }
    }

    private void SelectTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, LayerMask.GetMask("Dice"));

        if (hit.collider == null)
        {
            StageManager.Instance.CloseStatUI();
            return;
        }

        if (hit.collider.tag == "Dice")
        {
            playerDiceManager.SelectDice(hit.collider.gameObject.GetComponent<Dice>());
            holdedDicePos = playerDiceManager.SelectedDice().transform.position;
            stageManager.ChooseDice(true);
        }
    }

    private void PutTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider == null || hit.collider.tag == "Dice")
        {
            stageManager.ChooseDice(false);
            return;
        }

        if (hit.collider.tag == "Tile")
        {
            TileData tileData = hit.collider.GetComponent<TileData>();

            if (!stageManager.AddPlayerDiceOnBoard(tileData, selectedDice())) return;

            Vector3 tilePos = hit.collider.transform.position;
            selectedDice().transform.parent = null;
            selectedDice().transform.position = tilePos;

            selectedDice().SetRandomNumber();
            selectedDice().SetNumberUI();
        }

        if (hit.collider.tag == "DiceTray")
        {
            SideTrayController.Instance.RecallDice(selectedDice());
        }

        stageManager.ChooseDice(false);
        
    }

    private void ClickEnd()
    {
        if (stageManager.HideList(tray))
        {
            stageManager.NextTurn();
            nextBtn.gameObject.SetActive(false);
            recallTray.SetActive(false);
            this.enabled = false;
        }
    }
}
