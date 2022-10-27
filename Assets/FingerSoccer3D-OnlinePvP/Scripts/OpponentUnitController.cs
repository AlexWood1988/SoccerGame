using UnityEngine;
using System.Collections;

public class OpponentUnitController : MonoBehaviour {

    /// <summary>
    /// Unit controller class for AI units
    /// </summary>

    //3D model settings
    public GameObject modelObject;
    private Animator anim;
    public GameObject modelRig;
    public GameObject cap;
    private SkinnedMeshRenderer smr;
    private GameObject ball;
    private float moveSpeed;

    internal int unitIndex;					//every AI unit has an index. this is for the AI controller to know which unit must be selected.
											//Indexes are given to units by the AIController itself.

	private bool  canShowSelectionCircle;	//if the turn is for AI, units can show the selection circles.
	public GameObject selectionCircle;		//reference to gameObject.

	public AudioClip unitsBallHit;			//units hits the ball sfx

    //Update v1.1 - commentator
    private GameObject commentatorManager;

    void Awake ()
    {
		canShowSelectionCircle = true;

        //2021
        ball = GameObject.FindGameObjectWithTag("ball");

        //2022
        commentatorManager = GameObject.FindGameObjectWithTag("CommentatorManager");
    }

    void Start()
    {
        //2021 - get new components
        anim = modelObject.GetComponent<Animator>();
    }

	void Update ()
    {	
		if(GlobalGameManager.opponentsTurn && canShowSelectionCircle && !GlobalGameManager.goalHappened)
			selectionCircle.GetComponent<Renderer>().enabled = true;
		else	
			selectionCircle.GetComponent<Renderer>().enabled = false;

        //2021
        ManageFriction();
        FaceTowardsBall();
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
            //disable kick anim
            StartCoroutine(DisableKickAnim());
        }
    }


    IEnumerator DisableKickAnim()
    {
        yield return new WaitForSeconds(0.9f);
        anim.SetBool("CanKick", false);
    }


    void OnCollisionEnter (Collision other)
    {
		switch(other.gameObject.tag) 
        {
		    case "ball":
			    PlaySfx(unitsBallHit);

                //Update v1.1
                //play voice - good shoot
                if (GlobalGameManager.opponentsTurn && moveSpeed >= 10)
                    commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("GoodShoot", 0);

                //play voice - bad shoot
                if (GlobalGameManager.opponentsTurn && moveSpeed < 10)
                    commentatorManager.GetComponent<CommentatorManager>().PlayVoiceEvent("BadShoot", 0);

                break;
		}
	}
	

	void PlaySfx (AudioClip _clip)
    {
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}
}