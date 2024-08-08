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
            Debug.Log("Loading");
            if (asyncOperation.progress >= 0.9f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        //로딩된 시간 + 1초
        time = 1.5f;

        while (true)
        {
            time -= Time.deltaTime;
            Debug.Log("Loading Dummy");
            if (time <= 0f)
            {
                break;
            }

            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Loading End");

        asyncOperation.allowSceneActivation = true;

        yield break;
    }
}
