using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConfigController : MonoBehaviour
{
    /// <summary>
    /// Main Config Controller.
    /// This class provides the available settings to the players, like formations,
    /// gameDuration, etc... and then prepare the main game with the setting.
    /// 
    /// Important:
    /// Upon the addition of tournament mode, this config scene also manages to set the player team 
    /// for the tournament by hiding unnecessary options. But in normal gameplay, it shows the full settings.
    /// Example:
    /// in Tournament mode: We just show the Player 1 team and formation selection options.
    /// We also config the start button to load the tournament scene afterward.
    /// </summary>

    private int isTournamentMode;

	private int isOnlineMode;

	public Button startButton;				//the actual start button!

	//required game objects to active/deactive
	public GameObject p1TeamSel;			//p1 team selection settings
    public Text p1LabelText;
    public GameObject p2TeamSel;			//p2 team selection settings
	public GameObject timeSel;				//time selection settings
	public GameObject fieldSel;				//field selection settings

	private int barScaleDivider = 10;		//we divide actual time/power setting of each team by this number
	public AudioClip tapSfx;                //tap sound for buttons click

    [Space(15)]
    public Sprite[] availableTeams;			//just the images.
	public string[] availableFormations;	//Just the string values. We setup actual values somewhere else.
	public string[] availableTimes;			//Just the string values. We setup actual values somewhere else.
    public Sprite[] availableFields;

    [Space(15)]
    public GameObject fieldIcon;
    private int selectedFieldIndex;

    //Reference to gameObjects
    [Space(15)]
    public Image p1Team;
	public GameObject p1PlayerModel;
	public GameObject p1TeamLock;
	public GameObject p1FormationLock;
	public Image p1PowerBar;
	public Image p1TimeBar;
    public Image p1AimBar;

    [Space(15)]
    public GameObject fieldTBL;
    public GameObject fieldTBR;

    [Space(15)]
    public Image p2Team;
	public GameObject p2PlayerModel;
	public GameObject p2TeamLock;
	public GameObject p2FormationLock;
	public Image p2PowerBar;
	public Image p2TimeBar;
    public Image p2AimBar;

    [Space(15)]
    public Text p1FormationLabel;			//UI text object
	public Text p2FormationLabel;			//UI text object
	public Text gameTimeLabel;			    //UI text object

	private int p1FormationCounter = 0;			//Actual player-1 formation index
	private int p1TeamCounter = 0;				//Actual player-1 team index
	private int p2FormationCounter = 0;			//Actual player-2 formation index
	private int p2TeamCounter = 0;				//Actual player-2 team index
	private int timeCounter = 0;				//Actual game-time index


	void Awake ()
	{
        //populate all public arrays
        //fields
        availableFields = new Sprite[GlobalGameSettings.instance.availableFields.Length];
        for (int i = 0; i < availableFields.Length; i++)
        {
            availableFields[i] = GlobalGameSettings.instance.availableFields[i].fieldSprite;
        }

        //times
        availableTimes = new string[GlobalGameSettings.instance.availableTimes.Length];
        for (int i = 0; i < availableTimes.Length; i++)
        {
            availableTimes[i] = GlobalGameSettings.instance.availableTimes[i].timeName;
        }

        //Formations
        availableFormations = new string[GlobalGameSettings.instance.availableFormations.Length];
        for (int i = 0; i < availableFormations.Length; i++)
        {
            availableFormations[i] = GlobalGameSettings.instance.availableFormations[i].formationName;
        }

        //teams
        availableTeams = new Sprite[GlobalGameSettings.instance.availableTeams.Length];
        for (int i = 0; i < availableTeams.Length; i++)
        {
            availableTeams[i] = GlobalGameSettings.instance.availableTeams[i].teamSprite;		//Populate the array with Sprites instead of texture2D
        }
               
        //Init the game
        selectedFieldIndex = 0;

        //check if this config scene is getting used for tournament,online or normal play mode
        isTournamentMode = PlayerPrefs.GetInt("IsTournament");
        isOnlineMode = PlayerPrefs.GetInt("IsOnline");
		if( isTournamentMode == 1 || 
            isOnlineMode == 1 || 
            PlayerPrefs.GetInt("IsCrazyMode") == 1 || 
            (PlayerPrefs.GetInt("GameMode") == 0 && PlayerPrefs.GetInt("IsPenalty") == 0 && PlayerPrefs.GetInt("IsLeague") == 1) )
		{
			if (isTournamentMode == 1)
			{
				//first of all, check if we are going to continue an unfinished tournament
				if(PlayerPrefs.GetInt("TorunamentLevel") > 0) {
					//if so, there is no need for any configuration. load the next scene.
					SceneManager.LoadScene("Tournament-c#");
					return;
				}
			}

			//disable unnecessary options
			p2TeamSel.SetActive(false);
			timeSel.SetActive(false);
			fieldSel.SetActive(false);
			fieldIcon.SetActive(false);
			fieldTBL.SetActive(false);
			fieldTBR.SetActive(false);

			//p1TeamSel.transform.position = new Vector3(0, 5.7f, -1);
			p1TeamSel.GetComponent<RectTransform>().localPosition = new Vector3(0, -25f, 0);
            p1LabelText.text = "Select your team";
        }

		//P1
		p1FormationLabel.text = availableFormations[p1FormationCounter];    //loads default formation
		p1PowerBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.x / barScaleDivider;
		p1TimeBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.y / barScaleDivider;
		p1AimBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.z / barScaleDivider;

        //team lock state
        checkTeamLockState(p1TeamLock, p1TeamCounter); 
		//formation lock state
		checkFormationLockState(p1FormationLock, p1FormationCounter); 

		//P2/AI
		p2FormationLabel.text = availableFormations[p2FormationCounter];   //loads default formation
		p2PowerBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.x / barScaleDivider;
		p2TimeBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.y / barScaleDivider;
		p2AimBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.z / barScaleDivider;

		//team lock state
		checkTeamLockState(p2TeamLock, p2TeamCounter);
		//formation lock state
		checkFormationLockState(p2FormationLock, p2FormationCounter); 

		gameTimeLabel.text = availableTimes[timeCounter];               //loads default game-time

		//New - fix the shirt texture (flag) on 3d player models
		//set the flag on Unit's shirt
		if (p1PlayerModel)
			p1PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamIcon;
		if (p2PlayerModel)
        {
			//v1
			//p2PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamIcon;
			//v1.1 - we need to make sure we are referencing the "dress" material on the player object
			p2PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[0].mainTexture = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamIcon;
		}
	}


    //*****************************************************************************
    // FSM
    //*****************************************************************************
    void Update ()
	{	
		if(Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene("Menu-c#");

		if (!p1TeamLock.activeSelf && !p2TeamLock.activeSelf && !p1FormationLock.activeSelf && !p2FormationLock.activeSelf) 
		{
			startButton.interactable = true;
		} 
		else 
		{
			startButton.interactable = false;
		}
	}


    /// <summary>
    /// When selection form available options, when player reaches the last option,
    /// and still taps on the next option, this will cycle it again to the first element of options.
    /// This is for p-1, p-2 and time/field settings.
    /// </summary>
    void fixCounterLengths ()
    {
		//set array counters limitations
		
		//Player-1 formation
		if(p1FormationCounter > availableFormations.Length - 1)
			p1FormationCounter = 0;
		if(p1FormationCounter < 0)
			p1FormationCounter = availableFormations.Length - 1;

		//Player-1 team
		if(p1TeamCounter > availableTeams.Length - 1)
			p1TeamCounter = 0;
		if(p1TeamCounter < 0)
			p1TeamCounter = availableTeams.Length - 1;
			
		//Player-2 formation
		if(p2FormationCounter > availableFormations.Length - 1)
			p2FormationCounter = 0;
		if(p2FormationCounter < 0)
			p2FormationCounter = availableFormations.Length - 1;

		//Player-2 team
		if(p2TeamCounter > availableTeams.Length - 1)
			p2TeamCounter = 0;
		if(p2TeamCounter < 0)
			p2TeamCounter = availableTeams.Length - 1;
			
		//GameTime
		if(timeCounter > availableTimes.Length - 1)
			timeCounter = 0;
		if(timeCounter < 0)
			timeCounter = availableTimes.Length - 1;

        //Fields
        if (selectedFieldIndex > availableFields.Length - 1)
            selectedFieldIndex = 0;
        if (selectedFieldIndex < 0)
            selectedFieldIndex = availableFields.Length - 1;
    }


	//check if the selected team is locked or selectable
	void checkTeamLockState(GameObject teamLock, int teamCounter)
    {
		//check for initial team lock settings
		print(teamLock.name + " (" + teamCounter + ") state is: " + GlobalGameSettings.instance.availableTeams[teamCounter].teamAttributes.w);
		if (GlobalGameSettings.instance.availableTeams[teamCounter].teamAttributes.w == 1) 
		{
			teamLock.SetActive (false);
		} 
		else 
		{
			//now check for shop settings if user has purchased this team
			if (PlayerPrefs.GetInt ("shopTeam-" + teamCounter.ToString ()) == 1)
            {
				print ("Player has already purchased this team!");
				teamLock.SetActive (false);
			}
            else
				teamLock.SetActive (true);
		}			
	}


	//check if the selected formation is locked or selectable
	void checkFormationLockState(GameObject formationLock, int formationCounter) 
	{
		//check for initial formation lock settings
		print(formationLock.name + " (" + formationCounter + ") state is: " + GlobalGameSettings.instance.availableFormations[formationCounter].isUnlocked);
		if (GlobalGameSettings.instance.availableFormations[formationCounter].isUnlocked == 1)
		{
			formationLock.SetActive (false);
		} 
		else 
		{
			//now check for shop settings if user has purchased this formation
			if (PlayerPrefs.GetInt ("shopFormation-" + formationCounter.ToString ()) == 1)
            {
				print ("Player has already purchased this formation!");
				formationLock.SetActive (false);
			}
            else
				formationLock.SetActive (true);
		}			
	}


	void playSfx (AudioClip _clip)
	{
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}


	public void ClickOnBackButton()
	{
		StartCoroutine(ClickOnBackButtonCo());
	}

	public IEnumerator ClickOnBackButtonCo()
	{
		playSfx(tapSfx);
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene("Menu-c#");
	}


	public void ClickOnStartButton()
    {
		StartCoroutine(ClickOnStartButtonCo());
    }

	public IEnumerator ClickOnStartButtonCo()
    {
		playSfx(tapSfx);

		//Save configurations
		PlayerPrefs.SetInt("PlayerFormation", p1FormationCounter);      //save the player-1 formation index
		PlayerPrefs.SetInt("PlayerFlag", p1TeamCounter);                //save the player-1 team index
		PlayerPrefs.SetInt("Player2Formation", p2FormationCounter);     //save the player-2 formation index
		PlayerPrefs.SetInt("Player2Flag", p2TeamCounter);               //save the player-2 team index

		//Opponent uses the exact same settings as player-2, so:
		PlayerPrefs.SetInt("OpponentFormation", p2FormationCounter);    //save the Opponent formation index
		PlayerPrefs.SetInt("OpponentFlag", p2TeamCounter);              //save the Opponent team index
		PlayerPrefs.SetInt("GameTime", timeCounter);                    //save the game-time value
		PlayerPrefs.SetInt("GameField", selectedFieldIndex);            //save the field value

		//** Please note that we just set the indexes here. We fetch the actual index values in the <<Game>> scene.
		yield return new WaitForSeconds(0.5f);

		//if we are using the config for a tournament, we should load it as the next scene.
		//otherwise we can instantly jump to the Game scene in normal play mode.
		if (isTournamentMode == 1)
			SceneManager.LoadScene("Tournament-c#");
		else if (isOnlineMode == 1)
			SceneManager.LoadScene("Login-c#");
		else if (PlayerPrefs.GetInt("GameMode") == 0)
		{
			//if this is a crazy mode, we need to load the game asap
			if (PlayerPrefs.GetInt("IsCrazyMode") == 1)
			{
				SceneManager.LoadScene("Game-c#");
			}
			else
			{
				//else, this is a new single player league game
				SceneManager.LoadScene("Leagues");
			}
		}
		else
			SceneManager.LoadScene("Game-c#");  //local 2-players mode
	}


	/// <summary>
	/// UI button events
	/// </summary>	
	public void ClickOnP1PrevTeamButton()
    {
		playSfx(tapSfx);
		p1TeamCounter--;            //cycle through available team indexs for this player. This is the main index value.
		fixCounterLengths();        //when reached to the last option, start from the first index of the other side.
		p1Team.sprite = availableTeams[p1TeamCounter]; //set the flag on UI

		//set the flag on Unit's shirt
		if(p1PlayerModel)
			p1PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamIcon;

		p1PowerBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.x / barScaleDivider;
		p1TimeBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.y / barScaleDivider;
		p1AimBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.z / barScaleDivider;
		//team lock state
		checkTeamLockState(p1TeamLock, p1TeamCounter);
	}

	public void ClickOnP1NextTeamButton()
	{
		playSfx(tapSfx);
		p1TeamCounter++;            //cycle through available team indexs for this player. This is the main index value.
		fixCounterLengths();        //when reached to the last option, start from the first index of the other side.
		p1Team.sprite = availableTeams[p1TeamCounter]; //set the flag on UI

		//set the flag on Unit's shirt
		if (p1PlayerModel)
			p1PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamIcon;

		p1PowerBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.x / barScaleDivider;
		p1TimeBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.y / barScaleDivider;
		p1AimBar.fillAmount = GlobalGameSettings.instance.availableTeams[p1TeamCounter].teamAttributes.z / barScaleDivider;
		//team lock state
		checkTeamLockState(p1TeamLock, p1TeamCounter);
	}

	public void ClickOnP1PrevFormationButton()
	{
		playSfx(tapSfx);
		p1FormationCounter--;												//cycle through available formation indexs for this player. This is the main index value.
		fixCounterLengths();												//when reached to the last option, start from the first index of the other side.
		p1FormationLabel.text = availableFormations[p1FormationCounter];	//set the string on the UI																								 
		checkFormationLockState(p1FormationLock, p1FormationCounter);       //formation lock state
	}

	public void ClickOnP1NextFormationButton()
	{
		playSfx(tapSfx);
		p1FormationCounter++;
		fixCounterLengths();
		p1FormationLabel.text = availableFormations[p1FormationCounter];
		//formation lock state
		checkFormationLockState(p1FormationLock, p1FormationCounter);
	}

	public void ClickOnP2PrevTeamButton()
	{
		playSfx(tapSfx);
		p2TeamCounter--;            //cycle through available team indexs for this player. This is the main index value.
		fixCounterLengths();        //when reached to the last option, start from the first index of the other side.
		p2Team.sprite = availableTeams[p2TeamCounter]; //set the flag on UI

		//set the flag on Unit's shirt
		if (p2PlayerModel)
        {
			//v1
			//p2PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamIcon;
			//v1.1 - we need to make sure we are referencing the "dress" material on the player object
			p2PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[0].mainTexture = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamIcon;
		}

		p2PowerBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.x / barScaleDivider;
		p2TimeBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.y / barScaleDivider;
		p2AimBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.z / barScaleDivider;
		//team lock state
		checkTeamLockState(p2TeamLock, p2TeamCounter);
	}

	public void ClickOnP2NextTeamButton()
	{
		playSfx(tapSfx);
		p2TeamCounter++;            //cycle through available team indexs for this player. This is the main index value.
		fixCounterLengths();        //when reached to the last option, start from the first index of the other side.
		p2Team.sprite = availableTeams[p2TeamCounter]; //set the flag on UI

		//set the flag on Unit's shirt
		if (p2PlayerModel)
        {
			//v1
			//p2PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamIcon;
			//v1.1 - we need to make sure we are referencing the "dress" material on the player object
			p2PlayerModel.transform.GetChild(0).GetComponent<Renderer>().materials[0].mainTexture = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamIcon;
		}

		p2PowerBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.x / barScaleDivider;
		p2TimeBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.y / barScaleDivider;
		p2AimBar.fillAmount = GlobalGameSettings.instance.availableTeams[p2TeamCounter].teamAttributes.z / barScaleDivider;
		//team lock state
		checkTeamLockState(p2TeamLock, p2TeamCounter);
	}

	public void ClickOnP2PrevFormationButton()
	{
		playSfx(tapSfx);
		p2FormationCounter--;
		fixCounterLengths();
		p2FormationLabel.text = availableFormations[p2FormationCounter];
		//formation lock state
		checkFormationLockState(p2FormationLock, p2FormationCounter);
	}

	public void ClickOnP2NextFormationButton()
	{
		playSfx(tapSfx);
		p2FormationCounter++;
		fixCounterLengths();
		p2FormationLabel.text = availableFormations[p2FormationCounter];
		//formation lock state
		checkFormationLockState(p2FormationLock, p2FormationCounter);
	}

	public void ClickOnPrevStadiumButton()
	{
		playSfx(tapSfx);
		selectedFieldIndex--;
		fixCounterLengths();
		fieldIcon.GetComponent<Image>().sprite = availableFields[selectedFieldIndex];
	}

	public void ClickOnNextStadiumButton()
	{
		playSfx(tapSfx);
		selectedFieldIndex++;
		fixCounterLengths();
		fieldIcon.GetComponent<Image>().sprite = availableFields[selectedFieldIndex];
	}

	public void ClickOnPrevDurationButton()
	{
		playSfx(tapSfx);
		timeCounter--;
		fixCounterLengths();
		gameTimeLabel.text = availableTimes[timeCounter];
	}

	public void ClickOnNextDurationButton()
	{
		playSfx(tapSfx);
		timeCounter++;
		fixCounterLengths();
		gameTimeLabel.text = availableTimes[timeCounter];
	}

}