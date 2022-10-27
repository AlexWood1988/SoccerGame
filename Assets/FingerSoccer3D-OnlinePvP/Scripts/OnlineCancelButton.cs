using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class OnlineCancelButton : MonoBehaviourPunCallbacks
{
    public void OnMouseDown()
    {
        print("Clicked on: " + gameObject.name);
        PhotonNetwork.Disconnect();
        StartCoroutine(ChangeScene());
    }

    public void ClickOnCancelButton()
    {
        print("Clicked on: " + gameObject.name);
        PhotonNetwork.Disconnect();
        StartCoroutine(ChangeScene());
    }

    public IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Config-c#");
    }
}
