using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activateBall : MonoBehaviour
{
    private float time = 0.0f;
#pragma warning disable CS0108 // Element blendet vererbte Element aus; fehlendes 'new'-Schlüsselwort
    public GameObject gameObject;
#pragma warning restore CS0108 // Element blendet vererbte Element aus; fehlendes 'new'-Schlüsselwort

    public Dragon dragon;
    public DragonTransformation CurrentTransformation;
    public EndTrigger endTrigger;

    // Update is called once per frame
    void Update()
    {

        time += Time.deltaTime;


        if(gameObject.transform.position.y < -5)
        {           
            if (!endTrigger.win)
            {
                gameObject.transform.position = new Vector3(0, 1.77f, -1);
            }
            CurrentTransformation = dragon.TransformFans(0, 0);
        }
    }
}
