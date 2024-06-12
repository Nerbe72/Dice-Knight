using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerDiceCointroller : Dice
{
    public static PlayerDiceCointroller Instance;

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

    }

    private void OnMouseOver()
    {
        StageManager.Instance.OpenStatUI(this);
    }

    private void OnMouseExit()
    {
        StageManager.Instance.CloseStatUI();
    }

}
