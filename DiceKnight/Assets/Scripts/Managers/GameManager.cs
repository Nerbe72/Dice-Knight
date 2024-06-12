using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int clearedStage = 0;

    //보유한 주사위 목록
    private Dictionary<bool, DiceType> ownDice = new Dictionary<bool, DiceType>();

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


    }

    private void Start()
    {
        AddDice();
    }


    //테스트용 주사위 추가
    private void AddDice()
    {
        ownDice.Add(true, DiceType.Normal);
    }

    public void SetStage(Difficulty _diff)
    {
        //난이도별 적 주사위의 위치와 cost를 불러오고 전달을 위해 변수로 저장함
        //가져와야할 내용
        /*
         * 내가 사용 가능한 코스트의 양
         * 내가 배치 가능한 다이스 수
         * 내가 보유한 주사위
         * 내가 보유한 스킬 목록(후순위 추가)
         * 
         * 적의 다이스 종류
         * 적 다이스의 위치
         * 적 다이스의 시작 숫자(랜덤)
         * 적이 생각하는 시간(더미)의 최소값
         * 
         * 적 알고리즘과 관련된 변수
         * 
         */
    }
}
