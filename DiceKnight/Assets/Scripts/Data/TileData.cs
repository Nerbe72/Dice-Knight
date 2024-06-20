using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData : MonoBehaviour
{
    //가져야 하는 데이터
    // x, y, 주사위 존재 유무, 타겟팅 유무

    public int x;
    public int y;
    public bool attackTargeted { get; set; }

    private SpriteRenderer spriteRenderer;

    private Coroutine targetedCo;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public (int, int) GetXY()
    {
        return (x, y);
    }

    public void SetBlinking()
    {
        attackTargeted = true;
        if (targetedCo != null) StopCoroutine(targetedCo);
        targetedCo = StartCoroutine(targetedBlinkCo());
    }

    public void UnsetBlinking()
    {
        attackTargeted = false;
        if (targetedCo != null)
        {
            StopCoroutine(targetedCo);
            targetedCo = null;
        }
        spriteRenderer.color = Color.white;
    }

    private IEnumerator targetedBlinkCo()
    {
        float time = 0f;
        bool switcher = true;
        while (attackTargeted)
        {
            time += Time.deltaTime * 2f;

            if (switcher)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, time);
            }
            else
            {
                spriteRenderer.color = Color.Lerp(Color.red, Color.white, time);
            }

            if (time >= 1f)
            {
                time = 0f;
                switcher = !switcher;
            }

            yield return new WaitForEndOfFrame();
        }

        spriteRenderer.color = Color.white;
        targetedCo = null;
        yield break;
    }
}
