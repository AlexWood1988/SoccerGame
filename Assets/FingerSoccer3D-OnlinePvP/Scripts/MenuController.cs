using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    /// <summary>
    /// Main Menu Controller.
    /// This class handles all touch events on buttons, and also updates the 
    /// player status (wins and available-money) on screen.
    /// </summary>

	public static bool canTap = false;			//flag to prevent double tap
	public AudioClip tapSfx;					//tap sound for buttons click
	public Text playerWins;	
	public Text playerMoney;


	void Awake ()
	{
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
		Application.targetFrameRate = 60;		
		playerWins.text = "" + PlayerPrefs.GetInt("PlayerWins");
		playerMoney.text = "" + PlayerPrefs.GetInt("PlayerMoney");
	}


	IEnumerator Start() 
	{
		canTap = false;
		yield return new WaitForSeconds(0.2f);
		canTap = true;

		//Test Ads
		//GameObject.FindGameObjectWithTag("AdController").GetComponent<AdController>().RequestAndLoadInterstitialAd();
		//GameObject.FindGameObjectWithTag("AdController").GetComponent<AdController>().RequestBannerAd();
	}


	void playSfx (AudioClip _clip)
	{
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying)
        {
			GetComponent<AudioSource>().Play();
		}
	}


	public void ClickOnPlusButton()
	{
		if (!canTap)
			return;
		canTap = false;

		SceneManager.LoadScene("BuyCoinPack-c#");
	}


	public void ClickOnShopButton()
	{
		if (!canTap)
			return;
		canTap = false;

		StartCoroutine(ClickOnShopButtonCo());
	}


	public IEnumerator ClickOnShopButtonCo()
	{
		playSfx(tapSfx);
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene("Shop-c#");
	}

	/// <summary>
	/// Gameplay mode buttons
	/// </summary>

	//player vs AI mode
	public void PlaySinglePlayer()
    {
		if (!canTap)
			return;
		canTap = false;

		StartCoroutine(PlaySinglePlayerCo());
    }

	public IEnumerator PlaySinglePlayerCo()
    {
		//Admob test
		//GameObject.FindGameObjectWithTag("AdController").GetComponent<AdController>().RequestAndLoadInterstitialAd();
		//GameObject.FindGameObjectWithTag("AdController").GetComponent<AdController>().RequestBannerAd();

		playSfx(tapSfx);                            //play touch sound
		PlayerPrefs.SetInt("GameMode", 0);          //set game mode to fetch later in "Game" scene
		PlayerPrefs.SetInt("IsLeague", 1);          //Single player mode always starts a league session
		PlayerPrefs.SetInt("IsCrazyMode", 0);       //Crazy mode?
		PlayerPrefs.SetInt("IsTournament", 0);      //are we playing in a tournament?
		PlayerPrefs.SetInt("IsPenalty", 0);         //are we playing penalty kicks?
		PlayerPrefs.SetInt("IsOnline", 0);           //are we playing online match?
		PlayerPrefs.SetInt("IsFakeOnlineWithBot", 0);   //New
		yield return new WaitForSeconds(1.0f);      //Wait for the animation to end
		SceneManager.LoadScene("Config-c#");        //Load the next scene
	}

	//two players (local) mode
	public void PlayTwoPlayersLocal()
	{
		if (!canTap)
			return;
		canTap = false;

		StartCoroutine(PlayTwoPlayersLocalCo());
	}

	public IEnumerator PlayTwoPlayersLocalCo()
	{
		playSfx(tapSfx);
		PlayerPrefs.SetInt("GameMode", 1);
		PlayerPrefs.SetInt("IsLeague", 0);
		PlayerPrefs.SetInt("IsCrazyMode", 0);
		PlayerPrefs.SetInt("IsTournament", 0);
		PlayerPrefs.SetInt("IsPenalty", 0);
		PlayerPrefs.SetInt("IsOnline", 0);
		PlayerPrefs.SetInt("IsFakeOnlineWithBot", 0);   //New
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene("Config-c#");
	}

	//Tournament
	public void PlayTournament()
	{
		if (!canTap)
			return;
		canTap = false;

		StartCoroutine(PlayTournamentCo());
	}

	public IEnumerator PlayTournamentCo()
	{
		playSfx(tapSfx);
		PlayerPrefs.SetInt("GameMode", 0);
		PlayerPrefs.SetInt("IsLeague", 0);
		PlayerPrefs.SetInt("IsCrazyMode", 0);
		PlayerPrefs.SetInt("IsTournament", 1);
		PlayerPrefs.SetInt("IsPenalty", 0);
		PlayerPrefs.SetInt("IsOnline", 0);
		PlayerPrefs.SetInt("IsFakeOnlineWithBot", 0);   //New
		yield return new WaitForSeconds(1.0f);

		//if we load the tournament scene directly, player won't get the chance to select a team
		//and has to play with the default team
		//SceneManager.LoadScene("Tournament-c#");

		//here we let the player to use to modified config scene to select a team
		SceneManager.LoadScene("Config-c#");
	}

	//Online Match
	public void PlayTwoPlayersOnline()
	{
		if (!canTap)
			return;
		canTap = false;

		StartCoroutine(PlayTwoPlayersOnlineCo());
	}

	public IEnumerator PlayTwoPlayersOnlineCo()
	{
		playSfx(tapSfx);
		PlayerPrefs.SetInt("GameMode", 3);
		PlayerPrefs.SetInt("IsLeague", 0);
		PlayerPrefs.SetInt("IsCrazyMode", 0);
		PlayerPrefs.SetInt("IsTournament", 0);
		PlayerPrefs.SetInt("IsPenalty", 0);
		PlayerPrefs.SetInt("IsOnline", 1);
		PlayerPrefs.SetInt("IsFakeOnlineWithBot", 0);   //New
		yield return new WaitForSeconds(1.0f);

		//load config scene for letting player select a team before going to lobby
		SceneManager.LoadScene("Config-c#");
	}


	//Crazy mode
	public void PlayCazyMode()
	{
		if (!canTap)
			return;
		canTap = false;

		StartCoroutine(PlayCazyModeCo());
	}

	public IEnumerator PlayCazyModeCo()
	{
		playSfx(tapSfx);
		PlayerPrefs.SetInt("GameMode", 0);
		PlayerPrefs.SetInt("IsLeague", 0);
		PlayerPrefs.SetInt("IsCrazyMode", 1);
		PlayerPrefs.SetInt("IsTournament", 0);
		PlayerPrefs.SetInt("IsPenalty", 0);
		PlayerPrefs.SetInt("IsOnline", 0);
		PlayerPrefs.SetInt("IsFakeOnlineWithBot", 0);   //New
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene("Config-c#");
	}

}