using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Stage(int _diff)
    {
        Difficulty diff = (Difficulty)_diff;
        GameManager.Instance.LoadStageDataFromJson(diff);
        //¾À ·Îµå
        SceneManager.LoadScene("Stage");
    }
}
