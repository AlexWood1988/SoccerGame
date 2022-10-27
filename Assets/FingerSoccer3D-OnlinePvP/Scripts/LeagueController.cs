#pragma warning disable 414

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LeagueController : MonoBehaviour
{
    /// <summary>
    /// Main league selection class.
    /// </summary>

    public AudioClip tapSfx;                //tap sfx
    public AudioClip coinsCheckout;         //buy sfx
    public AudioClip notEnoughMoney;
    public Text playerMoney;                //UI text
    private int availableMoney;


    void Awake()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        //Updates UI text with saved values fetched from playerprefs
        availableMoney = PlayerPrefs.GetInt("PlayerMoney");
        playerMoney.text = "" + availableMoney;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Config-c#");
    }


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
                case "LeagueButton":

                    //if we have enough money, we can enter this league
                    if (availableMoney >= objectHit.GetComponent<LeagueProperties>().leagueEntryfee)
                    {
                        //deduct the price from user money
                        availableMoney -= objectHit.GetComponent<LeagueProperties>().leagueEntryfee;

                        //save new amount of money
                        PlayerPrefs.SetInt("PlayerMoney", availableMoney);

                        //save selected league's setting into playerprefs
                        PlayerPrefs.SetInt("IsLeague", 1);
                        PlayerPrefs.SetInt("LeaguePrize", objectHit.GetComponent<LeagueProperties>().leaguePrize);
                        PlayerPrefs.SetInt("LeagueRuleID", objectHit.GetComponent<LeagueProperties>().leagueID);
                        PlayerPrefs.SetInt("LeagueTime", (int)LeagueManager.GetLeaguesSettings(objectHit.GetComponent<LeagueProperties>().leagueID).z);

                        //set a unique stadium for each league
                        PlayerPrefs.SetInt("GameStadium", objectHit.GetComponent<LeagueProperties>().leagueID);

                        //play sfx
                        playSfx(tapSfx);

                        //Wait
                        yield return new WaitForSeconds(0.25f);

                        //play sfx
                        playSfx(coinsCheckout);

                        //Wait
                        yield return new WaitForSeconds(1.0f);

                        //Load the game
                        SceneManager.LoadScene("Game-c#");
                    }
                    else
                    {
                        //if our money is not enough to enter this league
                        playSfx(notEnoughMoney);
                    }
                    break;
            }
        }
    }


    public void GoBack()
    {
        StartCoroutine(GoBackCo());
    }

    public IEnumerator GoBackCo()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Config-c#");
    }


    public void LoadLeague(LeagueProperties lp)
    {
        StartCoroutine(LoadLeagueCo(lp));
    }

    public IEnumerator LoadLeagueCo(LeagueProperties lp)
    {
        //debug
        print("LeagueProperties ID: " + lp.leagueID);
        print("LeagueProperties EntryFee: " + lp.leagueEntryfee);
        print("LeagueProperties Prize: " + lp.leaguePrize);
        print("LeagueProperties Duration: " + lp.leagueDuration);

        //if we have enough money, load the league
        if (availableMoney >= lp.leagueEntryfee)
        {
            //play sfx
            playSfx(tapSfx);
            yield return new WaitForSeconds(0.25f);

            //deduct coins from player balance
            StartCoroutine(DeductEntryFee(availableMoney, lp.leagueEntryfee));

            //deduct the price from user money
            availableMoney -= lp.leagueEntryfee;

            //save new amount of money
            PlayerPrefs.SetInt("PlayerMoney", availableMoney);

            //save the setting
            PlayerPrefs.SetInt("IsLeague", 1);
            PlayerPrefs.SetInt("LeaguePrize", lp.leaguePrize);
            PlayerPrefs.SetInt("LeagueID", lp.leagueID);
            PlayerPrefs.SetInt("LeagueDuration", lp.leagueDuration);

            //play sfx
            playSfx(coinsCheckout);
            yield return new WaitForSeconds(1.25f);

            //Load the game
            SceneManager.LoadScene("Game-c#");
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


    public IEnumerator DeductEntryFee(float balance, float fee)
    {
        float to = balance - fee;
        float t = 0;
        while(t < 1)
        {
            t += Time.deltaTime;
            playerMoney.text = "" + (int)Mathf.SmoothStep(balance, to, t);
            yield return 0;
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
        SceneManager.LoadScene("Menu-c#");
    }

}