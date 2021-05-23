using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceFanNOT : MonoBehaviour
{
    Vector3 ballpos;

    public float angularVelocity; 

    // Start is called before the first frame update
    void Start()
    {
        // Postion of the Sphere
        ballpos = transform.position;

        //maximimum angular velocity of the rigidbody access
        GetComponent<Rigidbody>().maxAngularVelocity = angularVelocity;
    }
}
