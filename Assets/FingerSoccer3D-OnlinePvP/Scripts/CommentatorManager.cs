using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommentatorManager : MonoBehaviour
{
	public static CommentatorManager instance;

	//only play one sound clip at a time
	public static bool canPlayClip;

	//full list of clips
	[Header("Full Audio Lists")]
	public AudioClip[] commentatorVoices;

	//Subjective clips
	[Header("Goal")]
	public AudioClip[] commentatorGoals;

	[Header("GoalKeeper")]
	public AudioClip[] commentatorGoalkeeper;

	[Header("Good Shoot")]
	public AudioClip[] commentatorGoodShoot;

	[Header("Bad Shoot")]
	public AudioClip[] commentatorBadShot;

	[Header("Match Start")]
	public AudioClip[] commentatorMatchStart;

	[Header("Match End")]
	public AudioClip[] commentatorMatchEnd;

	[Header("Match Tie")]
	public AudioClip[] commentatorMatchTie;

	[Header("Hit Post")]
	public AudioClip[] commentatorHitPost;

	[Header("Game Status")]
	public AudioClip[] commentatorGameStatus;

	[Header("Time Low")]
	public AudioClip[] commentatorTimeLow;

	[Header("Collision")]
	public AudioClip[] commentatorUnitCollision;


	void Awake ()
	{
		canPlayClip = true;
	}


	//plays the exact clip indicated with the ID
	public void PlayVoiceClip(int clipID)
	{
		if (!canPlayClip)
			return;
		
		canPlayClip = false;
		StartCoroutine (ReactiveAudioPlayCo());

		GetComponent<AudioSource>().clip = commentatorVoices[clipID];

		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}


	public void PlayVoiceEvent(string state, int forcePlay) 
	{
		//if we need a clip to be played we need to send it with forcePlay flag as 1
		if (!canPlayClip && forcePlay == 0)
			return;
	
		canPlayClip = false;
		StartCoroutine (ReactiveAudioPlayCo());

		//dont play a sound if the game is already finished.
		if (GlobalGameManager.gameIsFinished)
			return;

		//Debug
		print("Playing: " + state);

		int audioclipId = 0;
		switch (state) {

		case "Goal":
			audioclipId = Random.Range (0, commentatorGoals.Length);
			GetComponent<AudioSource> ().clip = commentatorGoals[audioclipId];
			break;

		case "GoodShoot":
				audioclipId = Random.Range(0, commentatorGoodShoot.Length);
				GetComponent<AudioSource>().clip = commentatorGoodShoot[audioclipId];
				break;

		case "BadShoot":
				audioclipId = Random.Range(0, commentatorBadShot.Length);
				GetComponent<AudioSource>().clip = commentatorBadShot[audioclipId];
				break;

		case "HitPost":
				audioclipId = Random.Range(0, commentatorHitPost.Length);
				GetComponent<AudioSource>().clip = commentatorHitPost[audioclipId];
				break;

		case "UnitCollision":
				audioclipId = Random.Range(0, commentatorUnitCollision.Length);
				GetComponent<AudioSource>().clip = commentatorUnitCollision[audioclipId];
				break;

		case "MatchStart":
				audioclipId = Random.Range(0, commentatorMatchStart.Length);
				GetComponent<AudioSource>().clip = commentatorMatchStart[audioclipId];
				break;

		case "MatchTie":
				audioclipId = Random.Range(0, commentatorMatchTie.Length);
				GetComponent<AudioSource>().clip = commentatorMatchTie[audioclipId];
				break;

		case "TimeLow":
				audioclipId = Random.Range(0, commentatorTimeLow.Length);
				GetComponent<AudioSource>().clip = commentatorTimeLow[audioclipId];
				break;

		case "GameGoalkeeper":
				audioclipId = Random.Range(0, commentatorGoalkeeper.Length);
				GetComponent<AudioSource>().clip = commentatorGoalkeeper[audioclipId];
				break;

		case "GameStatus":
				audioclipId = Random.Range(0, commentatorGameStatus.Length);
				GetComponent<AudioSource>().clip = commentatorGameStatus[audioclipId];
				break;
		}

		//Play the selected audio
		if(!GetComponent<AudioSource>().isPlaying) 
		{
			GetComponent<AudioSource>().Play();
		}
	}


	public IEnumerator ReactiveAudioPlayCo() 
	{
		yield return new WaitForSeconds (Random.Range(3f, 4f));
		//yield return new WaitForSeconds(2.0f);
		canPlayClip = true;
	}

}
