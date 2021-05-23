using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceFan : MonoBehaviour
{
    Vector3 ballpos;

    public GameObject Sphere;
    public GameObject Qube;
    public Dragon dragon;
    public DragonTransformation CurrentTransformation;

    public float angularVelocity;
    private float ballposFloat;

    private int ballposInt;
    private float time = 0.0f;

    public EndTrigger eT;

    // Start is called before the first frame update
    void Start()
    {
        //Sphere.SetActive(true);

        // Postion of the Sphere
        ballpos = transform.position;

        //maximimum angular velocity of the rigidbody access
        GetComponent<Rigidbody>().maxAngularVelocity = angularVelocity;

        CurrentTransformation = dragon.TransformFans(100,100);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        ballpos = transform.position;

        //Float in Int
        ballposFloat = ballpos.x * 200;
        ballposInt = (int)ballposFloat;
            
        if (ballpos.y > 0  && !eT.win)
        {
            if (time > 3 && time < 5)
            {
                Debug.Log(time);
                Qube.SetActive(false);
            }
            
            //Move the fans to the postion of the ball
            if (ballpos.x < 0f)
            {
                CurrentTransformation = dragon.TransformFans(0, ballposInt * (-1));
                    
            }
            else
            {
                CurrentTransformation = dragon.TransformFans(ballposInt, 0);

            }
            
        }
    }
}
