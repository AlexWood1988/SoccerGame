using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiCameraManager : MonoBehaviour
{
    public static MultiCameraManager instance;

    public Camera mainCamera;
    public Camera freestyleCamera;
    public Camera spiderCamera;

    [Space(15)]
    public Transform mouseInputHelper;
    private GameObject ball;

    public static Camera cameraInUse;

    private void Awake()
    {
        instance = this;
        ball = GameObject.FindGameObjectWithTag("ball");
        cameraInUse = mainCamera;
    }
}
