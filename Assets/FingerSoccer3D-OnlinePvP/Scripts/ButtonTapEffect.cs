using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTapEffect : MonoBehaviour
{
    private bool canTap;
    public AudioClip tapSfx;
    private RaycastHit hitInfo;
    private Ray ray;

    // Start is called before the first frame update
    void Start()
    {
        canTap = false;
        StartCoroutine(ReactiveTap());
    }

    public void OnMouseDown()
    {
        if(canTap)
        {
            print("Clicked on: " + gameObject.name);
            playSfx(tapSfx);
            StartCoroutine(animateButton(this.gameObject));
        }      
    }

    IEnumerator animateButton(GameObject _btn)
    {
        canTap = false;
        StartCoroutine(ReactiveTap());
        Vector3 startingScale = _btn.transform.localScale;  //initial scale	
        Vector3 destinationScale = startingScale * 0.85f;       //target scale

        //Scale up
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime * 12f;
            _btn.transform.localScale = new Vector3(Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
                                                    Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
                                                    _btn.transform.localScale.z);
            yield return 0;
        }

        //Scale down
        float r = 0.0f;
        if (_btn.transform.localScale.x >= destinationScale.x)
        {
            while (r <= 1.0f)
            {
                r += Time.deltaTime * 10f;
                _btn.transform.localScale = new Vector3(Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
                                                        Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
                                                        _btn.transform.localScale.z);
                yield return 0;
            }
        }

        /*if(r >= 1)
			canTap = true;*/
    }


    IEnumerator ReactiveTap()
    {
        yield return new WaitForSeconds(1.0f);
        canTap = true;
    }

	void playSfx ( AudioClip _clip  )
    {
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}

}
