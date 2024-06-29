using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int clearedStage;

    [SerializeField] private List<GameObject> everyPlayerDices;
    [SerializeField] private List<GameObject> everyEnemyDices;
    private StageData selectedStageData;

    //보유한 주사위 목록
    private Dictionary<bool, List<GameObject>> ownDice = new Dictionary<bool, List<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this);

        LoadOwnDiceList();

        Application.targetFrameRate = 60;
    }
    
    private void AddDice()
    {
        //테스트용 주사위 추가
        ownDice[true].Add(everyPlayerDices[0]);
        ownDice[true].Add(everyPlayerDices[1]);
    }

    public void LoadOwnDiceList()
    {
        ownDice.Add(true, new List<GameObject>());
        ownDice.Add(false, new List<GameObject>());

        //소유한 주사위 목록 불러오기
        //불러온 정보를 기반으로 (보유여부, 주사위prefab리스트)로 구성된 딕셔너리를 구성
        AddDice();
    }

    public void LoadStageDataFromJson(Difficulty _diff)
    {
        //난이도별 적 주사위의 위치와 cost를 불러오고 전달을 위해 변수로 저장함
        //가져와야할 내용
        /*
         * 사용 가능한 코스트의 양
         * 배치 가능한 다이스 수
         * 내가 보유한 주사위
         * 
         * 적의 다이스 종류
         * 적 다이스의 위치
         * 적이 생각하는 시간(더미)의 최소값
         */

        string stagePath = Path.Combine(Application.dataPath + "/StageDatas", _diff + ".json");

        try
        {
            string stageJson = File.ReadAllText(stagePath);
            selectedStageData = JsonUtility.FromJson<StageData>(stageJson);
        }
        catch
        {
            return;
        }
    }

    public StageData GetStageData()
    {
        return selectedStageData;
    }

    public (int, int) XYFromVec2(Vector2 _vecPos)
    {
        return (Mathf.RoundToInt(_vecPos.x), Mathf.RoundToInt(_vecPos.y));
    }

    public List<GameObject> GetOwnDiceList()
    {
        return ownDice.GetValueOrDefault(true);
    }

    public GameObject GetEnemyDiceAtIndex(int _index)
    {
        return everyEnemyDices[_index];
    }

    public GameObject GetEnemyDiceAtType(DiceType _type)
    {
        for (int i = 0; i < everyEnemyDices.Count; i++)
        {
            if (everyEnemyDices[i].GetComponent<Dice>().GetDiceType() == _type)
            {
                return everyEnemyDices[i];
            }
        }
        return null;
    }
}
