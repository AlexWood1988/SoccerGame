using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class FreeCoinController : MonoBehaviour
{
	/// <summary>
	/// We use this class to give the player a chance to receive a few coins everytime he uses the game.
	/// This way you can have a better player-retention by motivating the players to return to game as often
	/// to receive a free prize.
	/// </summary>

	public Text buttonLabel;
	public int prizeAmount = 10;		//amount
	public int prizeTimeInterval = 6; 	//Hours

	private DateTime baseTime = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
	private int timestamp;
	private bool tapFlag;

	void Awake ()
	{		
		//deactivate the button on game start
		GetComponent<Button> ().interactable = false;
		tapFlag = false;

		//set UI text
		buttonLabel.text = prizeAmount + " free coin every " + prizeTimeInterval + " hours!";
		EnableFreeCoins();
	}


	IEnumerator Start ()
	{		
		//activate the button
		yield return new WaitForSeconds(1.0f);
		GetComponent<Button> ().interactable = true;
	}


	public void ClickOnFreeCoinButton()
    {
        if (tapFlag)
            return;

		StartCoroutine(ClickOnFreeCoinButtonCo());
    }


	public IEnumerator ClickOnFreeCoinButtonCo()
	{
		//set tap flag
		tapFlag = true;
		//get available coins
		int availableCoins = PlayerPrefs.GetInt("PlayerMoney");
		PlayerPrefs.SetInt("PlayerMoney", availableCoins + prizeAmount);
		//wait
		yield return new WaitForSeconds(1.0f);
		//reload
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	void EnableFreeCoins()
    {
		timestamp = (int)(System.DateTime.UtcNow - baseTime).TotalSeconds;
		print("Global System Time is: " + timestamp.ToString());

		//User can get a prize every N hours (1 hour = 3600 seconds)
		//when he enters the game for the first time, we save the time and then recheck it we passed the 21600, 
		//if so we give the coins and resave the new time. else we bypass everything.
		int deltaTimeForPrize = 3600 * prizeTimeInterval; //in seconds

		//runs just once
		if(!PlayerPrefs.HasKey("lastPlayTime"))
        {			
			PlayerPrefs.SetInt("lastPlayTime", timestamp);
			print("Initial Timestamp saved...");
			return;
		}

		//give prize every N hours
		if(timestamp - PlayerPrefs.GetInt("lastPlayTime") >= deltaTimeForPrize)
        {			
			print("Wow, welcome back. You won something!");
			PlayerPrefs.SetInt("lastPlayTime", timestamp);
		}
        else
        {
			print("No prize available. come back later!");
			gameObject.SetActive(false);
		}
	}
}
