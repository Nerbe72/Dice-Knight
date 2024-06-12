using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButtonController : MonoBehaviour
{ 
    [SerializeField] private GameObject stageListPage;

    public void OpenStageList()
    {
        stageListPage.SetActive(true);
    }

    public void CloseStageList()
    {
        stageListPage.SetActive(false);
    }

    public void Stage(Difficulty _diff)
    {
        
    }
}
