using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NameInputField : MonoBehaviour
{
    public static int maxPlayerNameLength = 12;

    #region Private Constanst 
    //store the PlayerPref key to avoid typos
    const string playerNamePrefKey = "PlayerName";
    #endregion    

    #region MonoBehavior Callbacks
    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        GetComponent<InputField>().readOnly = true;
        //GetComponent<InputField>().interactable = false;

        //string defaultName = string.Empty;
        string defaultName = "Player_" + Random.Range(1000, 9999).ToString();

        //update 2020 - cut playerName if the string is too long
        //defaultName = "abcdefghijklmno";
        int playerNameChars = defaultName.Length;
        print("playerNameChars: " + playerNameChars);
        if (playerNameChars > maxPlayerNameLength)
        {
            defaultName = defaultName.Substring(0, maxPlayerNameLength);
            print("defaultName: " + defaultName);
        }

        InputField inputField = this.GetComponent<InputField>();
        inputField.text = defaultName;

        if (inputField != null)
        {
            if(PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;

        //update 2020  - automatic click on play button
        StartCoroutine(AutomaticStart());
    }
    #endregion


    private IEnumerator AutomaticStart()
    {
        yield return new WaitForSeconds(0.1f);
        LoginManager.instance.connect();
    }


    #region Public Methods
    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerName(string value)
    {
        // #important
        if(string.IsNullOrEmpty(value))
        {
            Debug.Log("Player name is empty or null");
            return;
        }

        PhotonNetwork.NickName = value;

        PlayerPrefs.SetString(playerNamePrefKey,value);
    }
    #endregion

}
