using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    
    private void Start()
    {
        StartCoroutine(LoadSceneCo());
    }

    IEnumerator LoadSceneCo()
    {
        yield return new WaitForEndOfFrame();
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Stage");
        asyncOperation.allowSceneActivation = false;
        float time = 0f;
        while (!asyncOperation.isDone)
        {
            time += Time.deltaTime;

            if (asyncOperation.progress >= 0.9f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        //�ε��� �ð� + 1��
        time += 1f;

        while (true)
        {
            time -= Time.deltaTime;

            if (time <= 0f)
            {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        asyncOperation.allowSceneActivation = true;

        yield break;
    }
}
