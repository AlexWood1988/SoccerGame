using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperController : MonoBehaviour
{
    public AudioClip bumperSfx;
    private AudioSource ascomp;
    private Vector3 defaultScale;
    private Vector3 targetScale;
    private int bounceNumber = 6;

    private bool isAnimating;

    void Start()
    {
        isAnimating = false;
        ascomp = GetComponent<AudioSource>();
        defaultScale = transform.localScale;
        targetScale = transform.localScale * 0.9f;
        //print("defaultScale: " + defaultScale);
    }


    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "ball" || collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player_2" || collision.gameObject.tag == "Opponent")
        {
            //print("A unit (or ball) collided with a bumper.");
            ascomp.PlayOneShot(bumperSfx);

            //apply some force to keep the collision away from this bumper
            Vector3 bumperForce = collision.contacts[0].normal;
            print("bumperForce: " + bumperForce);
            collision.gameObject.GetComponent<Rigidbody>().AddForce(bumperForce * -3f, ForceMode.Impulse);

            if (!isAnimating)
                StartCoroutine(AnimScale());
        }
    }


    public IEnumerator AnimScale()
    {
        transform.localScale = defaultScale;
        isAnimating = true;

        for (int i = 0; i < bounceNumber; i++)
        {
            if(i == bounceNumber - 1)
            {
                print("Bumper anim is done.");
                transform.localScale = defaultScale;
                isAnimating = false;
                yield break;
            }

            if(i % 2 == 0)
            {
                transform.localScale = targetScale;

            } else
            {
                transform.localScale = defaultScale;
            }

            yield return new WaitForSeconds(0.045f);
        }
    }

}

