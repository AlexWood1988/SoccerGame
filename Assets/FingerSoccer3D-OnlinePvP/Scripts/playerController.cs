#pragma warning disable 0219

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Unity.Collections;

public class playerController : MonoBehaviour,IOnEventCallback
{
    /// <summary>
    /// Main Player Controller.
    /// This class manages the shooting process of human players.
    /// This also handles the rendering of debug lines in editor.
    /// </summary>

    //Static
    public static GameObject selectedUnit;

    //3D model settings
    public GameObject modelObject;
    private Animator anim;
    public GameObject modelRig;
    public GameObject cap;
    public GameObject cameraPosition;
    private SkinnedMeshRenderer smr;
    private GameObject ball;
    internal float moveSpeed;
    private float helperArrowDeselectLimit = 1.15f;

    // Public Variables
    public int unitIndex;				//This unit's ID (given automatically by PlayerAI class)
	public GameObject selectionCircle;	//Reference to gameObject

	public GameObject collisionChecker;	//Collision checker prefab
    private Vector3 collisioFreePosition;

    public bool isInsideTheGate;

	//Referenced GameObjects
	private GameObject helperBegin; 	//Start helper
	private GameObject helperEnd; 		//End Helper
	private GameObject arrowPlane; 		//arrow plane which is used to show shotPower
	private GameObject shootCircle;		//shoot Circle plane which is used to show shotPower

	private GameObject aimPivot;		//starting point for aim arrow
	private GameObject aimArrow;		//arrow plane which is used to show aim precision (the holder)
	private GameObject aimArrowBody;	//arrow plane which is used to show aim precision

	private GameObject gameController;	//Reference to main game controller
	private float currentDistance;		//real distance of our touch/mouse position from initial drag position
	private float safeDistance; 			//A safe distance value which is always between min and max to avoid supershoots

	private float pwr;					//shoot power

	//this vector holds shooting direction
	private Vector3 shootDirectionVector;

	//prevent player to shoot twice in a round
	public static bool canShoot;
	internal float shootTime;
	private int timeAllowedToShoot = 10000; //In Seconds (in this kit we give players unlimited time to perform their turn of shooting)

	private bool is_online = false;

	public readonly byte unit_pos_code = 5; //raise event code for fix unit position


