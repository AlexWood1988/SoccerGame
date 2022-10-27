using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public bool isInCollision;

    void Awake()
    {
        isInCollision = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other != null)
        {
            isInCollision = true;
            //print("In collision with " + other.gameObject.name);
        }
    }
}
