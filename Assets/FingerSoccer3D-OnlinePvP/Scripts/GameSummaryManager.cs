using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSummaryManager : MonoBehaviour
{
	/// <summary>
	/// This class gathers all usefull data throughout the match and shows them when the match is finished.
	/// - Notice:
	/// if applied power to ball is less than 24, we count it as a pass. Otherwise it is a shoot.
	/// if ball gets near to any gates, we count it as a shootToGoal.
	/// - Important:
	/// player-2 and opponent share some statistics, but we need to fetch their flags in separate calls.
	/// </summary>

	public Sprite[] availableFlags;			//list of all available flags

	//reference to UI objects
	public Image uiP1Flag;
	public Image uiP2Flag;
	//-
	public Text uiP1Goals;
	public Text uiP2Goals;
	//-
	public Text uiP1Shoots;
	public Text uiP2Shoots;
	//-
	public Text uiP1Pass;
	public Text uiP2Pass;
	//-
	public Text uiP1ShootsToGate;
	public Text uiP2ShootsToGate;

	private GlobalGameManager gameController;

	private void Awake()
	{
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GlobalGameManager>();
	}

	void Start () 
	{
		ShowSummary ();
	}


	void ShowSummary()
    {
		//Do not show Summary in Penalty scene
		if (SceneManager.GetActiveScene().name == "Penalty-c#")
			return;

		///flags for player-1 and (player-2 or AI)
		//P1
		uiP1Flag.sprite = availableFlags[PlayerPrefs.GetInt("PlayerFlag")];
		
		//if this is a normal match (single or two player) and not a tournament game
		if(PlayerPrefs.GetInt("IsTournament") == 0)
			uiP2Flag.sprite = availableFlags[PlayerPrefs.GetInt("Player2Flag")];
		else
			uiP2Flag.sprite = availableFlags[PlayerPrefs.GetInt("OpponentFlag")];

		if (gameController.isOnline)
		{			
			uiP1Flag.sprite = availableFlags[(int) PhotonNetwork.CurrentRoom.CustomProperties["serverFlag"]];				
			uiP2Flag.sprite = availableFlags[(int) PhotonNetwork.CurrentRoom.CustomProperties["clientFlag"]];

			var roomProp = PhotonNetwork.CurrentRoom.CustomProperties;

			if (roomProp["serverPassCount"] != null)
				uiP1Pass.text = roomProp["serverPassCount"].ToString();
			
			if(roomProp["serverShootCount"] != null)
				uiP1Shoots.text = roomProp["serverShootCount"].ToString();
			
			if (roomProp["serverShootToGate"] != null)
				uiP1ShootsToGate.text = roomProp["serverShootToGate"].ToString();

			if (roomProp["clientPassCount"] != null)
				uiP2Pass.text = roomProp["clientPassCount"].ToString();
			
			if(roomProp["clientShootCount"] != null)
				uiP2Shoots.text = roomProp["clientShootCount"].ToString();

			if (roomProp["clientShootToGate"] != null)
				uiP2ShootsToGate.text = roomProp["clientShootToGate"].ToString();			
			
			if (!gameController.isServer)
			{
				uiP1Goals.text = GlobalGameManager.opponentGoals.ToString();
				uiP2Goals.text = GlobalGameManager.playerGoals.ToString();
			}
			else
			{
				uiP1Goals.text = GlobalGameManager.playerGoals.ToString();
				uiP2Goals.text = GlobalGameManager.opponentGoals.ToString();
			}
		}
		else
		{
			//passes
			uiP1Pass.text = GlobalGameManager.playerPasses.ToString();
			uiP2Pass.text = GlobalGameManager.opponentPasses.ToString();
			
			//goals
			uiP1Goals.text = GlobalGameManager.playerGoals.ToString();
			uiP2Goals.text = GlobalGameManager.opponentGoals.ToString();
			
			//shoots
			uiP1Shoots.text = GlobalGameManager.playerShoots.ToString();
			uiP2Shoots.text = GlobalGameManager.opponentShoots.ToString();
			
			//shoots to gate
			uiP1ShootsToGate.text = GlobalGameManager.playerShootToGate.ToString();
			uiP2ShootsToGate.text = GlobalGameManager.opponentShootToGate.ToString();
		}
	}
}
