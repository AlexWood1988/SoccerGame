using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LoginManager : MonoBehaviourPunCallbacks
{
    public static LoginManager instance;
    private string gameVersion = "1"; //game version as string to sync all client version
    [SerializeField] private GameObject status;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        status.SetActive(false);
    }


    /// <summary>
    /// connect to photon server 
    /// </summary>
    public void connect()
    {
        status.SetActive(true); //show progress text and hide login button
        setPlayerProperties();

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    /// <summary>
    /// save player team and formation in photon player custom properties 
    /// </summary>
    private void setPlayerProperties()
    {
        var playerFlag = PlayerPrefs.GetInt("PlayerFlag");
        var playerFormation = PlayerPrefs.GetInt("PlayerFormation");
        
        Hashtable hashtable = new Hashtable();
        hashtable.Add("Flag",playerFlag);
        hashtable.Add("Formation",playerFormation);
        
        PhotonNetwork.SetPlayerCustomProperties(hashtable);
    }

    /// <summary>
    /// connected to master client
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("connected to master");
        PhotonNetwork.JoinRandomRoom();
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("join random room failed");
        PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = 2});
    }


    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MatchMaking-c#");
    }
}