	private void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}


	private void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}


	void Awake ()
    {
		//Find and cache important gameObjects
		helperBegin = GameObject.FindGameObjectWithTag("mouseHelperBegin");
		helperEnd = GameObject.FindGameObjectWithTag("mouseHelperEnd");
		arrowPlane = GameObject.FindGameObjectWithTag("helperArrow");
		gameController = GameObject.FindGameObjectWithTag("GameController");
		shootCircle = GameObject.FindGameObjectWithTag("shootCircle");

		aimArrow = GameObject.FindGameObjectWithTag("aimArrow");	
		aimArrowBody = GameObject.FindGameObjectWithTag("aimArrowBody");
		aimPivot = GameObject.FindGameObjectWithTag("aimPivot");
		
		//Init Variables
		pwr = 0.1f;
		currentDistance = 0;
		shootDirectionVector = new Vector3(0,0,0);
		canShoot = true;
		shootTime = timeAllowedToShoot;
		arrowPlane.GetComponent<Renderer>().enabled = false;	 //hide arrowPlane
		shootCircle.GetComponent<Renderer>().enabled = false;	 //hide shoot Circle
		aimArrowBody.GetComponent<Renderer>().enabled = false;   //hide aim arrow

        isInsideTheGate = false;
        collisioFreePosition = new Vector3(0, 0, 0);

        //2021
        ball = GameObject.FindGameObjectWithTag("ball");
        selectedUnit = null;
    }

	void Start ()
	{
        //2021 - get new components
        anim = modelObject.GetComponent<Animator>();

        if (GlobalGameManager.gameMode == 3)
		{
			is_online = true;
		}
	}


	void Update ()
	{
		//Active the selection circles around Player units when they have the turn.
		if(GlobalGameManager.playersTurn && gameObject.tag == "Player" && !GlobalGameManager.goalHappened)
			selectionCircle.GetComponent<Renderer>().enabled = true;
		else if(GlobalGameManager.opponentsTurn && gameObject.tag == "Player_2" && !GlobalGameManager.goalHappened)
			selectionCircle.GetComponent<Renderer>().enabled = true;			
		else	
			selectionCircle.GetComponent<Renderer>().enabled = false;

        //2021
        ManageFriction();
        FaceTowardsBall();
    }


    void LateUpdate()
    {
        //check if a unit is inside a gate (which in that case we need to move it out)
        //if (is_online && gameController.GetComponent<GlobalGameManager>().myShootTurn)
            PreventStuckInsideGates();
    }


    void ManageFriction()
    {
        moveSpeed = GetComponent<Rigidbody>().velocity.magnitude;
        anim.SetFloat("MoveSpeed", moveSpeed);
        //print(gameObject.name + " Speed: " + moveSpeed);

        if (moveSpeed < 0.5f)
        {
            GetComponent<Rigidbody>().drag = 2;
        }
        else
        {
            //let it slide
            GetComponent<Rigidbody>().drag = 0.7f;
        }

        float distanceToBall = Vector3.Distance(ball.transform.position, transform.position);
        if (distanceToBall <= 3 && moveSpeed >= 5)
        {
            //enable kick anim
            anim.SetBool("CanKick", true);
            //anim.SetBool("CanTackle", true);
            //disable kick anim
            StartCoroutine(DisableKickAnim());
        }
    }


    IEnumerator DisableKickAnim()
    {
        yield return new WaitForSeconds(0.9f);
        anim.SetBool("CanKick", false);
        //anim.SetBool("CanTackle", false);
    }


    /// <summary>
    /// Make the player face towards the ball at all times
    /// </summary>
    void FaceTowardsBall()
    {
        //calculate rotation
        Vector3 dir = transform.position - ball.transform.position;
        float outRotation; // between 0 - 360
        if (Vector3.Angle(dir, transform.forward) > 90)
            outRotation = Vector3.Angle(dir, transform.right);
        else
            outRotation = Vector3.Angle(dir, transform.right) * -1;
        //3d - rotate player body
        modelObject.transform.localEulerAngles = new Vector3(-180, outRotation + 90, 0);
    }

    /// <summary>
    /// Check if this unit is inside a gate, and in that case move it out to a feww position on field.
    /// </summary>
    public void PreventStuckInsideGates()
    {
        float unitSpeed = GetComponent<Rigidbody>().velocity.magnitude;
        //print(gameObject.name + " speed is: " + unitSpeed);
        
        if(unitSpeed > 0.05f)
        {
            isInsideTheGate = false;
            return;
        }            

        if ((transform.position.y < 1f && transform.position.y > -2.8f) && (transform.position.x < -15.3f || transform.position.x > 15.3f))
        {
            //print(gameObject.name + " is inside the gate.");
            isInsideTheGate = true;

        } else
        {
            isInsideTheGate = false;
        }

        //move the player out , only if we are not already moving it out
        if(isInsideTheGate && GetComponent<MeshCollider>().enabled)
        {
            StartCoroutine(FindCollisionFreePosition());
        }
    }


    public IEnumerator FindCollisionFreePosition()
    {
        Vector3 freePos = new Vector3(0, 0, 0);
        GetComponent<MeshCollider>().enabled = false;

        //j:x
        //i:y
        for (int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                Vector3 randomLocation = new Vector3( (-14.5f + (j/2f)) * -Mathf.Sign(transform.position.x), (-1.0f + (i/2f)), -0.5f);
                print("randomLocation: " + randomLocation);

                GameObject tmpCC = Instantiate(collisionChecker, randomLocation, Quaternion.Euler(90, 0, 0)) as GameObject;
                tmpCC.name = "CC-" + i + "-" + j;

                yield return new WaitForSeconds(0.05f);

                //if (!tmpCC.GetComponent<CollisionChecker>().isInCollision)
                if (!tmpCC.transform.GetChild(0).GetComponent<CollisionChecker>().isInCollision)
                {
                    freePos = tmpCC.transform.position;
                    print(i + " - we have found a free position: " + freePos);

                    //tmpCC.GetComponent<MeshCollider>().enabled = false;
                    tmpCC.transform.GetChild(0).GetComponent<MeshCollider>().enabled = false;

                    Destroy(tmpCC);
                    collisioFreePosition = freePos;
                    StartCoroutine(MoveUnitToFreePosition(collisioFreePosition));
                    yield break;
                }
                else
                {
                    Destroy(tmpCC);
                }
            }
        }
    }


    public IEnumerator MoveUnitToFreePosition(Vector3 _targetPos)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<MeshCollider>().enabled = false;

        Vector3 cPos = transform.position;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 1f;
            transform.position = new Vector3(Mathf.SmoothStep(cPos.x, _targetPos.x, t),
                                             Mathf.SmoothStep(cPos.y, _targetPos.y, t),
                                             -0.5f);
            yield return 0;
        }

        if(t >= 1)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<MeshCollider>().enabled = true;
            yield break;
        }
    }


    private void FixedUpdate()
	{
		sendUnitVelocity();
	}


    /// <summary>
    /// Works fine with mouse and touch
    /// This is the main functiuon used to manage drag on units, calculating the power and debug vectors, and set the final parameters to shoot.
    /// </summary>
    void OnMouseDrag ()
	{		
		if( canShoot && ((GlobalGameManager.playersTurn && gameObject.tag == "Player") || (GlobalGameManager.opponentsTurn && gameObject.tag == "Player_2")) )
		{
            //print("Draged");

            //set this unit as selected
            SetSelectedUnit(this.gameObject);

            currentDistance = Vector3.Distance(helperBegin.transform.position, transform.position);
				
			//limiters
			if(currentDistance <= GlobalGameManager.maxDistance)
				safeDistance = currentDistance;
			else
				safeDistance = GlobalGameManager.maxDistance;
					
			pwr = Mathf.Abs(safeDistance) * 9; //this is very important. change with extreme caution.

            //Make player body transparent
            //Old way
            //modelRig.GetComponent<Renderer>().materials[0].color = new Color(1, 1, 1, 0.1f);
            //modelRig.GetComponent<Renderer>().materials[1].color = new Color(1, 1, 1, 0.1f);
            //modelRig.GetComponent<Renderer>().materials[2].color = new Color(1, 1, 1, 0.1f);
            //New way
            int modelMaterialCount = modelRig.GetComponent<Renderer>().materials.Length;
            for (int i = 0; i < modelMaterialCount; i++)
            {
                modelRig.GetComponent<Renderer>().materials[i].color = new Color(1, 1, 1, 0.1f);
            }

			//show the power arrow above the unit and scale is accordingly.
			manageArrowTransform();		
				
			//position of helperEnd
			//HelperEnd is the exact opposite (mirrored) version of our helperBegin object 
			//and help us to calculate debug vectors and lines for a perfect shoot.
			//Please refer to the basic geometry references of your choice to understand the math.
			Vector3 dxy = helperBegin.transform.position - transform.position;
			float diff = dxy.magnitude;
			helperEnd.transform.position = transform.position + ((dxy / diff) * currentDistance * -1);
				
			helperEnd.transform.position = new Vector3( helperEnd.transform.position.x,
					                                    helperEnd.transform.position.y,
					                                    -0.5f);				
				
			//debug line from initial position to our current touch position
			Debug.DrawLine(transform.position, helperBegin.transform.position, Color.red);
			//debug line from initial position to maximum power position (mirrored)
			Debug.DrawLine(transform.position, arrowPlane.transform.position, Color.blue);
			//debug line from initial position to the exact opposite position (mirrored) of our current touch position
			Debug.DrawLine(transform.position, (2 * transform.position) - helperBegin.transform.position, Color.yellow);
			//cast ray forward and collect informations
			castRay();
								
			//final vector used to shoot the unit.
			shootDirectionVector = Vector3.Normalize(helperBegin.transform.position - transform.position);
			//print(shootDirectionVector);
		}
	}


    /// <summary>
    /// Cast a ray forward and collect informations like if it hits anything...
    /// </summary>
    private RaycastHit hitInfo;
	private Ray ray;
	void castRay ()
    {		
		//cast the ray from units position with a normalized direction out of it which is mirrored to our current drag vector.
		ray = new Ray(transform.position, (helperEnd.transform.position - transform.position).normalized );
			
		if(Physics.Raycast(ray, out hitInfo, currentDistance)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			
			//debug line whenever the ray hits something.
			Debug.DrawLine(ray.origin, hitInfo.point, Color.cyan);
					
			//draw reflected vector like a billiard game. this is the out vector which reflects from targets geometry.
			Vector3 reflectedVector = Vector3.Reflect( (hitInfo.point - ray.origin),  hitInfo.normal );
			Debug.DrawRay(hitInfo.point, reflectedVector , Color.gray, 0.2f);
			
			//draw inverted reflected vector (useful for fine-tuning the final shoot)
			Debug.DrawRay(hitInfo.transform.position, reflectedVector * -1, Color.white, 0.2f);
			
			//draw the inverted normal which is more likely to be similar to real world response.
			Debug.DrawRay(hitInfo.transform.position, hitInfo.normal * -3, Color.red, 0.2f);
			
			//Debug
			//print("Ray hits: " + objectHit.name + " At " + Time.time + " And Reflection is: " + reflectedVector);
		}
	}


    /// <summary>
    /// Unhide and process the transform and scale of the power Arrow object
    /// </summary>
    void manageArrowTransform ()
    {
		if(currentDistance < helperArrowDeselectLimit)
        {
			arrowPlane.GetComponent<Renderer>().enabled = false;
			shootCircle.GetComponent<Renderer>().enabled = false;
			aimArrowBody.GetComponent<Renderer>().enabled = false;
			return;
		}

		//power arrow codes
		arrowPlane.GetComponent<Renderer>().enabled = true;
		shootCircle.GetComponent<Renderer>().enabled = true;
		aimArrowBody.GetComponent<Renderer>().enabled = true;
		
		//calculate position
        arrowPlane.transform.position = transform.position + new Vector3(0, 0, 0.03f);
        shootCircle.transform.position = transform.position + new Vector3(0, 0, 0.05f);

		//calculate rotation
		Vector3 dir = helperBegin.transform.position - transform.position;
		float outRotation; // between 0 - 360

		if(Vector3.Angle(dir, transform.forward) > 90) 
			outRotation = Vector3.Angle(dir, transform.right);
		else
			outRotation = Vector3.Angle(dir, transform.right) * -1;

        //2021 - we need to reverse the outRot vector direction
        outRotation += 180;

		arrowPlane.transform.eulerAngles = new Vector3(0, 0, outRotation);
		//print(Vector3.Angle(dir, transform.forward));
		
		//calculate scale
		float scaleCoefX = Mathf.Log(1 + safeDistance/2, 2) * 2.2f;
		float scaleCoefY = Mathf.Log(1 + safeDistance/2, 2) * 2.2f;

		//arrowPlane.transform.localScale = new Vector3(1 + scaleCoefX, 1 + scaleCoefY, 0.001f); //default scale
		arrowPlane.transform.localScale = new Vector3(1 + scaleCoefX * 3, 1 + scaleCoefY * 3, 0.001f); //default scale

		aimArrow.transform.position = aimPivot.transform.position + new Vector3(0, 0, 0);
		aimArrow.transform.eulerAngles = new Vector3(0, 0, outRotation);

        //int teamID = PlayerPrefs.GetInt ("PlayerFlag");
        //aimArrow.transform.localScale = new Vector3(1.5f + (TeamsManager.getTeamSettings(teamID).z / 2.5f), 1.5f + (TeamsManager.getTeamSettings(teamID).z / 2.5f), 0.01f) * -1;

        if (GlobalGameManager.playersTurn && gameObject.tag == "Player")
        {
            aimArrow.transform.localScale = new Vector3(
                1.5f + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("PlayerFlag")].teamAttributes.z / 2.5f),
                1.5f + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("PlayerFlag")].teamAttributes.z / 2.5f),
                0.01f) * -1;
        }
        else if (GlobalGameManager.opponentsTurn && gameObject.tag == "Player_2")
        {
            aimArrow.transform.localScale = new Vector3(
                1.5f + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("Player2Flag")].teamAttributes.z / 2.5f),
                1.5f + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("Player2Flag")].teamAttributes.z / 2.5f),
                0.01f) * -1;
        }

        shootCircle.transform.localScale = new Vector3(1 + scaleCoefX * 3, 1 + scaleCoefY * 3, 0.001f); //default scale

		if (is_online)
		{
			raiseShowArrowEvent(arrowPlane.transform.position,arrowPlane.transform.eulerAngles,
				arrowPlane.transform.localScale,shootCircle.transform.position,shootCircle.transform.localScale
				,aimArrow.transform.eulerAngles,aimArrow.transform.localScale);
		}
	}


    /// <summary>
    /// Actual shoot fucntion
    /// </summary>
    void OnMouseUp ()
	{
        //2021 - Unset selected unit
        SetSelectedUnit();

        //2021 - Make player body solid color
        //Old way
        //modelRig.GetComponent<Renderer>().materials[0].color = new Color(1, 1, 1, 1f);
        //modelRig.GetComponent<Renderer>().materials[1].color = new Color(1, 1, 1, 1f);
        //modelRig.GetComponent<Renderer>().materials[2].color = new Color(1, 1, 1, 1f);
        //New way
        int modelMaterialCount = modelRig.GetComponent<Renderer>().materials.Length;
        for (int i = 0; i < modelMaterialCount; i++)
        {
            modelRig.GetComponent<Renderer>().materials[i].color = new Color(1, 1, 1, 1f);
        }

        //Special checks for 2-player game
        if ( ((GlobalGameManager.playersTurn && gameObject.tag == "Player_2") || (GlobalGameManager.opponentsTurn && gameObject.tag == "Player") ) && PlayerPrefs.GetInt("IsCrazyMode") == 0)
        {
			arrowPlane.GetComponent<Renderer>().enabled = false;
			shootCircle.GetComponent<Renderer>().enabled = false;
			aimArrowBody.GetComponent<Renderer>().enabled = false;

			if (is_online)
			{
				raiseHideArrowEvent();
			}

            print("<b>Exit here...</b>");
			return;
		}
		
		//give the player a second chance to choose another ball if drag on the unit is too low
		print("currentDistance: " + currentDistance);
		if(currentDistance < helperArrowDeselectLimit)
        {
			arrowPlane.GetComponent<Renderer>().enabled = false;
			shootCircle.GetComponent<Renderer>().enabled = false;
			aimArrowBody.GetComponent<Renderer>().enabled = false;
			
			if (is_online)
			{
				raiseHideArrowEvent(); //hide for online player
			}

			return;
		}

        //But if player wants to shoot anyway:
        //prevent double shooting in a round
        if (!canShoot)
        {
			return;
        }			
		
		//no more shooting is possible	
		canShoot = false;
		
		//keep track of elapsed time after letting the ball go, 
		//so we can findout if ball has stopped and the round should be changed
		//this is the time which user released the button and shooted the ball
		shootTime = Time.time;
		
		//hide helper arrow object
		arrowPlane.GetComponent<Renderer>().enabled = false;
		shootCircle.GetComponent<Renderer>().enabled = false;
		aimArrowBody.GetComponent<Renderer>().enabled = false;

		if (is_online)
		{
			raiseHideArrowEvent(); //hide for online player
		}

		//do the physics calculations and shoot the ball 
		Vector3 outPower = shootDirectionVector * pwr * 1;	

		if (GlobalGameManager.playersTurn && gameObject.tag == "Player")
		{
			outPower *= (1.15f + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("PlayerFlag")].teamAttributes.x / 35.0f));
			//print ("[P1] NEW outPower: " + outPower.magnitude);

		} else if (GlobalGameManager.opponentsTurn && gameObject.tag == "Player_2") {
			outPower *= (1.15f + (GlobalGameSettings.instance.availableTeams[PlayerPrefs.GetInt("Player2Flag")].teamAttributes.x / 35.0f));
			//print ("[P2] NEW outPower: " + outPower.magnitude);
		}
		
		
		//Avoid shoot power over 40, or the ball might fly off of the level bounds.
		if(outPower.magnitude >= 40)
			outPower *= 0.9f;		
		
		//always make the player to move only in x-y plane and not on the z direction
		print("shoot power: " + outPower.magnitude);
		
		GetComponent<Rigidbody>().AddForce(outPower, ForceMode.Impulse);

		//Update game summary
		//Please notice that both player1 and player2 (human players) are using this class to shoot their units.
		//so we need to distinguish each one when we want to monitor their stats
		if (GlobalGameManager.gameMode == 0)
        {			
			//for player vs AI mode
			if (outPower.magnitude > 24)
				GlobalGameManager.playerShoots++;
			else
				GlobalGameManager.playerPasses++;
			
		}
        else if(GlobalGameManager.gameMode == 1 || GlobalGameManager.gameMode == 3)
        {			
			//for player1 vs player2 mode or online mode
			if (outPower.magnitude > 24)
            {
				if (gameObject.tag == "Player")
				{
					GlobalGameManager.playerShoots++;
				}
				else if(gameObject.tag == "Player_2")
					GlobalGameManager.opponentShoots++;
			} 
			else
            {
				if (gameObject.tag == "Player")
				{
					GlobalGameManager.playerPasses++;
				}
				else if(gameObject.tag == "Player_2")
					GlobalGameManager.opponentPasses++;
			}
			
			//update online match player summary
			if (is_online)
				updateOnlineSummary();			
		}
		//End - Update game summary
		
		//change the turn
		if(GlobalGameManager.gameMode == 0)
			StartCoroutine(gameController.GetComponent<GlobalGameManager>().managePostShoot("Player"));
		else if(GlobalGameManager.gameMode == 1 || GlobalGameManager.gameMode == 3)
			StartCoroutine(gameController.GetComponent<GlobalGameManager>().managePostShoot(gameObject.tag));

        if (gameController.GetComponent<GlobalGameManager>().isOnline)
        {
            gameController.GetComponent<GlobalGameManager>().sendPostShootEvent("Player_2");
        }
    }


    public void SetSelectedUnit(GameObject _g = null)
    {
        if (_g == null)
            selectedUnit = null;
        else
            selectedUnit = _g;

        //print("Global selected unit: " + selectedUnit);
    }


	private void raiseShowArrowEvent(Vector3 arrowPlane_pos,Vector3 arrowPlane_angle,Vector3 arrowPlane_scale,
		Vector3 circle_pos,Vector3 circle_scale,Vector3 aimArrow_angle,Vector3 aimArrow_scale)
	{
		var evCode = GameObject.FindGameObjectWithTag("playerAI").GetComponent<PlayerAI>().showArrowCode;
		
		object[] content = new object[]
		{
			arrowPlane_pos,arrowPlane_angle,arrowPlane_scale,circle_pos,circle_scale,aimArrow_angle,aimArrow_scale
		};

		RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
		SendOptions sendOptions = new SendOptions{Reliability = true};

		PhotonNetwork.RaiseEvent(evCode, content, eventOptions, sendOptions);
	}


	private void raiseHideArrowEvent()
	{
		var evCode = GameObject.FindGameObjectWithTag("playerAI").GetComponent<PlayerAI>().hideArrowCode;
		
		RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others};
		SendOptions sendOptions = new SendOptions{Reliability = true};

		PhotonNetwork.RaiseEvent(evCode, null, eventOptions, sendOptions);
	}


    private void sendUnitVelocity()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        if (is_online && gameController.GetComponent<GlobalGameManager>().myShootTurn)
        {
            var xVelocity = rigidbody.velocity.x;
            var yVelocity = rigidbody.velocity.y;

            if (xVelocity > (0.05) || xVelocity < (-0.05) || yVelocity > (0.05) || yVelocity < (-0.05))
            {
                var velocity = rigidbody.velocity;
                var position = gameObject.transform.position;
                var ping = PhotonNetwork.GetPing();

                object[] data = new object[] { velocity, position, gameObject.name, ping };


                RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
                SendOptions sendOptions = new SendOptions { Reliability = true };

                PhotonNetwork.RaiseEvent(unit_pos_code, data, eventOptions, sendOptions);
            }
        }
    }


    private void updateOnlineSummary()
	{
		Hashtable hashtable = new Hashtable();

		if (gameController.GetComponent<GlobalGameManager>().isServer)
		{
			hashtable.Add("serverPassCount",GlobalGameManager.playerPasses);
			hashtable.Add("serverShootCount",GlobalGameManager.playerShoots);
		}
		else
		{
			hashtable.Add("clientPassCount",GlobalGameManager.playerPasses);
			hashtable.Add("clientShootCount",GlobalGameManager.playerShoots);
		}
				
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
	}


    private int fixedCounter;
    public void OnEvent(EventData photonEvent)
    {
        var evCode = photonEvent.Code;

        if (evCode == unit_pos_code)
        {
            if (fixedCounter == 1)
            {
                object[] data = (object[])photonEvent.CustomData;

                var velocity = (Vector3)data[0];
                var position = (Vector3)data[1];
                var coming_name = data[2];
                var remotePing = (int)data[3];

                var ping = PhotonNetwork.GetPing();
                var delay = (ping / 2 + remotePing / 2);

                if (coming_name.ToString().Contains("PlayerUnit"))
                {
                    coming_name = coming_name.ToString().Replace("PlayerUnit", "Player2Unit");
                }
                else if (coming_name.ToString().Contains("Player2Unit"))
                {
                    coming_name = coming_name.ToString().Replace("Player2Unit", "PlayerUnit");
                }

                GameObject unit = GameObject.Find((string)coming_name);

                Rigidbody myRigidbody = unit.GetComponent<Rigidbody>();

                myRigidbody.velocity = velocity;
                unit.transform.position = position + (velocity * (delay / 2000));
            }

            if (!gameController.GetComponent<GlobalGameManager>().myShootTurn)
            {
                fixedCounter++;
            }

            if (fixedCounter > 10)
            {
                fixedCounter = 0;
            }
        }
    }

}