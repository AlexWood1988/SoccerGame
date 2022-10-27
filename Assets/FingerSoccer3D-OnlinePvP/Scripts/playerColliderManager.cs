using UnityEngine;
using System.Collections;

public class playerColliderManager : MonoBehaviour {

    /// <summary>
    /// Optional controller for collision of player units vs other elements in the scene like ball or opponent units
    /// </summary>

    public AudioClip unitsBallHit;		//units hits the ball sfx
	public AudioClip unitsGeneralHit;   //units general hit sfx 

	//Update v1.1 - commentator
	private GameObject commentatorManager;

    private void Awake()
    {
		commentatorManager = GameObject.FindGameObjectWithTag("CommentatorManager");
	}

    void OnCollisionEnter (Collision other)
    {
		//print("<b>" + gameObject.name + " collided with: " + other.gameObject.name + "</b>");
		//print("<b>" + gameObject.name + " collided with: " + other.gameObject.tag + "</b>");

		switch (other.gameObject.tag)
        {
			case "Opponent":				
			case "Player":				
			case "Player_2":				
				//PlaySfx(unitsGeneralHit);
				if(Random.value > 0.75f)
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("UnitCollision", 0);
				break;

			case "ball":
				PlaySfx(unitsBallHit);

				//Update v1.1
				//play voice - good shoot
				//print("HIT Speed: " + GetComponent<playerController>().moveSpeed);
				if (GlobalGameManager.opponentsTurn && GetComponent<playerController>().moveSpeed >= 10)
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("GoodShoot", 0);

				//play voice - bad shoot
				if (GlobalGameManager.opponentsTurn && GetComponent<playerController>().moveSpeed < 10)
					commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("BadShoot", 0);

				break;
		}
	}

	void PlaySfx (AudioClip _clip)
    {
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying)
        {
			GetComponent<AudioSource>().Play();
		}
	}

}