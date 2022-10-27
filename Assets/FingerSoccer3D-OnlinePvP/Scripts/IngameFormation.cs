using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class IngameFormation : MonoBehaviour
{
	/// <summary>
	/// In-game formation manager
	/// </summary>

	//we need to know if player-1 or player-2 are changing their formation!
	//so we assign an ID for each one to know who is sending the request.
	// ID 1 = Player-1
	// ID 2 = Player-2
	public static int formationChangeRequestID;
	public GameObject[] availableFormationButtons;	//array containing all formation buttons we are using inisde "newFormationPlane" game object
	public GameObject tickGo;
	public Text requestByLabel;
	private int player1Formation;
	private int player2Formation;
	private GlobalGameManager gameController;


	private void Awake()
	{
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GlobalGameManager>();
	}

	void OnEnable() 
	{
		//2021 - locked formations should not be selectable via the ingame formation panel
		DeactivateLockedFormations();
		FetchFormations();
	}


	public void DeactivateLockedFormations()
    {
		for(int i = 0; i < availableFormationButtons.Length; i++)
        {
			if (GlobalGameSettings.instance.availableFormations[i].isUnlocked == 1 || PlayerPrefs.GetInt("shopFormation-" + i.ToString()) == 1)
			{
				print("Formation #" + i + " is available.");
				availableFormationButtons[i].GetComponent<Button>().interactable = true;
			}
			else
			{
				print("Formation #" + i + " is LOCKED.");
				availableFormationButtons[i].GetComponent<Button>().interactable = false;
			}
		}
    }


	/// <summary>
	/// Restore previous formation settings for each player upon formation change
	/// </summary>
	void FetchFormations() 
	{		
		//get current formation
		player1Formation = PlayerPrefs.GetInt("PlayerFormation");
		player2Formation = PlayerPrefs.GetInt("Player2Formation");

		print ("formationChangeRequestID: " + formationChangeRequestID);
		print ("player1FormationID: " + player1Formation + " | player2FormationID: " + player2Formation);

		if (formationChangeRequestID == 1)
			moveTickToPosition ( availableFormationButtons[player1Formation].transform.position );
		else if (formationChangeRequestID == 2)
			moveTickToPosition ( availableFormationButtons[player2Formation].transform.position );	
	}
		
	
	void Update ()
    {
		//monitor request label text
		requestByLabel.text = "(Player " + formationChangeRequestID + ")";
	}


	public void SelectNewFormation(int formationBtnID)
    {
		moveTickToPosition(availableFormationButtons[formationBtnID].transform.position);
		saveNewFormationSetting(formationBtnID);
	}


	/// <summary>
	/// move tick over the selected formation image
	/// </summary>
	/// <param name="newPos">New position.</param>
	void moveTickToPosition(Vector3 newPos) 
	{
		//unhide tick
		tickGo.GetComponent<Image>().enabled = true;

		//move tick over the selected formation image
		tickGo.transform.position = newPos + new Vector3 (0, 0, -1);
	}


	/// <summary>
	/// Saves the new formation setting for the player who requested it
	/// </summary>
	void saveNewFormationSetting(int newFormationID) 
	{
		//save
		if(formationChangeRequestID == 1)
			PlayerPrefs.SetInt("PlayerFormation", newFormationID);
		if(formationChangeRequestID == 2)
			PlayerPrefs.SetInt("Player2Formation", newFormationID);		
		
		//online
		if (gameController.isOnline)
		{
			Hashtable hashtable = new Hashtable();

			if (gameController.isServer)
			{
				hashtable.Add("serverFormation",newFormationID);
			}
			else
			{
				hashtable.Add("clientFormation",newFormationID);
			}
			
			PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
		}

		//debug
		print("New formation setting has been saved for Player " + formationChangeRequestID + " | Formation ID: " + newFormationID);
	}

}
