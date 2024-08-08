using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MakeStageData : MonoBehaviour
{
    [Tooltip("�ҷ��� ���̵��� ����(Load Json)")][SerializeField] private Difficulty difficulty;
    [SerializeField] private StageData stageData;

    [ContextMenu("To Json Data")]
    void SaveStageDataToJson()
    {
        string jsonData = JsonUtility.ToJson(stageData);
        string path = Path.Combine(Application.dataPath + "/Resources/StageDatas", stageData.StageDifficulty + ".json");
        File.WriteAllText(path, jsonData);
        print("Successfully Exported\n" + path);
    }

    [ContextMenu("Load Json Data")]
    void LoadStageDataFromJson()
    {
        string stagePath = Path.Combine(Application.dataPath + "/Resources/StageDatas", difficulty + ".json");

        try
        {
            string stageJson = File.ReadAllText(stagePath);
            stageData = JsonUtility.FromJson<StageData>(stageJson);
            print("Data Loaded");
        }
        catch
        {
            return;
        }
    }
}
