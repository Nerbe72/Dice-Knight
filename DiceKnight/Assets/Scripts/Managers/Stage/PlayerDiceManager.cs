using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDiceManager : MonoBehaviour
{
    public static PlayerDiceManager Instance;

    private Dice selectedDice;

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

    public void SelectDice(Dice _select)
    {
        selectedDice = _select;

        if (selectedDice == null) return;

        selectedDice.SetFrameBlinking();
    }

    public void UnSelectDice()
    {
        if (selectedDice == null) return;

        selectedDice.UnSetFrameBlinking();
        selectedDice.SetTempNumber(selectedDice.GetNumbers());
        selectedDice = null;
    }

    public Dice SelectedDice()
    {
        return selectedDice;
    }
}
