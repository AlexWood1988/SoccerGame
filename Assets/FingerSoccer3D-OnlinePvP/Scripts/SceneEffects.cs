using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEffects : MonoBehaviour
{
    public int effectID = 1;
    private float rotAmount;
    private float dir = -1;
    private float speed = 1.25f;

    void Start()
    {
        StartCoroutine(RunEffect());
    }

    void Update()
    {
        rotAmount = Time.deltaTime * 355;
        transform.eulerAngles += new Vector3(0, 0, rotAmount * dir);
    }


    public IEnumerator RunEffect()
    {
        float t = 0;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = transform.localScale * 1.2f;

        while(t < 1)
        {
            t += Time.deltaTime * speed;

            //scale
            transform.localScale = new Vector3(
                Mathf.SmoothStep(startScale.x, targetScale.x, t),
                Mathf.SmoothStep(startScale.y, targetScale.y, t),
                startScale.z
                );

            if (t >= 0.95f || t <= 0.05f)
            {
                speed *= -1;
            }

            yield return 0;
        }          
    }

}
