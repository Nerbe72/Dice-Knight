using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    (int x, int y) currentGridPos;

    public bool IsSelected;

    [SerializeField] GameObject selectedFrame;

    private void Awake()
    {
        Init();
    }

    void Update()
    {
        ChangeSelectedAnimation();
    }

    private void Init()
    {
        IsSelected = false;
        currentGridPos = (0, 0);
        selectedFrame.SetActive(false);
    }

    private void ChangeSelectedAnimation()
    {
        if (IsSelected & !selectedFrame.activeSelf)
        {
            //선택되었다는것을 표시
            selectedFrame.SetActive(true);
        }
    }

    

    public void SetGridPosition(int x, int y)
    {
        currentGridPos = (x, y);
    }

    public (int, int) GetGridPosition()
    {
        return currentGridPos;
    }


}
