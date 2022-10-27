using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarRandomizer : MonoBehaviour
{
    public static AvatarRandomizer instance;
    public static bool matchIsFound;
    public AudioClip matchingSfx;
    public Texture2D avatarAtlas;
    public Texture2D avatarFinal;
    private Renderer r;
    private float offset;
    private float randomOffset;

    void Awake()
    {
        instance = this;
        matchIsFound = false;
        randomOffset = Random.Range(1, 10);
        r = GetComponent<Renderer>();
    }


    void Update()
    {
        RotateAvatar();
    }


    public void RotateAvatar()
    {
        if (!matchIsFound)
        {
            offset = Time.time * 0.55f;
            r.material.SetTextureOffset("_MainTex", new Vector2(0, offset));
            PlaySfx(matchingSfx);

        } else
        {
            //Option #1
            //r.material.SetTextureOffset("_MainTex", new Vector2(0, 0));
            //r.material.SetTextureScale("_MainTex", new Vector2(1, 1));
            //r.material.mainTexture = avatarFinal; //player 2 avatar here

            //Option #2
            r.material.SetTextureOffset("_MainTex", new Vector2(0, randomOffset / 10f));
        }          
    }

    /// <summary>
    /// Play sound clips
    /// </summary>
    /// <param name="_clip"></param>
    void PlaySfx(AudioClip _clip)
    {
        GetComponent<AudioSource>().clip = _clip;
        if (!GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
        }
    }

}
