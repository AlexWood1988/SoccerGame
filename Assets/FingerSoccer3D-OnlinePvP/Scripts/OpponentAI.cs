using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class OpponentAI : MonoBehaviour {

    /// <summary>
    /// Main AI Controller.
    /// This class manages the shooting process of AI (CPU opponent).
    /// This also handles the rendering of AI debug lines in editor.
    /// </summary>

    public static GameObject[] myTeam;		//List of all AI units
	private GameObject target;				//reference to main Ball
	private float distanceToTarget;			//Distance of selected unit to ball
	private Vector3 directionToTarget;		//Direction of selected unit to ball
	private float shootPower = 40;			//AI shoot power. Edit with extreme caution!!!!

	public static bool opponentCanShoot;	//Allowed to shoot? flag
	private float shootTime;				//Allowed time to perform the shoot
	private bool  isReadyToShoot;			//if all processes are done, flag

	//AI
	private GameObject actor;				//Selected unit to shoot	
	private GameObject gameController;		//Reference to main game controller
	private GameObject PlayerBasketCenter;	//helper object which shows the center of player gate to the AI
	private int opponentFormation;			//Selected formation for AI
	internal bool canChangeFormation;		//Is allowed to change formation on the fly?

    //New update v1.2
    //Crazy mode's mandatory delay for AI
    [Range(0.35f, 1.5f)]
    public float forcedCrazyMoveDelay = 0.7f;
    public static bool canPerformCrazyMove;


    void Awake ()
	{
		gameController = GameObject.FindGameObjectWithTag("GameController");
		target = GameObject.FindGameObjectWithTag("ball");
		PlayerBasketCenter = GameObject.FindGameObjectWithTag("PlayerBasketCenter");
		isReadyToShoot = false;
		opponentCanShoot = true;
        canPerformCrazyMove = true;
        canChangeFormation = true;

		if(PlayerPrefs.HasKey("OpponentFormation"))
			opponentFormation = PlayerPrefs.GetInt("OpponentFormation");
		else	
			opponentFormation = 0; //Default Formation
		
		//cache all available units
		myTeam = GameObject.FindGameObjectsWithTag("Opponent").OrderBy(go => go.name).ToArray();
		
        //debug
		int i = 1;
		foreach(GameObject unit in myTeam)
        {
			//Optional
			unit.name = "Opponent-Player-" + i;
			unit.GetComponent<OpponentUnitController>().unitIndex = i;
			unit.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("OpponentFlag", 1)].teamIcon;

			//2021 - set the flag on the cap object
			unit.GetComponent<OpponentUnitController>().cap.GetComponent<Renderer>().material.mainTexture = GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("OpponentFlag", 1)].teamIcon;
			//2021 - set the flag on Unit's shirt
			//unit.GetComponent<OpponentUnitController>().modelRig.GetComponent<Renderer>().materials[2].mainTexture = GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("OpponentFlag", 1)].teamIcon;
			int modelTotalMaterials = unit.GetComponent<OpponentUnitController>().modelRig.GetComponent<Renderer>().materials.Length;
			for (int j = 0; j < modelTotalMaterials; j++)
			{
				if (unit.GetComponent<OpponentUnitController>().modelRig.GetComponent<Renderer>().materials[j].name.Contains("dress") || unit.GetComponent<OpponentUnitController>().modelRig.GetComponent<Renderer>().materials[j].name.Contains("shirt"))
					unit.GetComponent<OpponentUnitController>().modelRig.GetComponent<Renderer>().materials[j].mainTexture = GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("OpponentFlag", 1)].teamIcon;
			}

			i++;		
			//print("My Team: " + unit.name);
		}
	}


	void Start ()
    {
		if(!GlobalGameManager.isPenaltyKick)
        {
			StartCoroutine(changeFormation(opponentFormation, 1));
			canChangeFormation = false;
		}
        else
        {
			StartCoroutine(goToPosition(myTeam, GlobalGameManager.AIDestination, 1));
		}
	}


    /// <summary>
    /// AI can change it's formation to a new one after it delivers or receives a goal.
    /// </summary>
    /// <param name="_formationIndex"></param>
    /// <param name="_speed"></param>
    /// <returns></returns>
    public IEnumerator changeFormation ( int _formationIndex ,   float _speed  )
    {
		//cache the initial position of all units
		List<Vector3> unitsSartingPosition = new List<Vector3>();
		foreach(var unit in myTeam)
        {
			unitsSartingPosition.Add(unit.transform.position);	//get the initial postion of this unit for later use.
			unit.GetComponent<MeshCollider>().enabled = false;	//no collision for this unit till we are done with re positioning.
		}
		
		float t = 0;
		while(t < 1)
        {
			t += Time.deltaTime * _speed;
			for(int cnt = 0; cnt < myTeam.Length; cnt++) {
				myTeam[cnt].transform.position = new Vector3(	Mathf.SmoothStep(	unitsSartingPosition[cnt].x, 
				                                                               		FormationManager.getPositionInFormation(_formationIndex, cnt).x * -1,
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
			foreach(var unit in myTeam)
				unit.GetComponent<MeshCollider>().enabled = true;	//collision is now enabled.
		}
	}


	void Update ()
    {
		//prepare to shoot
		if(GlobalGameManager.opponentsTurn && opponentCanShoot) {
			opponentCanShoot = false;
			StartCoroutine(shoot());	
		}
	}


    /// <summary>
    /// Shoot the selected unit.
    /// All AI steps are described and fully commented.
    /// </summary>
    private GameObject bestShooter;	//select the best unit to shoot.
	IEnumerator shoot ()
    {
        //We need a mandatory delay for AI "JUST" in crazy mode!
        if (PlayerPrefs.GetInt("IsCrazyMode") == 1)
        {
            if (!canPerformCrazyMove)
                yield break;

            canPerformCrazyMove = false;
            //Ask GameManager to enable it again.
            StartCoroutine(gameController.GetComponent<GlobalGameManager>().EnableCrazyMoveForAI(forcedCrazyMoveDelay));
        }

        //wait for a while to fake thinking process :)
        yield return new WaitForSeconds(1.0f);

		//add a little extra delay for penalty kicks
		if(GlobalGameManager.isPenaltyKick)
			yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));

		//init
		bestShooter = null;

		//1. find units with good position to shoot
		//Units that are in the right hand side of the ball are considered a better options. 
		//They can have better angle to the player's gate.
		List<GameObject> shooters = new List<GameObject>();		//list of all good units
		List<float> distancesToBall = new List<float>();	//distance of these good units to the ball
		foreach(GameObject shooter in myTeam) {
			if(shooter.transform.position.x > target.transform.position.x + 1.5f) {
				shooters.Add(shooter);
				distancesToBall.Add( Vector3.Distance(shooter.transform.position, target.transform.position) );
			}
		}
		
		//if we found atleast one good unit...
		float minDistance = 1000;
		int minDistancePlayerIndex = 0;
		if(shooters.Count > 0) {
			//print("we have " + shooters.Count + " unit(s) in a good shoot position");
			for(int i = 0; i < distancesToBall.Count; i++) {
				if(distancesToBall[i] <= minDistance) {
					minDistance = distancesToBall[i];
					minDistancePlayerIndex = i;
				}
				//print(shooters[i] + " distance to ball is " + distancesToBall[i]);
			}
			//find the unit which is most closed to ball.
			bestShooter = shooters[minDistancePlayerIndex];
			//print("MinDistance to ball is: " + minDistance + " by opponent " + bestShooter.name);	
		} else {
			//print("no player is availabe for a good shoot!");
			//Select a random unit
			bestShooter = myTeam[Random.Range(0, myTeam.Length)];
		}
		
		//calculate direction and power and add a little randomness to the shoot (can be used to make the game easy or hard)
		float distanceCoef = 0;
		if(minDistance <= 5 && minDistance >= 0) 
			distanceCoef = Random.Range(1.0f, 2.5f);
		else
			distanceCoef = Random.Range(2.0f, 4.0f);
		print ("distanceCoef: " + distanceCoef);	
		
		//////////////////////////////////////////////////////////////////////////////////////////////////
		// Detecting the best angle for the shoot
		//////////////////////////////////////////////////////////////////////////////////////////////////
		Vector3 vectorToGate;					//direct vector from shooter to gate
		Vector3 vectorToBall;					//direct vector from shooter to ball
		float straightAngleDifferential;		//angle between "vectorToGate" and "vectorToBall" vectors
		vectorToGate = PlayerBasketCenter.transform.position - bestShooter.transform.position;
		vectorToBall = target.transform.position - bestShooter.transform.position;
		straightAngleDifferential = Vector3.Angle(vectorToGate, vectorToBall);
		//if angle between these two vector is lesser than 10 for example, we have a clean straight shot to gate.
		//but if the angle is more, we have to calculate the correct angle for the shoot.
		print("straightAngleDifferential: " + straightAngleDifferential);
		//////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////
		
		float shootPositionDifferential = bestShooter.transform.position.y - target.transform.position.y;
		print("Y differential for shooter is: " + shootPositionDifferential);
		
		if(straightAngleDifferential <= 10) {
			
			//direct shoot
			directionToTarget = target.transform.position - bestShooter.transform.position;
		
		} else if( Mathf.Abs(shootPositionDifferential) <= 0.5f ) {
		
			//direct shoot
			directionToTarget = target.transform.position - bestShooter.transform.position;
			
		} else if( Mathf.Abs(shootPositionDifferential) > 0.5f && Mathf.Abs(shootPositionDifferential) <= 1 ) {
		
			if(shootPositionDifferential > 0)
				directionToTarget = (target.transform.position - new Vector3(0, bestShooter.transform.localScale.z / 2.5f, 0)) - bestShooter.transform.position;
			else
				directionToTarget = (target.transform.position + new Vector3(0, bestShooter.transform.localScale.z / 2.5f, 0)) - bestShooter.transform.position;
				
		} else if( Mathf.Abs(shootPositionDifferential) > 1 && Mathf.Abs(shootPositionDifferential) <= 2 ) {
		
			if(shootPositionDifferential > 0)
				directionToTarget = (target.transform.position - new Vector3(0, bestShooter.transform.localScale.z / 2, 0)) - bestShooter.transform.position;
			else
				directionToTarget = (target.transform.position + new Vector3(0, bestShooter.transform.localScale.z / 2, 0)) - bestShooter.transform.position;
				
		} else if( Mathf.Abs(shootPositionDifferential) > 2 && Mathf.Abs(shootPositionDifferential) <= 3 ) {
		
			if(shootPositionDifferential > 0)
				directionToTarget = (target.transform.position - new Vector3(0, bestShooter.transform.localScale.z / 1.6f, 0)) - bestShooter.transform.position;
			else
				directionToTarget = (target.transform.position + new Vector3(0, bestShooter.transform.localScale.z / 1.6f, 0)) - bestShooter.transform.position;
				
		} else if( Mathf.Abs(shootPositionDifferential) > 3 ) {
		
			if(shootPositionDifferential > 0)
				directionToTarget = (target.transform.position - new Vector3(0, bestShooter.transform.localScale.z / 1.25f, 0)) - bestShooter.transform.position;
			else
				directionToTarget = (target.transform.position + new Vector3(0, bestShooter.transform.localScale.z / 1.25f, 0)) - bestShooter.transform.position;
				
		}
		
		//set the shoot power based on direction and distance to ball
		Vector3 appliedPower = Vector3.Normalize(directionToTarget) * shootPower;
		//add team power bonus
		//print ("OLD appliedPower: " + appliedPower.magnitude);
		appliedPower *= ( 1 + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("Player2Flag")].teamAttributes.x / 35.0f) );
		//print ("NEW appliedPower: " + appliedPower.magnitude);

		//Bug fix. Avoid shoot powers over 40, or the ball might fly off the level bounds.
		//Introduced in version 1.5+
		if(appliedPower.magnitude >= 40)
			appliedPower *= 0.82f;

		//if this is a penalty shoot, we sure have a direct shoot. 
		//So we add an extra layer for randomness in shoot direction
		if(GlobalGameManager.isPenaltyKick) {

			int dir = 1;
			float rnd = UnityEngine.Random.value;
			if(rnd > 0.5f)
				dir = 1;
			else
				dir = -1;

			directionToTarget = (target.transform.position - new Vector3(0, bestShooter.transform.localScale.z / Random.Range(1.55f, 4.75f), 0) * dir ) - bestShooter.transform.position;
			appliedPower = Vector3.Normalize(directionToTarget) * shootPower;
			appliedPower *= 0.95f;
		}
		
		bestShooter.GetComponent<Rigidbody>().AddForce(appliedPower, ForceMode.Impulse);
		
		print(bestShooter.name + " shot the ball with a power of " + appliedPower.magnitude);
		StartCoroutine(visualDebug());

		//update game summary
		if (appliedPower.magnitude > 24)
			GlobalGameManager.opponentShoots++;
		else
			GlobalGameManager.opponentPasses++;

		StartCoroutine(gameController.GetComponent<GlobalGameManager>().managePostShoot("Opponent"));
	}


    /// <summary>
    /// Draw the debug lines of AI controller in editor
    /// </summary>
    /// <returns></returns>
    IEnumerator visualDebug ()
    {
		//Visual debug
		while(!isReadyToShoot)
        {		
			//draw helper line from shooter unit to ball
			Debug.DrawLine(bestShooter.transform.position, target.transform.position, Color.green);
			
			//draw helper line which gets out of ball after direct impact
			Debug.DrawLine(target.transform.position, (target.transform.position * 2 - bestShooter.transform.position), Color.gray);
			
			//draw helper line from shooter unit to ball with ball's tangent in mind
			Debug.DrawLine(bestShooter.transform.position, target.transform.position + new Vector3(0, target.transform.localScale.z / 2, 0) , Color.red);
			Debug.DrawLine(bestShooter.transform.position, target.transform.position - new Vector3(0, target.transform.localScale.z / 2, 0) , Color.red);
			
			//draw helper line from shooter unit to ball with shooter's tangent in mind
			Debug.DrawLine(bestShooter.transform.position, target.transform.position + new Vector3(0, bestShooter.transform.localScale.z / 2, 0), Color.yellow);
			Debug.DrawLine(bestShooter.transform.position, target.transform.position - new Vector3(0, bestShooter.transform.localScale.z / 2, 0), Color.yellow);
			
			//draw helper line from shooter unit to player's gate		
			Debug.DrawLine(bestShooter.transform.position, PlayerBasketCenter.transform.position, Color.cyan);
			
			//draw helper line from ball to player's gate
			Debug.DrawLine(target.transform.position, PlayerBasketCenter.transform.position, Color.green);
			
			yield return 0;
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

}