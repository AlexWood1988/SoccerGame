using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class GlobalGameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    /// <summary>
    /// Main Game Controller.
    /// This class controls main aspects of the game like rounds, levels, scores and ...
    /// Please note that the game always happens between 2 player: (Player-1 vs Player-2) or (Player-1 vs AI)
    /// Player-2 and AI are the same in some aspects like when they got their turns, but they use different controllers.
    /// Player-2 uses a similar controller as Player-1, while AI uses an artificial intelligent routine to play the game.
    ///
    /// Important! All units and ball object inside the game should be fixed at Z=-0.5f positon at all times. 
    /// You can do this with RigidBody's freeze position.
    /// </summary>

    public static string player1Name = "Player_1";
	public static string player2Name = "Player_2";
	public static string cpuName = "CPU";

    public static int isLeague;     //1 means this is a single player league game
    private int leagueID;
    private int leaguePrize;
    private int leagueDuration;

    //flag to check if we are master client or not
    public bool isServer = false;

	//You are free tp change these positions at any time to customize the location of each element
	public static Vector3 penaltyKickStartPosition = new Vector3(0, -1, -0.5f);
	public static Vector3 penaltyKickGKPosition = new Vector3(13, -1, -0.5f);
	public static Vector3 penaltyKickBallPosition = new Vector3(5, -1, -0.5f);

	//Used just in penalty mode
	public static Vector3 playerDestination;	//destination for player unit after each penlaty round
	public static Vector3 AIDestination;		//destination for AI unit after each penlaty round

	// Available Game Modes:
	/*
	Indexes:
	0 = 1 player against cpu (normal or Tournament)
	1 = 2 player against each other on the same platform/device
	2 = Penalty Kicks
	3 = online
	*/
	public static int gameMode;
	public static bool isPenaltyKick;

	public bool isOnline = false; //is online mode

	//Odd rounds are player (Player-1) turn and Even rounds are AI (Player-2)'s
	public static int round;

	//in-game formation buttons (these are not needed in penalty kicks mode!)
	public GameObject p1FormationButton;
	public GameObject p2FormationButton;

	//available time to think and shoot
	public static float baseShootTime = 15;		//fixed shoot time for all players and AI

	private float p1ShootInitTime; //initial time for shooting p1
	private float p2ShootInitTime; // initial time for shooting p2
	private float p1ShootTime;					//additional time (based on the selected team) for p1
	private float p2ShootTime;					//additional time (based on the selected team) for p2 or AI
	public GameObject p1TimeBar;
	public GameObject p2TimeBar;
	private float p1TimeBarInitScale;
	private float p1TimeBarCurrentScale;
	private float p2TimeBarInitScale;
	private float p2TimeBarCurrentScale;

	//mamixmu distance that players can drag away from selected unit to shoot the ball (is in direct relation with shoot power)
	public static float maxDistance = 3.0f;

	//Turns in flags
	public static bool playersTurn;
	public static bool opponentsTurn;	

	//After players did their shoots, the round changes after this amount of time.
	public static float timeStepToAdvanceRound = 4.5f; 

	//Special occasions
	public static bool goalHappened;
	public static bool shootHappened;
	public static bool gameIsFinished;
	public static int goalLimit = 10; //To finish the game quickly, without letting the GameTime end.

	///Game timer vars
	public static float gameTimer; //in seconds
	private string remainingTime;
	private int seconds;
	private int minutes;

	//Game Status
	public static int playerGoals;
	public static int opponentGoals;
	public static float gameTime; //Main game timer (in seconds). Always fixed.

	//summary information
	public static int playerPasses;
	public static int playerShoots;
	public static int playerShootToGate;
	public static int opponentPasses;
	public static int opponentShoots;
	public static int opponentShootToGate;

	//gameObject references
	private GameObject playerAIController;
	private GameObject opponentAIController;
	public GameObject goalPlane;
    private GameObject pauseBtn;

	private GameObject ball;
	private Vector3 ballStartingPosition;

	//AudioClips
	public AudioClip startWistle;
	public AudioClip finishWistle;
	public AudioClip[] goalSfx;
	public AudioClip[] goalHappenedSfx;
	public AudioClip[] crowdChants;
	private bool canPlayCrowdChants;
    private AudioSource aso;

	//Public references
	public GameObject gameStatusPlane;			//user to show win/lose result at the end of match
    public GameObject disconnectedPlane;        //plane we use to show user that connection has lost
    public GameObject continueTournamentBtn;	//special tournament button to advance in tournament incase of win
	public GameObject summaryBtn;	            //...
	public GameObject menuBtn;					//...
	public Image statusTextureObject;		//plane we use to show the result texture in 3d world
	public Sprite[] statusModes;					//Available status textures

	//flag to check if game is ready to play
	private bool isGameReady = true;	
	public bool myShootTurn;
	private byte postShoot_evCode = 6;
	private byte turnChange_evCode = 7;

    //private bool playerDisconnected ; //flag to check if other player disconnected from game or left the room
    private bool opponentDisconnected; //flag to check if other player disconnected from game or left the room
    private bool disconnectedPhoton = false; //flag to check if we disconnected from photon server

    private bool isFakeOnlineMode;      //we use this flag to set correct settings for the game if a human player is not found after some time 
                                        //and game is using a fake bot to simulate an online match.


    [Header("Light Settings")]
    public GameObject[] realtimeLights;
    public static bool enableLights;
    public Button lightButtonUI;
    public Image lightButtonIconUI;

	//Update v1.1 - commentator
	private GameObject commentatorManager;


	public override void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}


    void Awake ()
    {
		//debug
		//PlayerPrefs.DeleteAll();

		commentatorManager = GameObject.FindGameObjectWithTag("CommentatorManager");

		opponentDisconnected = false;

        //init
        goalHappened = false;
		shootHappened = false;
		gameIsFinished = false;
		round = 1;
		playerGoals = 0;
		opponentGoals = 0;
		gameTime = 0;
		seconds = 0;
		minutes = 0;
		canPlayCrowdChants = true;
        aso = GetComponent<AudioSource>();

        //Update light status
        UpdateLightStatus();

        //Set your default names here.
        player1Name = "Player_1";
        player2Name = "Player_2";
        cpuName = "CPU";

        //reset summary info
        playerPasses = 0;
		playerShoots = 0;
		playerShootToGate = 0;
		opponentPasses = 0;
		opponentShoots = 0;
		opponentShootToGate = 0;

        //show league rules on UI (in single-player mode)
        isLeague = PlayerPrefs.GetInt("IsLeague");
        if (isLeague == 1)
        {
            print("This is a single-player league game...");
            leagueID = PlayerPrefs.GetInt("LeagueID");
            leaguePrize = PlayerPrefs.GetInt("LeaguePrize");
            leagueDuration = PlayerPrefs.GetInt("LeagueDuration");
        }

        //Check if this is a penalty game or a normal match
        isPenaltyKick = (PlayerPrefs.GetInt("IsPenalty") == 1) ? true : false;

		//To avoid null reference errors caused by running the penalty scene without opening it from main menu,
		//we need to add the following lines. You should remove this in your live game.
		if(SceneManager.GetActiveScene().name == "Penalty-c#") //just to avoid null errors
			isPenaltyKick = true;

		setDestinationForPenaltyMode();	//init the positions

		//hide gameStatusPlane
		gameStatusPlane.SetActive(false);
		continueTournamentBtn.SetActive(false);

        if(summaryBtn)
			summaryBtn.SetActive(false);
		
		//Translate gameTimer index to actual seconds
		switch(PlayerPrefs.GetInt("GameTime"))
        {
			case 0:
				gameTimer = GlobalGameSettings.instance.availableTimes[0].timeValue;
				break;
			case 1:
				gameTimer = GlobalGameSettings.instance.availableTimes[1].timeValue;
				break;
			case 2:
				gameTimer = GlobalGameSettings.instance.availableTimes[2].timeValue;
				break;
			
			//You can add more cases and options here.
		}

        //if this is a single-player league match, we get the time from playerprefs
        if (PlayerPrefs.GetInt("IsLeague") == 1)
            gameTimer = leagueDuration;

        //get additonal time for each player and AI
        p1ShootTime = baseShootTime + GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("PlayerFlag")].teamAttributes.y;
		p2ShootTime = baseShootTime + GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("Player2Flag")].teamAttributes.y;
		//print ("P1 shoot time: " + p1ShootTime + " // " + "P2 shoot time: " + p2ShootTime);

		//fill player shoot timer to full (only in normal game mode, where these objects are available)
		if(!isPenaltyKick) 
		{
			p1TimeBarInitScale = p1TimeBar.GetComponent<Image>().fillAmount;
			p1TimeBarCurrentScale = p1TimeBar.GetComponent<Image>().fillAmount;

			p2TimeBarInitScale = p2TimeBar.GetComponent<Image>().fillAmount;
			p2TimeBarCurrentScale = p2TimeBar.GetComponent<Image>().fillAmount;

			p1TimeBar.GetComponent<Image>().fillAmount = 1f; //full
			p2TimeBar.GetComponent<Image>().fillAmount = 1f; //full
		}		
		
		//Get Game Mode
		if(PlayerPrefs.HasKey("GameMode"))
			gameMode = PlayerPrefs.GetInt("GameMode");
		else
			gameMode = 0; // Deafault Mode (Player-1 vs AI)
		
		playerAIController = GameObject.FindGameObjectWithTag("playerAI");
		opponentAIController = GameObject.FindGameObjectWithTag("opponentAI");

		ball = GameObject.FindGameObjectWithTag("ball");
		ballStartingPosition = new Vector3(0, -0.81f, -0.7f);	//for normal play mode
		
		manageGameModes();

        //New setting (update v1.1)
        //Check it this is "Fake Online" mode
        isFakeOnlineMode = PlayerPrefs.GetInt("IsFakeOnlineWithBot", 0) == 1 ? true : false;
        //print("Are we using fake online mode: " + isFakeOnlineMode);

        //Special timer for online mode (& fake online mode)
        if (isOnline || isFakeOnlineMode)
        {
            gameTimer = GlobalGameSettings.instance.onlineModeTimer;
            print("Overriding timer for online match: @" + gameTimer + "seconds.");
        }

        //Use only for debug
        //gameTimer = 5;

        //disable pause button in online mode
        pauseBtn = GameObject.Find("PauseBtn");
        if (isOnline)
        {
            pauseBtn.GetComponent<Button>().interactable = false;
        }

        // init players shoot time by online flag information
        if (isOnline)
		{
			if (isServer)
			{
                p1ShootTime = baseShootTime + GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("PlayerFlag")].teamAttributes.y ; //left player is player1
            	foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            	{
            		if (!player.IsMasterClient)
            		{
						p2ShootTime = baseShootTime + GlobalGameSettings.instance.availableTeams[(int)player.CustomProperties["Flag"]].teamAttributes.y;
            		}
            	}
			}
			else
			{
            	p2ShootTime = baseShootTime + GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("PlayerFlag")].teamAttributes.y; //right player is player1
            	foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            	{
            		if (player.IsMasterClient)
            		{
						p1ShootTime = baseShootTime + GlobalGameSettings.instance.availableTeams[(int)player.CustomProperties["Flag"]].teamAttributes.y;
            		}
            	}            				
			}
		}

		//set initial value of shooting time
		p1ShootInitTime = p1ShootTime;
		p2ShootInitTime = p2ShootTime;
		
		storeGameTimer();

		if (gameMode == 3) // is online game	(set players name)
		{
			if (isServer)
			{
				myShootTurn = true;
				player1Name = PhotonNetwork.NickName;
				foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
				{
					if (!player.IsMasterClient)
					{
						player2Name = player.NickName;
					}
				}
			}
			else
			{
				myShootTurn = false;
				player2Name = PhotonNetwork.NickName;
				foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
				{
					if (player.IsMasterClient)
					{
						player1Name = player.NickName;
					}
				}
			}
		}

        //using fake settings for P1 & p2 (bot) if fake online mode (play with bot) is enabled.
        if(isFakeOnlineMode)
        {
            //Names
            player1Name = PlayerPrefs.GetString("FakeP1_Name", "Player_XXX");
            player2Name = PlayerPrefs.GetString("FakeBot_Name", "FAKE_BOT");
            cpuName = PlayerPrefs.GetString("FakeBot_Name", "FAKE_BOT");
        }

        //instruction for crazy mode
        if (PlayerPrefs.GetInt("IsCrazyMode") == 1)
        {
            print("This is a CrazyMode game!");
            timeStepToAdvanceRound = 1f;
        }
    }


    public void UpdateLightStatus()
    {
        if(PlayerPrefs.GetInt("EnableLights", 1) == 1)
        {
            ToggleLightStatus(true);
            print("Lights ON");
        }
        else
        {
            ToggleLightStatus(false);
            print("Lights OFF");
        }
    }

    public void ToggleLight()
    {
        enableLights = !enableLights;
        ToggleLightStatus(enableLights);
    }

    private void ToggleLightStatus(bool status = true)
    {
        if (status)
        {
            enableLights = true;
            PlayerPrefs.SetInt("EnableLights", 1);
            foreach(GameObject g in realtimeLights)
            {
                g.GetComponent<Light>().enabled = true;
            }

			if(lightButtonUI)
				lightButtonUI.GetComponent<Image>().color = new Color(0, 1, 0);
        }
        else
        {
            enableLights = false;
            PlayerPrefs.SetInt("EnableLights", 0);
            foreach (GameObject g in realtimeLights)
            {
                g.GetComponent<Light>().enabled = false;
            }

			if(lightButtonUI)
				lightButtonUI.GetComponent<Image>().color = new Color(1, 1, 1);
        }

        print("EnableLights: " + enableLights);
    }


    /// <summary>
    /// We have all units inside the game scene by default, but at the start of the game,
    /// we check which side in playing (should be active) and deactive the side that is
    /// not playing by deactivating all it's units.
    /// </summary>
    private GameObject[] player2Team;	//array of all player-2 units in the game
	private GameObject[] cpuTeam;		//array of all AI units in the game
	void manageGameModes ()
	{
		if (gameMode == 0)
		{
			//find and deactive all player2 units. This is player-1 vs AI.
			player2Team = GameObject.FindGameObjectsWithTag("Player_2");
			foreach (GameObject unit in player2Team)
			{
				unit.SetActive(false);
			}

			if (!isPenaltyKick)
			{
				//also deactivate p2FormationButton as there is no player-2 in the game
				p2FormationButton.SetActive(false);
			}
			
		}
		else if (gameMode == 1 || gameMode == 3)
		{
			//find and deactive all AI Opponent units. This is Player-1 vs Player-2. or online match
			cpuTeam = GameObject.FindGameObjectsWithTag("Opponent");
			foreach (GameObject unit in cpuTeam)
			{
				unit.SetActive(false);
			}

			//deactive opponent's AI
			opponentAIController.SetActive(false);
		}

		if (gameMode == 3)	//if it's online match so game not ready until next player join
		{
			isOnline = true;
			isGameReady = false;
			
			if (PhotonNetwork.IsMasterClient) //set first turn for master client 
			{
				isServer = true;
				round = 1;
			}
			else
			{
				round = 2;
			}
		}
	}

	void Start ()
	{
		StartCoroutine(startGame());
	}

	private IEnumerator startGame()
	{
		isGameReady = true; //set game ready to play status to true

		//roundTurnManager(false);
		roundTurnManager();

		yield return new WaitForSeconds(1.5f);
		playSfx(startWistle);

		//Update v1.1
		commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("MatchStart", 0);
	}
	

	void Update ()
	{
		//check game finish status every frame
		if(!gameIsFinished) 
		{
			manageGameStatus();

			//fill time limit bars, only in normal game mode
			if(!isPenaltyKick && isGameReady)
			{
				updateTimeBars();
			}
		}
		
		//every now and then, play some crowd chants
		StartCoroutine(playCrowdChants());

		//If you ever needed debug inforamtions:
		//print("GameRound: " + round + " & turn is for: " + whosTurn + " and GoalHappened is: " + goalHappened);

		//Update v1.1 - play random commentator sounds
		if (CommentatorManager.canPlayClip && Random.value <= 0.005f && opponentsTurn)
		{
			if (isPenaltyKick)
				commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("GameGoalkeeper", 1);
            else
            {
				//if game is just started
				if(gameTimer > 30f)
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("GameStatus", 0);
				else
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("TimeLow", 0);
			}				
		}
	}


	//determine the position of each side on the field for penalty mode
	void setDestinationForPenaltyMode()
    {
		if(round % 2 == 0) {
			playerDestination = penaltyKickGKPosition;
			AIDestination = penaltyKickStartPosition;
		} else {
			playerDestination = penaltyKickStartPosition;
			AIDestination = penaltyKickGKPosition;
		}

        //debug
        //Debug.Log("playerDestination: " + playerDestination + " - & AIDestination: " + AIDestination);
    }

	/// <summary>
    /// this function give turn to players in the game
	/// store_turn param : if true -> store turn in game properties
    /// </summary>
	void roundTurnManager ()
	{		
		if(gameIsFinished || goalHappened)
			return;

		//reset shootHappened flag
		shootHappened = false;

		//fill time limit bars, only in normal game mode
		if(!isPenaltyKick) {
			fillTimeBar(1);
			fillTimeBar(2);
		}				

		//if round number is odd, it's players turn, else it's AI or player-2 's turn
		int carry;
		carry = round % 2;

		if(carry == 1) 
		{
			playersTurn = true;
			opponentsTurn = false;
			playerController.canShoot = true;
			OpponentAI.opponentCanShoot = false;
			myShootTurn = true;
		} 
		else
		{
			playersTurn = false;
			opponentsTurn = true;
			playerController.canShoot = false;
			OpponentAI.opponentCanShoot = true;
			myShootTurn = false;
		}

		//Override
		//for two player game, players can always shoot.
		//we override this because both human players play on the same device and must be able to shoot at every turn.
		//we just limit their actions to their own units.
		if(gameMode == 1)
			playerController.canShoot = true;

        if (isOnline)
        {
            setRoundChanged();
        }

        //new update v1.2
        //testing a crazy idea - all units can shoot at the same time
        if (PlayerPrefs.GetInt("IsCrazyMode") == 1)
        {
            playersTurn = true;
            opponentsTurn = true;
            playerController.canShoot = true;
            OpponentAI.opponentCanShoot = true;
        }
    }


    /// <summary>
    ///  Update timebars by changing their scale (x) over time.
    /// If time ends, the turn will be transforred to opponent.
    /// </summary>
    void updateTimeBars() {

		if(gameIsFinished || goalHappened || shootHappened)
			return;

        //limiters and turn change
        //Also change turns incase of time running out!
        if (isOnline && !isServer)
        {
            if (p1TimeBarCurrentScale <= 0)
            {
                p1TimeBarCurrentScale = 0;
                setNewRound(2);
                return;
            }
            if (p2TimeBarCurrentScale <= 0)
            {
                p2TimeBarCurrentScale = 0;
                setNewRound(1);
                return;
            }
        }
        else
        {
            if (p1TimeBarCurrentScale <= 0)
            {
                p1TimeBarCurrentScale = 0;
                setNewRound(1);
                return;
            }
            if (p2TimeBarCurrentScale <= 0)
            {
                p2TimeBarCurrentScale = 0;
                setNewRound(2);
                return;
            }
        }

        if (!isOnline)
		{
			if(playersTurn)
			{
				drainTimeBar(true);
			}
			else {
				drainTimeBar(false);
			}
		}
		else //in online mode
		{
			if (isServer)
			{
				if (playersTurn)
				{
					drainTimeBar(true);
				}
				else
				{
					drainTimeBar(false);
				}
			}
			else
			{
				if (playersTurn)
				{
					drainTimeBar(false);
				}
				else
				{
					drainTimeBar(true);
				}
			}
		}
	}

	private void drainTimeBar(bool isPlayer1)
	{
		if (isPlayer1)
		{
			p1TimeBarCurrentScale -= Time.deltaTime / p1ShootTime;
			//p1TimeBar.transform.localScale = new Vector3(p1TimeBarCurrentScale, p1TimeBar.transform.localScale.y, p1TimeBar.transform.localScale.z);
			p1TimeBar.GetComponent<Image>().fillAmount = p1TimeBarCurrentScale;
			fillTimeBar(2);
		}
		else
		{
			p2TimeBarCurrentScale -= Time.deltaTime / p2ShootTime;
			//p2TimeBar.transform.localScale = new Vector3(p2TimeBarCurrentScale, p2TimeBar.transform.localScale.y, p2TimeBar.transform.localScale.z);
			p2TimeBar.GetComponent<Image>().fillAmount = p2TimeBarCurrentScale;
			fillTimeBar(1);
		}
	}


	void fillTimeBar(int _ID)
    {
		if(_ID == 1)
		{
			p1TimeBarCurrentScale = p1TimeBarInitScale;
			//p1TimeBar.transform.localScale = new Vector3(1,1,1);
			p1TimeBar.GetComponent<Image>().fillAmount = 1f;

			if (isOnline)
			{
				p1ShootTime = p1ShootInitTime;
			}
		} 
		else 
		{
			p2TimeBarCurrentScale = p2TimeBarInitScale;
			//p2TimeBar.transform.localScale = new Vector3(1,1,1);
			p2TimeBar.GetComponent<Image>().fillAmount = 1f;

			if (isOnline)
			{
				p2ShootTime = p2ShootInitTime;
			}
		}
	}


	void setNewRound(int _ID) 
	{
		switch(_ID)
		{
		case 1:
			round = 2;
			break;		
		case 2:
			round = 1;
			break;	
		}	
		
		roundTurnManager();
	}


    /// <summary>
    /// What happens after a shoot is performed?
    /// </summary>
    /// <param name="_shootBy"></param>
    /// <returns></returns>
    public IEnumerator managePostShoot ( string _shootBy  )
	{
		Debug.Log("shoot by : " + _shootBy);

		shootHappened = true;

		//get who is did the shoot
		//if we had a goal after the shoot was done and just before the round change, leave the process to other controllers.
		float t = 0;
		while(t < timeStepToAdvanceRound) 
		{	
			t += Time.deltaTime;
			if(goalHappened)
            {
				yield break;
			} 		
			yield return 0;
		}
		
		//we had a simple shoot with no goal result
		if(t >= timeStepToAdvanceRound)
		{
			//add to round counters
			switch(_shootBy)
			{
				case "Player":
					round = 2;
					break;		
				case "Player_2":
					round = 1;
					break;	
				case "Opponent":
					round = 1;
					break;
			}	

			//*** reformation of units ONLY for PENALTY mode ***//
			if(isPenaltyKick)
            {
				StartCoroutine(GetComponent<PenaltyController>().updateResultArray(_shootBy, 0));

				//update positions for penalty mode
				setDestinationForPenaltyMode();

				//Reformation for player_1
				StartCoroutine(playerAIController.GetComponent<PlayerAI>().goToPosition(PlayerAI.playerTeam, playerDestination, 1));

				//if this is player-1 vs player-2 match:
				if(GlobalGameManager.gameMode == 1) {
					//2-players penalty is not implemented yet
					//...
				} else {	
					//if this is player-1 vs AI match:
					StartCoroutine(opponentAIController.GetComponent<OpponentAI>().goToPosition(OpponentAI.myTeam, AIDestination, 1));
				}

				//bring the ball back to it's initial position
				ball.GetComponent<TrailRenderer>().enabled = false;
				ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
				ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				
				ball.transform.position = penaltyKickBallPosition;	//GO TO PENALTY POSITION!
			}
			
			//let the units get to their positions
			yield return new WaitForSeconds(0.5f);

			roundTurnManager(); //cycle again between players
		}
	}

    /// <summary>
    /// If we had a goal in this round, this is the function that manages all aspects of it.
    /// </summary>
    /// <param name="_goalBy"></param>
    /// <returns></returns>
    public IEnumerator managePostGoal ( string _goalBy  )
    {
		//avoid counting a goal twice (or more!)
		if(goalHappened)
			yield break;
		
		//soft pause the game for reformation and other things...
		goalHappened = true;
		shootHappened = false;
		
		//add to goal counters
		switch(_goalBy)
		{
			case "Player":
				playerGoals++;
				Debug.Log("player goal added");
				round = 2; //goal by player-1 and opponent should start the next round
				break;
			case "Opponent":
				opponentGoals++;
				Debug.Log("opponent goal added");
				round = 1; //goal by opponent and player-1 should start the next round
				break;
		}

        //2021 - shake the camera
        CameraShake.instance.PublicShake(0.3f, 1.5f);

        //wait a few seconds to show the effects , and physics cooldown
        GetComponent<AudioSource>().PlayOneShot(goalSfx[Random.Range(0, goalSfx.Length)], 1);
        GetComponent<AudioSource>().PlayOneShot(goalHappenedSfx[Random.Range(0, goalHappenedSfx.Length)], 1);

		//Update v1.1
		commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("Goal", 0);

		//yield return new WaitForSeconds(1);

		//activate the goal event plane
		GameObject gp = null;
        //gp = Instantiate (goalPlane, new Vector3(30, -4.5f, -7.5f), Quaternion.Euler(45, 180, 0)) as GameObject;
        gp = Instantiate (goalPlane, new Vector3(30, -6.5f, -5f), Quaternion.Euler(0, 0, 0)) as GameObject;

		float t = 0;
		float speed = 2.0f;
		while(t < 1)
        {
			t += Time.deltaTime * speed;
			gp.transform.position = new Vector3(Mathf.SmoothStep(30, 0, t), gp.transform.position.y, gp.transform.position.z);
			yield return 0;
		}

		yield return new WaitForSeconds(0.75f);

		float t2 = 0;
		while(t2 < 1)
        {
			t2 += Time.deltaTime * speed;
			gp.transform.position = new Vector3(Mathf.SmoothStep(0, -30, t2), gp.transform.position.y, gp.transform.position.z);
			yield return 0;
		}

		Destroy(gp, 1.5f);

		//update positions for penalty mode
		if(isPenaltyKick) {
			setDestinationForPenaltyMode();
			StartCoroutine(GetComponent<PenaltyController>().updateResultArray(_goalBy, 1));
		}
		
		//*** reformation of units ***//
		//Reformation for player_1
		if (!isPenaltyKick)
		{
			if (isOnline)
			{
				if (isServer)
				{
					StartCoroutine(playerAIController.GetComponent<PlayerAI>().changeFormation(PlayerAI.playerTeam,
						(int)PhotonNetwork.CurrentRoom.CustomProperties["serverFormation"]
                    				, 0.6f, 1));
				}
				else
				{
					StartCoroutine(playerAIController.GetComponent<PlayerAI>().changeFormation(PlayerAI.playerTeam,
						(int)PhotonNetwork.CurrentRoom.CustomProperties["clientFormation"], 0.6f, -1));
				}
			}
			else
			{
				StartCoroutine(playerAIController.GetComponent<PlayerAI>().changeFormation(PlayerAI.playerTeam,
					PlayerPrefs.GetInt("PlayerFormation"), 0.6f, 1));
			}
		}
		else
			StartCoroutine(playerAIController.GetComponent<PlayerAI>().goToPosition(PlayerAI.playerTeam, playerDestination, 1));
		
		//if this is player-1 vs player-2 match:
		if(GlobalGameManager.gameMode == 1)
		{
			StartCoroutine(playerAIController.GetComponent<PlayerAI>().changeFormation(PlayerAI.player2Team, PlayerPrefs.GetInt("Player2Formation"), 0.6f, -1));
		}
		//if this is online match
		else if (isOnline)
		{
			if (isServer)
			{
				StartCoroutine(playerAIController.GetComponent<PlayerAI>()
					.changeFormation(PlayerAI.player2Team, 
						(int)PhotonNetwork.CurrentRoom.CustomProperties["clientFormation"],
						0.6f, -1));
			}
			else
			{
				StartCoroutine(playerAIController.GetComponent<PlayerAI>()
					.changeFormation(PlayerAI.player2Team, (int)PhotonNetwork.CurrentRoom.CustomProperties["serverFormation"], 0.6f, 1));
			}
		}
		else
        {	//if this is player-1 vs AI match:
			if(!isPenaltyKick)
            {
				//get a new random formation everytime
				StartCoroutine(opponentAIController.GetComponent<OpponentAI>().changeFormation(Random.Range(0, GlobalGameSettings.instance.availableFormations.Length), 0.6f));
			}
            else
            {
				//go to correct penalty position
				StartCoroutine(opponentAIController.GetComponent<OpponentAI>().goToPosition(OpponentAI.myTeam, AIDestination, 1));
			}
		}

		//bring the ball back to it's initial position
		ball.GetComponent<TrailRenderer>().enabled = false;
		ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
		ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

		if(!isPenaltyKick)
			ball.transform.position = ballStartingPosition;		//go to the center of field
		else
			ball.transform.position = penaltyKickBallPosition;	//GO TO PENALTY POSITION!

		yield return new WaitForSeconds(1);
		ball.GetComponent<TrailRenderer>().enabled = true;
		
		yield return new WaitForSeconds(2);

		//check if the game is finished or not
		if(playerGoals > goalLimit || opponentGoals > goalLimit)
        {
			gameIsFinished = true;
			manageGameFinishState();
			yield break;
		} 
		
		//else, continue to the next round
		goalHappened = false;

		//roundTurnManager(true);
		roundTurnManager();

		playSfx(startWistle);
	}


    [Header("Time Settings")]
	public Text timeText;				
	public Text playerGoalsText;		
	public Text opponentGoalsText;	
	public Text playerOneName;		
	public Text playerTwoName;		
	void manageGameStatus ()
	{
		if (isGameReady)
		{
			gameTimer -= Time.deltaTime;
			seconds = Mathf.CeilToInt(gameTimer) % 60;
			minutes = Mathf.CeilToInt(gameTimer) / 60;
		}

		//We do not need time in penalty mode, so
		if(isPenaltyKick) {
			seconds = 0;
			minutes = 90;
		}
		
		if(seconds == 0 && minutes == 0 && isGameReady) 
		{
			gameIsFinished = true;
			manageGameFinishState();
		}
		
		remainingTime = string.Format("{0:00} : {1:00}", minutes, seconds); 
		timeText.text = remainingTime.ToString();

		if (isOnline && !isServer)
		{
			playerGoalsText.text = opponentGoals.ToString();
			opponentGoalsText.text = playerGoals.ToString();
		}
		else
		{
			playerGoalsText.text = playerGoals.ToString();
			opponentGoalsText.text = opponentGoals.ToString();
		}
		

		if(gameMode == 0)
        {
			playerOneName.text = player1Name;
			playerTwoName.text = cpuName;
		}
        else if(gameMode == 1 || gameMode == 3)
		{
			playerOneName.text = player1Name;
			playerTwoName.text = player2Name;
		} 
	}


    /// <summary>
    /// After the game is finished, this function handles the events.
    /// </summary>
    public void manageGameFinishState ()
    {
        if (isOnline && disconnectedPhoton)
        {
            disconnectedPlane.SetActive(true);
        }
        else
        {
            //Play gameFinish wistle
            aso.PlayOneShot(finishWistle);
            print("GAME IS FINISHED.");

            //show gameStatusPlane
            gameStatusPlane.SetActive(true);

            //for single player game, we should give the player some bonuses in case of winning the match
            if (gameMode == 0 || isOnline)
            {
                if (isOnline) // disable restart btn in online mode & disconnent from the network
                {
                    GameObject restartBtn = GameObject.Find("RestartBtn");
                    restartBtn.SetActive(false);

                    Debug.Log("player goals : " + playerGoals);
                    Debug.Log("opponent goals: " + opponentGoals);

                    summaryBtn.SetActive(true);
                    menuBtn.SetActive(false);
                    PhotonNetwork.Disconnect();
                }

                if (playerGoals > goalLimit || playerGoals > opponentGoals || isOnline && opponentDisconnected)
                {
                    print("Player 1 is the winner!!");

                    //set the result texture
                    //statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[0];
                    statusTextureObject.sprite = statusModes[0];

                    int playerWins = PlayerPrefs.GetInt("PlayerWins");
                    int playerMoney = PlayerPrefs.GetInt("PlayerMoney");

                    PlayerPrefs.SetInt("PlayerWins", ++playerWins);         //add to wins counter

                    //if this is a tournament match, update it with win state and advance.
                    if (PlayerPrefs.GetInt("IsTournament") == 1)
                    {
                        PlayerPrefs.SetInt("TorunamentMatchResult", 1);
                        PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
                        continueTournamentBtn.SetActive(true);
                    }

                    //if this is a single-player league match, we need to grant the special coin prize!
                    if (PlayerPrefs.GetInt("IsLeague") == 1)
                    {
                        print("LeaguePrize: " + leaguePrize);
                        playerMoney += leaguePrize;
                        PlayerPrefs.SetInt("PlayerMoney", playerMoney); //save league prize!
                    }
                    else
                    {
                        //grant a normal prize for player wins in other modes
                        PlayerPrefs.SetInt("PlayerMoney", playerMoney + 100);   //handful of coins as the prize!
                    }
                }
                else if (opponentGoals > goalLimit || opponentGoals > playerGoals)
                {
                    print("CPU is the winner!!");
                    //statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[1];
					statusTextureObject.sprite = statusModes[1];

					//if this is a tournament match, update it with lose state.
					if (PlayerPrefs.GetInt("IsTournament") == 1)
                    {
                        PlayerPrefs.SetInt("TorunamentMatchResult", 0);
                        PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
                        continueTournamentBtn.SetActive(true);
                    }
                }
                else if (opponentGoals == playerGoals)
                {
                    print("(Single Player) We have a Draw!");
                    //statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[4];
					statusTextureObject.sprite = statusModes[4];

					//Update v1.1
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("MatchTie", 0);

					//we count "draw" a lose in tournament mode.
					//if this is a tournament match, update it with lose state.
					if (PlayerPrefs.GetInt("IsTournament") == 1)
                    {
                        PlayerPrefs.SetInt("TorunamentMatchResult", 0);
                        PlayerPrefs.SetInt("TorunamentLevel", PlayerPrefs.GetInt("TorunamentLevel", 0) + 1);
                        continueTournamentBtn.SetActive(true);
                    }
                }
            }
            else if (gameMode == 1)
            {
                if (playerGoals > opponentGoals)
                {
                    print("Player 1 is the winner!!");
                    //statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[2];
					statusTextureObject.sprite = statusModes[2];
				}
                else if (playerGoals == opponentGoals)
                {
                    print("(Two-Player) We have a Draw!");
                    //statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[4];
					statusTextureObject.sprite = statusModes[4];

					//Update v1.1
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("MatchTie", 0);
				}
                else if (playerGoals < opponentGoals)
                {
                    print("Player 2 is the winner!!");
                    //statusTextureObject.GetComponent<Renderer>().material.mainTexture = statusModes[3];
					statusTextureObject.sprite = statusModes[3];
				}
            }
        }
	}


    /// <summary>
    /// Play a random crown sfx every now and then to spice up the game
    /// </summary>
    /// <returns></returns>
    IEnumerator playCrowdChants ()
    {
		if(canPlayCrowdChants) {
			canPlayCrowdChants = false;
			GetComponent<AudioSource>().PlayOneShot(crowdChants[Random.Range(0, crowdChants.Length)], 1);
			yield return new WaitForSeconds(Random.Range(15, 35));
			canPlayCrowdChants = true;
		}
	}


    /// <summary>
    /// Play sound clips
    /// </summary>
    /// <param name="_clip"></param>
    void playSfx(AudioClip _clip)
	{
		GetComponent<AudioSource>().clip = _clip;
		if (!GetComponent<AudioSource>().isPlaying)
		{
			GetComponent<AudioSource>().Play();
		}
	}


    /// <summary>
    /// Enable crazy move
    /// </summary>
    /// <param name="_delay"></param>
    /// <returns></returns>
    public IEnumerator EnableCrazyMoveForAI(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        OpponentAI.canPerformCrazyMove = true;
        print("<b>AI can perform crazy move again @" + Time.timeSinceLevelLoad + "</b>");
    }


    /// <summary>
    /// Post shoot event
    /// </summary>
    /// <param name="shoot_by"></param>
    public void sendPostShootEvent(string shoot_by)
    {
        var evCode = postShoot_evCode;
        Debug.Log("sending post shoot event by : " + shoot_by);

        object[] content = new object[]
        {
            shoot_by
        };

        RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(evCode, content, eventOptions, sendOptions);
    }


    private void setRoundChanged()
    {
        var evCode = turnChange_evCode;

        Debug.Log("sending round changed");

        object[] content = new object[]
        {
            myShootTurn
        };

        RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(evCode, content, eventOptions, sendOptions);
    }

    /// <summary>
    /// OnEvent logic
    /// </summary>
    /// <param name="photonEvent"></param>
    public void OnEvent(EventData photonEvent)
    {
        var evCode = photonEvent.Code;

        if (evCode == postShoot_evCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("received post shoot event by : " + data[0]);

            //run post shoot coroutine
            StartCoroutine(managePostShoot((string)data[0]));
        }
        else if (evCode == turnChange_evCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            bool opponentTurn = (bool) data[0];

            if (opponentTurn && myShootTurn)
            {
                setNewRound(1);
            }
            else if (!opponentTurn && !myShootTurn)
            {
                setNewRound(2);
            }

            /*Debug.Log("received turn change event by round =  " + data[0]);
            var mRound = (int)data[0];
            //changing round
            if (mRound == 1)
            {
                if (round != 1)
                {
                    setNewRound(2);
                }
            }
            else
            {
                if (round != 2)
                {
                    setNewRound(1);
                }
            }*/
        }
    }
    //End onEvent


    /// <summary>
    /// store start game time and gametimer value to the room properties 
    /// </summary>
    private void storeGameTimer()
	{
		if (isOnline && isServer)
		{
			//save timer to photon room properties
			Hashtable hashtable = new Hashtable();
			hashtable.Add("gameTimer",gameTimer);
			hashtable.Add("storeTime",PhotonNetwork.Time);	
			PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
		}
	}

    /// <summary>
    /// update my turn status in room properties in online mode
    /// </summary>
    /// <param name="propertiesThatChanged"></param>
    /*private void storeTurnInfo(bool myTurn)
	{
		Hashtable hashtable = new Hashtable();
		if (isServer)
		{
			hashtable.Add("serverTurn",myTurn);
			hashtable.Add("serverTurnStartTime",PhotonNetwork.Time);
		}
		else
		{
			hashtable.Add("clientTurn",myTurn);
			hashtable.Add("clientTurnStartTime",PhotonNetwork.Time);
		}
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
	}*/

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		if (!isServer)
		{
			if (propertiesThatChanged["gameTimer"] != null)
			{
				var timer = (float)propertiesThatChanged["gameTimer"];// timer value of master client
				var serverTime = (double)propertiesThatChanged["storeTime"];// master client start game time
				var myTime = PhotonNetwork.Time; // other client start game time
			

				var timeDiff = myTime - serverTime; // time difference between master client start game time and other client in miliseconds

				gameTimer = (float) (timer - timeDiff);// subtract time diff from master client timer value and set it to the this client game timer variable
			}
		}

		//fetch players new formation
		if (propertiesThatChanged["serverFormation"] != null )
		{
			if (isServer)
			{
				PlayerAI.playerFormation = (int) propertiesThatChanged["serverFormation"];
			}
			else
			{
				PlayerAI.player2Formation = (int) propertiesThatChanged["serverFormation"];
			}
		}
		if (propertiesThatChanged["clientFormation"] != null)
		{
			if (isServer)
			{
				PlayerAI.player2Formation = (int) propertiesThatChanged["clientFormation"];
			}
			else
			{
				PlayerAI.playerFormation = (int) propertiesThatChanged["clientFormation"];
			}
		}		
		//end fetch players new formation
	}


	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		//other player disconnected 
		Debug.Log("other player disconnected");
        opponentDisconnected = true;

        if (!gameIsFinished)
        {
            gameTimer = 0;
        }
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from photon");
        disconnectedPhoton = true;
        gameTimer = 0;
    }


	/// <summary>
	/// UI Commands
	/// </summary> 
	public void LoadMenu()
    {
        PauseManager.instance.UnPauseGame();
		SceneManager.LoadScene("Menu-c#");
    }

	public void RestartGame()
	{
        PauseManager.instance.UnPauseGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void PauseGame()
	{
		PauseManager.instance.ClickOnPauseBtn();
	}


	public void ResumeGame()
	{
        PauseManager.instance.ClickOnPauseBtn();
	}

}