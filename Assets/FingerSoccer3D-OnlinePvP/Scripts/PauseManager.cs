using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {

    /// <summary>
    /// This class manages pause and unpause states.
    /// </summary>

    public static PauseManager instance;
	internal bool isPaused;
	private float savedTimeScale;
	public GameObject pausePlane;
	public GameObject formationPlane;	//not needed is Panalty scene!

    private GlobalGameManager gameController;

	enum Page { PLAY, PAUSE }
	private Page currentPage = Page.PLAY;


	void Awake ()
	{
		instance = this;
		isPaused = false;
		
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
		
		if(pausePlane)
	    	pausePlane.SetActive(false); 

		if (formationPlane)
			formationPlane.SetActive (false);

        if (SceneManager.GetActiveScene().name != "Penalty-c#")
            gameController = GameObject.Find("GameController").GetComponent<GlobalGameManager>();
        else
            gameController = null;
    }


	void Update ()
	{		
		//optional pause in Editor & Windows (just for debug)
		if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
        {
			//PAUSE THE GAME
			ClickOnPauseBtn();
		}
		
		//debug restart
		if(Input.GetKeyDown(KeyCode.R) && Application.isEditor)
        {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}


	public void ClickOnContinueTournamentButton()
    {
		UnPauseGame();
		SceneManager.LoadScene("Tournament-c#");
	}


	public void ClickOnSummaryButton()
    {
		UnPauseGame();
		if (PhotonNetwork.IsConnected)
			PhotonNetwork.Disconnect();
		//SceneManager.LoadScene("EndGameInfo");
		SceneManager.LoadScene("Menu-c#");
	}


	public void ClickOnRestartButton()
    {
		UnPauseGame();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	public void ClickOnMenuButton()
    {
		UnPauseGame();
		if (PhotonNetwork.IsConnected)
			PhotonNetwork.Disconnect();
		SceneManager.LoadScene("Menu-c#");
	}


	public void ClickOnPauseBtn()
    {
		switch (currentPage)
		{
			case Page.PLAY:
				PauseGame();
				break;
			case Page.PAUSE:
				UnPauseGame();
				break;
			default:
				currentPage = Page.PLAY;
				break;
		}
	}


	public void ClickOnP1FormationButton()
    {
		if (!gameController)
			return;

		if (gameController.isOnline)
		{
			if (gameController.isServer) //open player 1 formation in online mode only if im server
			{
				IngameFormation.formationChangeRequestID = 1;

				if (formationPlane)
					formationPlane.SetActive(true);
			}
		}
		else
		{
			IngameFormation.formationChangeRequestID = 1;

			if (formationPlane)
				formationPlane.SetActive(true);
		}
	}


	public void ClickOnP2FormationButton()
	{
		if (!gameController)
			return;

		if (gameController.isOnline)
		{
			if (!gameController.isServer) //open player 2 formation in online mode only if im not server
			{
				IngameFormation.formationChangeRequestID = 2;

				if (formationPlane)
					formationPlane.SetActive(true);
			}
		}
		else
		{
			IngameFormation.formationChangeRequestID = 2;

			if (formationPlane)
				formationPlane.SetActive(true);
		}
	}


	public void AcceptFormationButton()
    {
		if (formationPlane)
			formationPlane.SetActive(false);
	}


	void PauseGame ()
	{		
		print("Game is Paused...");
		isPaused = true;
		savedTimeScale = Time.timeScale;
	    Time.timeScale = 0;
	    AudioListener.volume = 0;

	    if(pausePlane)
	    	pausePlane.SetActive(true);
		
	    currentPage = Page.PAUSE;
	}


	public void UnPauseGame ()
	{		
		print("Unpause");
	    isPaused = false;
	    Time.timeScale = savedTimeScale;
	    AudioListener.volume = 1.0f;

		if(pausePlane)
	    	pausePlane.SetActive(false);   

		if (formationPlane)
			formationPlane.SetActive (false);

	    currentPage = Page.PLAY;
	}
}