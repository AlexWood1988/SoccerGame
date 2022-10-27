#pragma warning disable 414

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ShopController : MonoBehaviour
{
    /// <summary>
    /// Main Shop Controller class.
    /// This class handles all touch events on buttons.
    /// It also checks if user has enough money to buy items, it items are already purchased,
    /// and saves the purchased items into playerprefs for further usage.
    /// </summary>

    public GameObject buyButtonPrefab;
    private int startingX = -11;
    private GameObject shopButtonHolder;
    private bool canSlide;
    private float touchDeltaPosition;
    private float scrollSpeedModifier = 4.5f;

    private float buttonAnimationSpeed = 9; //speed on animation effect when tapped on button
    private bool canTap = true;             //flag to prevent double tap
    public AudioClip tapSfx;                //tap sfx

    public AudioClip coinsCheckout;         //buy sfx
    public Text playerMoney;          //Reference to 3d text
    private int availableMoney;

    //money animation
    private bool runOnceFlag;
    public static bool runMoneyAnim;    //anim flag
    public static int pm;               //previous money
    public static int nm;               //new money


    void Awake()
    {
        runMoneyAnim = false;
        runOnceFlag = false;
        pm = 0;
        nm = 0;
        canSlide = true;
        shopButtonHolder = GameObject.FindGameObjectWithTag("ShopButtonHolder");

        //Updates UI text with saved values fetched from playerprefs
        availableMoney = PlayerPrefs.GetInt("PlayerMoney");
        playerMoney.text = "" + availableMoney;

        //create #N team buy button
        for (int i = 0; i < GlobalGameSettings.instance.availableTeams.Length; i++)
        {
            GameObject buyButton = Instantiate(buyButtonPrefab, new Vector3(startingX + (i * 7), -4f, -1), Quaternion.Euler(0, 0, 0)) as GameObject;
            buyButton.name = "BuyButton-" + i.ToString();
            buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().isInUse = 0;
            buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().itemIndex = 1000 + i;
            buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().itemPrice = GlobalGameSettings.instance.availableTeams[i].teamPrice;
            buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().flagMat = GlobalGameSettings.instance.availableTeams[i].teamMaterial;
            buyButton.transform.parent = shopButtonHolder.transform;

            //check if we previously purchased these items.
            if (PlayerPrefs.GetInt("shopTeam-" + i.ToString()) == 1)
            {
                //we already purchased this item
                buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().isInUse = 1;
                buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().itemPrice = 0;
                buyButton.transform.GetChild(0).GetComponent<ShopItemProperties> ().buyLabelUI.GetComponent<TextMesh>().text = "Purchased";
                buyButton.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;	//Not clickable anymore
            }

            //check if this team is open by default
            if (GlobalGameSettings.instance.availableTeams[i].teamAttributes.w == 1)
            {
                //we already purchased this item
                buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().isInUse = 1;
                buyButton.transform.GetChild(0).GetComponent<ShopItemProperties>().itemPrice = 0;
                buyButton.transform.GetChild(0).GetComponent<ShopItemProperties> ().buyLabelUI.GetComponent<TextMesh>().text = "Purchased";
                buyButton.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;	//Not clickable anymore
            }
        }
    }


    void Update()
    {
        slideManager();

        if (canTap)
        {
            StartCoroutine(tapManager());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Shop-c#");
    }


    void LateUpdate()
    {
        calculateMouseDelta();
    }


    /// <summary>
    /// Get and set last input position from user and guess the direction of slide
    /// </summary>
    private Vector3 mouseDelta = Vector3.zero;
    private Vector3 mouseLastPos = Vector3.zero;
    void calculateMouseDelta()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseLastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            mouseDelta = Input.mousePosition - mouseLastPos;
            mouseLastPos = Input.mousePosition;
        }
    }


    /// <summary>
    /// This function monitors player touches on menu buttons.
    /// detects both touch and clicks and can be used with editor, handheld device and 
    /// every other platforms at once.
    /// </summary>
    private RaycastHit hitInfo;
    private Ray ray;
    private string saveName = "";
    IEnumerator tapManager()
    {
        //Mouse or touch?
        if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)
            ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
        else if (Input.GetMouseButtonUp(0))
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
            yield break;

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject objectHit = hitInfo.transform.gameObject;
            switch (objectHit.tag)
            {
                case "buyButton":
                    //if we have enough money, purchase this item and save the event
                    if (availableMoney >= objectHit.GetComponent<ShopItemProperties>().itemPrice)
                    {
                        //animate the button
                        StartCoroutine(animateButton(objectHit));

                        //animate money
                        pm = availableMoney;
                        nm = availableMoney - objectHit.GetComponent<ShopItemProperties>().itemPrice;
                        StartCoroutine(animateCoin(pm, nm));

                        //deduct the price from user money
                        availableMoney -= objectHit.GetComponent<ShopItemProperties>().itemPrice;

                        //save new amount of money
                        PlayerPrefs.SetInt("PlayerMoney", availableMoney);

                        //save the event of purchase
                        saveName = "shopTeam-" + (objectHit.GetComponent<ShopItemProperties>().itemIndex - 1000).ToString();
                        PlayerPrefs.SetInt(saveName, 1);

                        //play sfx
                        playSfx(coinsCheckout);

                        //Wait
                        yield return new WaitForSeconds(1.5f);

                        //Reload the level
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                    break;
            }
        }
    }


    IEnumerator animateButton(GameObject _btn)
    {
        canTap = false;
        Vector3 startingScale = _btn.transform.localScale;  //initial scale	
        Vector3 destinationScale = startingScale * 0.85f;       //target scale

        //Scale up
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime * buttonAnimationSpeed;
            _btn.transform.localScale = new Vector3(Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
                                                    Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
                                                    _btn.transform.localScale.z);
            yield return 0;
        }

        //Scale down
        float r = 0.0f;
        if (_btn.transform.localScale.x >= destinationScale.x)
        {
            while (r <= 1.0f)
            {
                r += Time.deltaTime * buttonAnimationSpeed;
                _btn.transform.localScale = new Vector3(Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
                                                        Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
                                                        _btn.transform.localScale.z);
                yield return 0;
            }
        }

        if (r >= 1)
            canTap = true;
    }


    void slideManager()
    {
        if (Input.GetMouseButton(0) && canSlide)
        {
            touchDeltaPosition = mouseDelta.x;
            shopButtonHolder.transform.position += new Vector3((touchDeltaPosition * scrollSpeedModifier) * Time.deltaTime, 0, 0);
        }

        if (shopButtonHolder.transform.position.x >= 0)
            shopButtonHolder.transform.position = new Vector3(0, 0, 0);

        if (shopButtonHolder.transform.position.x <= -7 * (GlobalGameSettings.instance.availableTeams.Length - 4))
            shopButtonHolder.transform.position = new Vector3(-7 * (GlobalGameSettings.instance.availableTeams.Length - 4), 0, 0);

    }


    public IEnumerator animateCoin(int from, int to)
    {
        runOnceFlag = true;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 0.95f;
            playerMoney.text = "" + (int)Mathf.SmoothStep(from, to, t);
            yield return 0;
        }

        if (t >= 1)
        {
            yield return new WaitForSeconds(0.25f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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


    public void ClickOnBackButton()
    {
        StartCoroutine(ClickOnBackButtonCo());
    }

    public IEnumerator ClickOnBackButtonCo()
    {
        playSfx(tapSfx);
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Shop-c#");
    }


    public void ClickOnPlusButton()
    {
        SceneManager.LoadScene("BuyCoinPack-c#");
    }

}