using System;
using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BallManager : MonoBehaviour, IOnEventCallback
{
    /// <summary>
    /// Main Ball Manager.
    /// This class controls ball collision with Goal triggers and gatePoles, 
    /// and also stops the ball when the spped is too low.
    /// </summary>

    public readonly byte fixBallCode = 4;

    private GlobalGameManager gameController; //Reference to main game controller
    public AudioClip ballHitPost; //Sfx for hitting the poles

    //summary information
    private bool canCheck;
    private float threshold = 8.0f;
    private GameObject playerGate;
    private GameObject opponentGate;
    private float distanceToPlayerGate; //ball's distance to player gate
    private float distanceToOpponentGate; //ball's distance to opponent gate

    //Update v1.1 - commentator
    private GameObject commentatorManager;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GlobalGameManager>();
        playerGate = GameObject.FindGameObjectWithTag("playerGoalTrigger");
        opponentGate = GameObject.FindGameObjectWithTag("opponentGoalTrigger");
        canCheck = true;

        commentatorManager = GameObject.FindGameObjectWithTag("CommentatorManager");
    }

    void Update()
    {
        manageBallFriction();

        //get distances
        distanceToPlayerGate = Vector3.Distance(transform.position, playerGate.transform.position);
        distanceToOpponentGate = Vector3.Distance(transform.position, opponentGate.transform.position);


        //PhotonNetwork.IsConnected && !GlobalGameManager.gameIsFinished ==> checks are new - may 2020
        if (canCheck && !GlobalGameManager.goalHappened && PhotonNetwork.IsConnected && !GlobalGameManager.gameIsFinished)
            StartCoroutine(checkForShootToGoal());

        //debug
        //if(distanceToPlayerGate <= threshold || distanceToOpponentGate <= threshold)
        //	print("Ball's distanceToPlayerGate: " + distanceToPlayerGate + "\n\r" + "Ball's distanceToOpponentGate: " + distanceToOpponentGate);
    }

    void LateUpdate()
    {
        //we restrict rotation and position once again to make sure that ball won't has an unwanted effect.
        transform.position = new Vector3(transform.position.x,
            transform.position.y,
            -0.5f);

        //if you want a fixed ball with no rotation, uncomment the following line:
        //transform.rotation = Quaternion.Euler(90, 0, 0);

        moveTheBallAwayFromBorders();
    }

    private void FixedUpdate()
    {
        if (gameController.isOnline)
        {
            sendBallVelocity();
        }
    }

    /// <summary>
    /// Checks for shoot to goal event every 5 seconds
    /// </summary>
    IEnumerator checkForShootToGoal()
    {
        if (distanceToOpponentGate <= threshold && GlobalGameManager.playersTurn)
        {
            print("[LEFT] Shoot to goal happened!");
            if (gameController.isOnline)
            {
                if (gameController.isServer)
                {
                    GlobalGameManager.playerShootToGate++;
                }
                else
                {
                    GlobalGameManager.opponentShootToGate++;
                }

                updateRoomProp();
            }
            else
            {
                GlobalGameManager.playerShootToGate++;
            }

            canCheck = false;
            StartCoroutine(reactiveCheck());
            yield break;
        }

        if (distanceToPlayerGate <= threshold && GlobalGameManager.opponentsTurn)
        {
            print("[RIGHT] Shoot to goal happened!");

            if (gameController.isOnline)
            {
                if (gameController.isServer)
                {
                    GlobalGameManager.opponentShootToGate++;
                }
                else
                {
                    GlobalGameManager.playerShootToGate++;
                }
                updateRoomProp();

            }
            else
            {
                GlobalGameManager.opponentShootToGate++;
            }

            canCheck = false;
            StartCoroutine(reactiveCheck());
            yield break;
        }

    }

    /// <summary>
    /// add shoot to gate count to room properties
    /// </summary>
    private void updateRoomProp()
    {
        Hashtable hashtable = new Hashtable();

        if (gameController.GetComponent<GlobalGameManager>().isServer)
        {
            hashtable.Add("serverShootToGate", GlobalGameManager.playerShootToGate);
        }
        else
        {
            hashtable.Add("clientShootToGate", GlobalGameManager.opponentShootToGate);

        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    }


    /// <summary>
    /// Monitor ball's speed at all times.
    /// </summary>
    private float ballSpeed;
    void manageBallFriction()
    {
        ballSpeed = GetComponent<Rigidbody>().velocity.magnitude;
        //print("Ball Speed: " + rigidbody.velocity.magnitude);
        if (ballSpeed < 0.5f)
        {
            //forcestop the ball
            //rigidbody.velocity = Vector3.zero;
            //rigidbody.angularVelocity = Vector3.zero;
            GetComponent<Rigidbody>().drag = 2;
        }
        else
        {
            //let it slide
            GetComponent<Rigidbody>().drag = 0.9f;
        }
    }


    void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "gatePost":
                playSfx(ballHitPost);
                //Update v1.1
                float cmChance = UnityEngine.Random.value;
                if (cmChance >= 0.5f)
                    commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("HitPost", 1);
                else
                    commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("GoalKeeper", 1);
                break;
            case "Player":
            case "Player_2":
            case "Opponent":
                //give fake rotation to ball
                StartCoroutine(fakeRotation());
                break;
        }
    }


    bool frFlag = false;
    private int rotationSpeed = 15;
    IEnumerator fakeRotation()
    {
        if (frFlag)
            yield break;
        frFlag = true;

        float t = 0;
        while (t < 1)
        {
            if (GlobalGameManager.goalHappened)
            {
                frFlag = false;
                yield break;
            }

            //print ("fake rotation...");
            t += Time.deltaTime * 0.4f;
            float rot = rotationSpeed - (t * rotationSpeed);
            transform.Rotate(new Vector3(rot / 3, rot, rot / 3));
            yield return 0;
        }

        if (t >= 1)
        {
            frFlag = false;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "opponentGoalTrigger":
                if (gameController.isOnline)
                {
                    if (gameController.isServer)
                    {
                        StartCoroutine(gameController.managePostGoal("Player"));
                        Debug.Log("opponent trigger: online & server -> player called");
                    }
                    else
                    {
                        StartCoroutine(gameController.managePostGoal("Opponent"));
                        Debug.Log("opponent trigger: online & not server -> opponent called");
                    }
                }
                else
                {
                    StartCoroutine(gameController.managePostGoal("Player"));
                    Debug.Log("opponent trigger: offline & player called");
                }

                break;

            case "playerGoalTrigger":
                if (gameController.isOnline)
                {
                    if (gameController.isServer)
                    {
                        StartCoroutine(gameController.managePostGoal("Opponent"));
                        Debug.Log("playerTrigger : online & server -> opponent called");
                    }
                    else
                    {
                        StartCoroutine(gameController.managePostGoal("Player"));
                        Debug.Log("playerTrigger : online & not server -> player called");

                    }
                }
                else
                {
                    StartCoroutine(gameController.managePostGoal("Opponent"));
                    Debug.Log("playerTrigger : offline & opponent called");
                }
                break;
        }
    }


    /// <summary>
    /// In older versions, the ball sometimes got stuck in the edges/corners. To prevent this, we now are using a constant force parameter
    /// that moves the ball away when it gets too near to borders.
    /// </summary>
    void moveTheBallAwayFromBorders()
    {
        //bottom
        if (transform.position.y < -9.35f)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 1, 0), ForceMode.Force);
        }

        //top
        if (transform.position.y > 7.3f)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, -1, 0), ForceMode.Force);
        }

        //right
        if (transform.position.x > 14.45f && (transform.position.y > 2.7f || transform.position.y < -4.7f))
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(-1, 0, 0), ForceMode.Force);
        }

        //left
        if (transform.position.x < -14.45f && (transform.position.y > 2.7f || transform.position.y < -4.7f))
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(1, 0, 0), ForceMode.Force);
        }
    }


    void playSfx(AudioClip _clip)
    {
        GetComponent<AudioSource>().clip = _clip;
        if (!GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
        }
    }


    /// <summary>
    /// Enables to check for shoot to goal event again
    /// </summary>
    IEnumerator reactiveCheck()
    {
        yield return new WaitForSeconds(5.0f);
        canCheck = true;
    }

    /// <summary>
    /// send ball position and velocity to other client
    /// </summary>
    private void sendBallVelocity()
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

        if (gameController.myShootTurn &&
            !GlobalGameManager.goalHappened)
        {

            var xVelocity = rigidbody.velocity.x;
            var yVelocity = rigidbody.velocity.y;

            if (xVelocity > (0.05) || xVelocity < (-0.05) || yVelocity > (0.05) || yVelocity < (-0.05))
            {
                var velocity = gameObject.GetComponent<Rigidbody>().velocity;
                var position = gameObject.GetComponent<Rigidbody>().position;
                var ping = PhotonNetwork.GetPing();

                object[] data = new object[] { velocity, position, ping };

                RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
                SendOptions sendOptions = new SendOptions { Reliability = true };

                PhotonNetwork.RaiseEvent(fixBallCode, data, eventOptions, sendOptions);
            }
        }
    }

    private int fixedCounter;

    public void OnEvent(EventData photonEvent)
    {
        byte evCode = photonEvent.Code;

        if (evCode == fixBallCode)
        {
            if (!gameController.myShootTurn && fixedCounter == 1)
            {
                object[] data = (object[])photonEvent.CustomData;

                var velocity = (Vector3)data[0];
                var position = (Vector3)data[1];
                var remotePing = (int)data[2];

                var ping = PhotonNetwork.GetPing();
                var delay = (ping / 2 + remotePing / 2);

                Rigidbody myRigidbody = this.gameObject.GetComponent<Rigidbody>();

                myRigidbody.velocity = velocity;
                gameObject.transform.position = position + (velocity * (delay / 2000));
            }

            if (!gameController.myShootTurn)
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