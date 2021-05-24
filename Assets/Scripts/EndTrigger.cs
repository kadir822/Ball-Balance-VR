using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    public GameObject completeLevelUI;
    public Dragon dragon;
    public DragonTransformation CurrentTransformation;

    public bool win = false;

    private void OnTriggerEnter()
    {
        CompleteLevel();    
    }

    public void CompleteLevel()
    {
        completeLevelUI.SetActive(true);
        win = true;
    }
}
