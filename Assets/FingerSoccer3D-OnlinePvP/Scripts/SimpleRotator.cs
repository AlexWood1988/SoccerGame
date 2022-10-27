using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    private float rotAmount;
    private float dir = 1;

    void Update()
    {
        //transform.Rotate(0, 0, Time.deltaTime * 45);

        if (transform.eulerAngles.z > 45)
            dir *= -1;

        rotAmount = Time.deltaTime * 45;
        transform.eulerAngles += new Vector3(0, 0, rotAmount * dir);
    }
}
