using UnityEngine;
using System.Collections;

public class mouseFollow : MonoBehaviour
{
    ///*************************************************************************///
    /// Mouse Follower class.
    /// mouseHelperBegin is a helper gameObject which always follows mouse position
    /// and provides useful informations for various controllers.
    /// This script also works fine with touch.
    ///*************************************************************************///

    public Camera cam;
    private float zOffset = -0.5f; //fixed position on Z axis.
    private Vector3 tmpPosition;

    //New cam mode
    [Space(20)]
    public Camera camDefault;
    public Camera camLeft;
    public Camera camRight;
    public Camera camSpider;

    void Start ()
    {
		transform.position = new Vector3(transform.position.x, transform.position.y, zOffset);
	}
	
	void Update ()
    {
        if (playerController.selectedUnit != null)
        {
            cam = camSpider;
            camSpider.transform.parent = playerController.selectedUnit.GetComponent<playerController>().cameraPosition.transform;
            camSpider.transform.localPosition = new Vector3(0, 0, 0);
            camSpider.transform.localEulerAngles = playerController.selectedUnit.GetComponent<playerController>().cameraPosition.transform.localEulerAngles + 
                                                   new Vector3(0,0, -25f);
            camSpider.depth = 3f;
        }
        else
        {
            cam = camDefault;

            //Reset spidercam
            camSpider.depth = -5f;
        }

        float preciseZ = 0.25f + (Input.mousePosition.y / 14f);    //Warning. Do not edit !
        tmpPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, preciseZ));
        transform.position = new Vector3(tmpPosition.x, tmpPosition.y, zOffset);
    }

}