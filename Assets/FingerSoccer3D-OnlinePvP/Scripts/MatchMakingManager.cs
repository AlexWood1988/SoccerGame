using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchMakingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject player_1_Name;
    [SerializeField] private GameObject opponentName;
    [SerializeField] private GameObject title;

    //Using fake bot if game is not able to find a pair after some time
    private float timePassedForSearch;
    private bool canLoadFakeBotAsPair;     // (DO NOT EDIT!) flag used to pair a fake bot just once

    private void Awake()
    {
        timePassedForSearch = 0;
        canLoadFakeBotAsPair = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players)
                {
                    if (player.Value.IsMasterClient)
                    {
                        //get master client properties
                        var formation = (int)player.Value.CustomProperties["Formation"];
                        var flag = (int)player.Value.CustomProperties["Flag"];

                        updateRoomProp(true, flag, formation);
                        StartCoroutine(loadGame(player.Value, flag, formation));

                    }
                }
            }
        }

        //Only if we want to show our dynamic name in search for opponent scene!
        if (PhotonNetwork.IsMasterClient)
        {
            player_1_Name.GetComponent<TextMesh>().text = PhotonNetwork.NickName;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //get new player properties
        var formation = (int) newPlayer.CustomProperties["Formation"];
        var flag = (int) newPlayer.CustomProperties["Flag"];

        updateRoomProp(false,flag,formation);
        StartCoroutine(loadGame(newPlayer,flag,formation));
    }


    /// <summary>
    /// add player flag and formation info to room properties
    /// </summary>
    /// <param name="is_server"></param>
    /// <param name="flag"></param>
    /// <param name="formation"></param>
    private void updateRoomProp(bool is_server,int flag,int formation)
    {
        Hashtable hashtable = new Hashtable();

        if (is_server)
        {
            hashtable.Add("serverFlag",flag);
            hashtable.Add("serverFormation",formation);
        }
        else
        {
            hashtable.Add("clientFlag",flag);
            hashtable.Add("clientFormation",formation);
        }
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    }


    private IEnumerator loadGame(Player player,int flag,int formation)
    {
        opponentName.GetComponent<TextMesh>().text = /*opponentName.GetComponent<TextMesh>().text +*/ player.NickName;        
        opponentName.SetActive(true);
        
        //set opponent properties
        PlayerPrefs.SetInt("Player2Flag",flag);
        PlayerPrefs.SetInt("Player2Formation",formation);
        
        title.GetComponent<TextMesh>().text = "Loading Game...";
        AvatarRandomizer.matchIsFound = true;

        yield return new WaitForSeconds(3f);
        
        PhotonNetwork.LoadLevel("Game-c#");
    }


    private void Update()
    {
        //update timePassedForSearch so we can monitor how many seconds we are searching for a human player
        timePassedForSearch = Time.timeSinceLevelLoad;
        print("TimePassedForSearch: " + timePassedForSearch);

        if (timePassedForSearch > GlobalGameSettings.instance.fakeOpponentSearchTime && GlobalGameSettings.instance.useFakeOpponent && canLoadFakeBotAsPair)
        {
            canLoadFakeBotAsPair = false;
            LoadFakeBotAsPair();
        }
    }


    /// <summary>
    /// We use this method to load a fake bot & oair it with player so they can start the game without waiting forever!
    /// of course, we need to use special attributes for the bot, so it can behave & play like a human.
    /// </summary>
    public void LoadFakeBotAsPair()
    {
        Debug.LogWarning("Loading Fake Bot!");

        //Set fake game mode (normal single player)
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.SetInt("IsTournament", 0);
        PlayerPrefs.SetInt("IsPenalty", 0);
        PlayerPrefs.SetInt("IsOnline", 0);
        //New
        PlayerPrefs.SetInt("IsFakeOnlineWithBot", 1);

        //Set new "Fake" names for player & Bot
        PlayerPrefs.SetString("FakeP1_Name", "Player_" + UnityEngine.Random.Range(1111, 9999));
        PlayerPrefs.SetString("FakeBot_Name", "Player_" + UnityEngine.Random.Range(1111, 9999));

        //Set new "Fake" formation & flag for the bot
        PlayerPrefs.SetInt("Player2Formation", 0);
        PlayerPrefs.SetInt("OpponentFormation", 0);
        int rndFlag = UnityEngine.Random.Range(0, 32);
        PlayerPrefs.SetInt("Player2Flag", rndFlag);        
        PlayerPrefs.SetInt("OpponentFlag", rndFlag);

        //Load a normal single player game
        opponentName.GetComponent<TextMesh>().text = PlayerPrefs.GetString("FakeBot_Name");
        opponentName.SetActive(true);
        title.GetComponent<TextMesh>().text = "Loading Game...";
        AvatarRandomizer.matchIsFound = true;
        StartCoroutine(LoadGame());
    }

    public IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.LoadLevel("Game-c#");
    }

}
