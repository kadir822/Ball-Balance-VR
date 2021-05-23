using System.Collections;
using UnityEngine;

/*
 * Simple script to manually check and test the connection to the Drag:on.
 * It allows to trigger basic functions inside the Dragon script via keyboard.
 * 
 * Author: André Zenner (andre.zenner@dfki.de)
 * Date: May 2018
 * Last Update: 28.05.2020
 * */
public class DragonTest : MonoBehaviour {

    [Header("Test Drag:on Communication")]
    [Tooltip(
        "Press 1 = Print Drag:on state\n" +
        "Press 2 = Go to current configuration\n" +
        "Press 3 = Update Drag:on state\n" +
        "Press M = all 100%\n" +
        "Press N = all 0%\n" +
        "Press Arrows = obfuscated states\n" +
        "Press Space = Start/Stop loop"
        )]
    public Dragon dragon;
    [Range(0,100)]
    public int PercentA = 0;
    [Range(0,100)]
    public int PercentB = 0;

    public Coroutine OpenCloseLooper;

    public GameObject TextLeft, TextRight;
    public DragonTransformation CurrentTransformation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //animate transformation
        if(CurrentTransformation != null)
        {
            if(TextLeft != null && TextRight != null)
            {
                TextLeft.GetComponent<TextMesh>().text = (CurrentTransformation.getProgressA() * 100f).ToString("0");
                TextRight.GetComponent<TextMesh>().text = (CurrentTransformation.getProgressB() * 100f).ToString("0");
            }
            if (CurrentTransformation.isOver())
                CurrentTransformation = null;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("FAN A is at " + dragon.GetFanState("A") + " and FAN B is at " + dragon.GetFanState("B"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CurrentTransformation = dragon.TransformFans(PercentA, PercentB);
            Debug.Log("FAN A is at " + dragon.GetFanState("A") + " and FAN B is at " + dragon.GetFanState("B"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            dragon.RefreshState();
        }
        if (dragon.GetButtonDownThisFrame())
        {
            Debug.Log("BUTTON PRESSED NOW!");
        }
        if (dragon.GetButtonUpThisFrame())
        {
            Debug.Log("BUTTON RELEASED NOW!");
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentTransformation = dragon.TransformFans(100, 100);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            CurrentTransformation = dragon.TransformFans(0, 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CurrentTransformation = dragon.TransformFansObfuscated(100, 100);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CurrentTransformation = dragon.TransformFansObfuscated(0, 0);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CurrentTransformation = dragon.TransformFansObfuscated(100, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CurrentTransformation = dragon.TransformFansObfuscated(0, 100);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(OpenCloseLooper == null)
            {
                StartCoroutine(OpenCloseLoop());
            }
            else
            {
                StopCoroutine(OpenCloseLooper);
                OpenCloseLooper = null;
            }
        }
    }

    private void OnApplicationQuit()
    {
        if(OpenCloseLooper != null)
        {
            StopCoroutine(OpenCloseLooper);
            OpenCloseLooper = null;
        }
    }

    protected IEnumerator OpenCloseLoop()
    {
        Debug.Log("START ANIMATION");
        yield return new WaitForSeconds(10);
        int wait = 3;
        for(int i=0; i<15; i++)
        {
            if (i % 5 == 0)
            {
                dragon.TransformFans(100, 100);
                yield return new WaitForSeconds(wait);
            }
            else if (i % 5 == 1)
            {
                dragon.TransformFans(0, 0);
                yield return new WaitForSeconds(wait);
            }
            else if (i % 5 == 2)
            {
                dragon.TransformFans(0, 100);
                yield return new WaitForSeconds(wait);
            }
            else if(i % 5 == 3)
            {
                dragon.TransformFans(100, 0);
                yield return new WaitForSeconds(wait);
            }
            else if (i % 5 == 4)
            {
                dragon.TransformFans(0, 0);
                yield return new WaitForSeconds(wait);
            }
        }
        Debug.Log("STOP ANIMATION");
    }
}
