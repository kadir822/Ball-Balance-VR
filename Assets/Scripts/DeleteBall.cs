using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteBall : MonoBehaviour
{
#pragma warning disable CS0108 // Element blendet vererbte Element aus; fehlendes 'new'-Schlüsselwort
    public GameObject gameObject;
#pragma warning restore CS0108 // Element blendet vererbte Element aus; fehlendes 'new'-Schlüsselwort
    public Dragon dragon;
    public DragonTransformation CurrentTransformation;

    // Start is called before the first frame update
    public void BallDestroy()
    {
        gameObject.SetActive(false);

        CurrentTransformation = dragon.TransformFans(0, 0);
    }
}
