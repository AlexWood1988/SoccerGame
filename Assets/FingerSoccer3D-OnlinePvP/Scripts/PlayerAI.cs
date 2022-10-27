using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerAI : MonoBehaviour,IOnEventCallback
{
    /// <summary>
    /// Main Player AI class.
    /// why do we need a player AI class?
    /// This class has a reference to all player-1 (and player-2) units, their formation and their position
    /// and will be used to setup new formations for these units at the start of the game or when a goal happens.
    /// </summary>

    public static GameObject[] playerTeam;		//array of all player-1 units
	public static int playerFormation;			//player-1 formation
	public static int playerFlag;				//player-1 team flag

	//for two player game
	public static GameObject[] player2Team;		//array of all player-2 units
	public static int player2Formation;			//player-2 formation
	public static int player2Flag;				//player-2 team flag

	//flags
	internal bool canChangeFormation;	
	
	//arrowPlane objects
	private GameObject arrowPlane; 		//arrow plane which is used to show shotPower
	private GameObject shootCircle;		//shoot Circle plane which is used to show shotPower
	private GameObject aimArrow;		//arrow plane which is used to show aim precision (the holder)
	private GameObject aimArrowBody;	//arrow plane which is used to show aim precision
	private GameObject aimPivot;		//starting point for aim arrow
	
	//main game controller
	private GameObject gameController;	
	
	//raise event variables//
	public readonly byte showArrowCode = 1; //show helper arrow
	public readonly byte hideArrowCode = 2; //hide helper arrow
	public readonly byte shootCode = 3;


	//add raise event callback
	private void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	//remove raise event callback
	private void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}


	private void Awake()
	{
		//initializing game objects 
		gameController = GameObject.FindGameObjectWithTag("GameController");
		aimArrow = GameObject.FindGameObjectWithTag("aimArrow");	
		aimArrowBody = GameObject.FindGameObjectWithTag("aimArrowBody");
		shootCircle = GameObject.FindGameObjectWithTag("shootCircle");
		arrowPlane = GameObject.FindGameObjectWithTag("helperArrow");
		aimPivot = GameObject.FindGameObjectWithTag("aimPivot");
	}


	void init()
	{
		canChangeFormation = true;	//we just change the formation at the start of the game. No more change of formation afterwards!

		//fetch player_1's formation
		if(PlayerPrefs.HasKey("PlayerFormation"))
			playerFormation = PlayerPrefs.GetInt("PlayerFormation");
		else	
			playerFormation = 0; //Default Formation

		//fetch player_1's flag
		if(PlayerPrefs.HasKey("PlayerFlag"))
			playerFlag = PlayerPrefs.GetInt("PlayerFlag");
		else	
			playerFlag = 0; //Default team
		
		//cache all player_1 units
		playerTeam = GameObject.FindGameObjectsWithTag("Player").OrderBy(go => go.name).ToArray();
		//debug
		int i = 1;
		foreach(GameObject unit in playerTeam) 
		{
			//Optional
			unit.name = "PlayerUnit-" + i;
			unit.GetComponent<playerController>().unitIndex = i;
			//unit.GetComponent<Renderer>().material.mainTexture = availableFlags[playerFlag];
			unit.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[playerFlag].teamIcon;
			
			//2021 - set the flag on the cap object
			unit.GetComponent<playerController>().cap.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[playerFlag].teamIcon;
			//2022 - set the flag on Unit's shirt
			int modelTotalMaterials = unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials.Length;
			for (int j = 0; j < modelTotalMaterials; j++)
			{
				if (unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[j].name.Contains("dress") || unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[j].name.Contains("shirt"))
					unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[j].mainTexture = GlobalGameSettings.instance.availableTeams[playerFlag].teamIcon;
			}

			i++;
		}
		
		//if this is a 2-player local game or online game
		if(GlobalGameManager.gameMode == 1 || GlobalGameManager.gameMode == 3) 
		{

			//fetch player_2's formation
			if(PlayerPrefs.HasKey("Player2Formation"))
				player2Formation = PlayerPrefs.GetInt("Player2Formation");
			else	
				player2Formation = 0; //Default Formation
			
			//fetch player_2's flag
			if(PlayerPrefs.HasKey("Player2Flag"))
				player2Flag = PlayerPrefs.GetInt("Player2Flag");
			else	
				player2Flag = 1; //Default team

			//fetch online flag and formation if game is online mode
			if (gameController.GetComponent<GlobalGameManager>().isOnline)
			{
				foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
				{
					if (PhotonNetwork.IsMasterClient)
					{
						if (!player.IsMasterClient)
						{
							player2Flag = (int) player.CustomProperties["Flag"];
							player2Formation = (int) player.CustomProperties["Formation"];
						}
					}
					else
					{
						if (player.IsMasterClient)
						{
							player2Flag = (int) player.CustomProperties["Flag"];
							player2Formation = (int) player.CustomProperties["Formation"];
						}
					}
				}
			}

			//cache all player_2 units
			player2Team = GameObject.FindGameObjectsWithTag("Player_2").OrderBy(go => go.name).ToArray();
			int j = 1;
			foreach(GameObject unit in player2Team) 
			{
				//Optional
				unit.name = "Player2Unit-" + j;
				unit.GetComponent<playerController>().unitIndex = j;
				unit.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[player2Flag].teamIcon;

				//2021 - set the flag on the cap object
				unit.GetComponent<playerController>().cap.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[player2Flag].teamIcon;
				//2021 - set the flag on Unit's shirt
				//unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[player2Flag].teamIcon;
				//2022 - v1.1 - set the flag on Unit's shirt, but on the correct material index
				int modelTotalMaterials = unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials.Length;
				for(int k = 0; k < modelTotalMaterials; k++)
                {
					if(unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[k].name.Contains("dress") || unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[k].name.Contains("shirt"))
						unit.GetComponent<playerController>().modelRig.GetComponent<Renderer>().materials[k].mainTexture = GlobalGameSettings.instance.availableTeams[player2Flag].teamIcon;
				}

				j++;		
			}
			
			//change all player_2 unit circle color
			foreach (var unit in player2Team)
			{
				GameObject circle = unit.gameObject.transform.Find("selectionCircle").gameObject;
				circle.GetComponent<Renderer>().material.color = Color.green;
			}
		}
	}


	void Start()
	{
		//run the starting commands
		init();

		if (!GlobalGameManager.isPenaltyKick)
		{
			if (GlobalGameManager.gameMode == 3) // is online mode
			{
				if (PhotonNetwork.IsMasterClient)//master client is always in left side of field
				{
					StartCoroutine(changeFormation(playerTeam, playerFormation, 1, 1));
				}
				else
				{
					StartCoroutine(changeFormation(playerTeam, playerFormation, 1, -1));
				}
			}
			else
			{
				StartCoroutine(changeFormation(playerTeam, playerFormation, 1, 1));
			}
		}
		else
			StartCoroutine(goToPosition(playerTeam, GlobalGameManager.playerDestination, 1));

		//For two-player mode,
		if(GlobalGameManager.gameMode == 1 || GlobalGameManager.gameMode == 3)
		{

			if (!GlobalGameManager.isPenaltyKick)
			{
				if (GlobalGameManager.gameMode == 3) // is online mode
				{
					if (PhotonNetwork.IsMasterClient)
					{
						StartCoroutine(changeFormation(player2Team, player2Formation, 1, -1));
					}
					else
					{
						StartCoroutine(changeFormation(player2Team, player2Formation, 1, 1));
					}
				}
				else
				{
					StartCoroutine(changeFormation(player2Team, player2Formation, 1, -1));
				}
			}
			else
			{
				StartCoroutine(goToPosition(player2Team, GlobalGameManager.AIDestination, 1));
			}
		}
			
		canChangeFormation = false;
	}


	public void reloadPlayer2Team(int flag,int formation)
	{
		player2Flag = flag;
		player2Formation = formation;
		
		foreach(GameObject unit in player2Team) 
		{
			unit.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[player2Flag].teamIcon;
		}
		
		StartCoroutine(changeFormation(player2Team, player2Formation, 1, -1));

	}


    /// <summary>
    /// changeFormation function take all units, selected formation and side of the player (left half or right half)
    /// and then position each unit on it's destination.
    /// speed is used to fasten the translation of units to their destinations.
    /// </summary>
    /// <param name="_team"></param>
    /// <param name="_formationIndex"></param>
    /// <param name="_speed"></param>
    /// <param name="_dir"></param>
    /// <returns></returns>
    public IEnumerator changeFormation ( GameObject[] _team ,   int _formationIndex ,   float _speed ,   int _dir  )
    {
		//cache the initial position of all units
		List<Vector3> unitsSartingPosition = new List<Vector3>();
		foreach(GameObject unit in _team)
        {
			unitsSartingPosition.Add(unit.transform.position); //get the initial postion of this unit for later use.
			unit.GetComponent<MeshCollider>().enabled = false;	//no collision for this unit till we are done with re positioning.
		}
		
		float t = 0;
		while(t < 1)
        {
			t += Time.deltaTime * _speed;
			for(int cnt = 0; cnt < _team.Length; cnt++) {
				_team[cnt].transform.position = new Vector3(	 Mathf.SmoothStep(	unitsSartingPosition[cnt].x, 
                                                              	 FormationManager.getPositionInFormation(_formationIndex, cnt).x * _dir,
                                                              	 t),
				                                            	 Mathf.SmoothStep(	unitsSartingPosition[cnt].y, 
												                 FormationManager.getPositionInFormation(_formationIndex, cnt).y,
												                 t),
				                                            	 FormationManager.fixedZ );
			}	
            
			yield return 0;
		}
		
		if(t >= 1)
        {
			canChangeFormation = true;
			foreach(GameObject unit in _team)
				unit.GetComponent<MeshCollider>().enabled = true; //collision is now enabled.
		}
	}


    /// <summary>
    /// move the unit to its position
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    /// <param name="_speed"></param>
    /// <returns></returns>
    public IEnumerator goToPosition ( GameObject[] unit, Vector3 pos,   float _speed  )
    {
		//first we need to completely freeze the unit
		unit[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
		unit[0].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		print (unit[0].name + " position is fixed.");

		Vector3 unitsSartingPosition = unit[0].transform.position;
		unit[0].GetComponent<MeshCollider>().enabled = false;	//no collision for this unit till we are done with re positioning.

		float t = 0;
		while(t < 1)
        {
			t += Time.deltaTime * _speed;
			unit[0].transform.position = new Vector3( Mathf.SmoothStep(unitsSartingPosition.x, pos.x, t),
			                                   		  Mathf.SmoothStep(unitsSartingPosition.y, pos.y, t),
			                                     	  unitsSartingPosition.z );
				
			yield return 0;
		}

		if(t >= 1)
        {
			unit[0].GetComponent<MeshCollider>().enabled = true;
		}
	}
	
	public void OnEvent(EventData photonEvent)
	{
		byte eventCode = photonEvent.Code;
		if (eventCode == showArrowCode) //show arrow plane with coming position data
		{
			object[] data = (object[]) photonEvent.CustomData;

			var arrowPlane_pos = (Vector3) data[0];
			var arrowPlane_angle = (Vector3)data[1];
			var arrowPlane_scale = (Vector3)data[2];
			var circle_pos = (Vector3)data[3];
			var circle_scale = (Vector3)data[4];
			var aimArrow_angle = (Vector3)data[5];
			var aimArrow_scale = (Vector3)data[6];
			
			arrowPlane.GetComponent<Renderer>().enabled = true;
			shootCircle.GetComponent<Renderer>().enabled = true;
			aimArrowBody.GetComponent<Renderer>().enabled = true;

			arrowPlane.transform.position = arrowPlane_pos;
			arrowPlane.transform.eulerAngles = arrowPlane_angle;
			arrowPlane.transform.localScale = arrowPlane_scale;
			shootCircle.transform.position = circle_pos;
			shootCircle.transform.localScale = circle_scale;
			aimArrow.transform.position = aimPivot.transform.position;
			aimArrow.transform.eulerAngles = aimArrow_angle;
			aimArrow.transform.localScale = aimArrow_scale;
		}
		else if (eventCode == hideArrowCode) // hide arrow plane
		{
			arrowPlane.GetComponent<Renderer>().enabled = false;
			shootCircle.GetComponent<Renderer>().enabled = false;
			aimArrowBody.GetComponent<Renderer>().enabled = false;
		}
		else if (eventCode == shootCode)
		{
			object[] data = (object[]) photonEvent.CustomData;
			Vector3 force = (Vector3) data[0]; 
			string opponentName = (string) data[1];            
			GameObject ball = GameObject.Find(opponentName);
			ball.GetComponent<Rigidbody>().velocity = force;
			StartCoroutine(gameController.GetComponent<GlobalGameManager>().managePostShoot("Player_2"));	
		}
	}

	
}