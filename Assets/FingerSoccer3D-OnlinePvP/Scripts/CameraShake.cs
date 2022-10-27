using UnityEngine;
using System.Collections;

/// <summary>
/// A simple script to shake the camera when needed
/// </summary>
/// 
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
	private static Vector3 originPosition;
	private static Quaternion originRotation;
	private static float shakeDecay = 0.005f;
	private static float shakeIntensity;
    private static bool isShaking;

    private void Awake()
    {
        instance = this;
        isShaking = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PublicShake(0.3f, 2.5f);
    }

    /// <summary>
    /// Transform to shake
    /// Power of shake
    /// </summary>
    /// <param name="t"></param>
    /// <param name="p"></param>
    public void PublicShake(float p = 0.25f, float d = 0.5f)
    {
        StartCoroutine(Shake(this.transform, p, d));
    }

	public static IEnumerator Shake(Transform t, float i, float duration = 0.5f)
    {
        if (isShaking)
            yield break;
        isShaking = true;

        originPosition = t.position;
		originRotation = t.rotation;
		shakeIntensity = i;
		while (shakeIntensity > 0)
        {
			t.position = originPosition + Random.insideUnitSphere * shakeIntensity;
			t.rotation = new Quaternion (
				originRotation.x + Random.Range (-shakeIntensity, shakeIntensity) * .1f,
				originRotation.y + Random.Range (-shakeIntensity, shakeIntensity) * .1f,
				originRotation.z + Random.Range (-shakeIntensity, shakeIntensity) * .1f,
				originRotation.w + Random.Range (-shakeIntensity, shakeIntensity) * .1f);
			shakeIntensity -= shakeDecay / duration;
			yield return false;
		}

        if (shakeIntensity <= 0)
            isShaking = false;
    }
}
