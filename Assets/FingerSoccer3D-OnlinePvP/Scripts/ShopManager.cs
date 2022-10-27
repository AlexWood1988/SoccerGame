using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
	private bool canTap;					//flag to prevent double tap
	public AudioClip tapSfx;				//buy sfx
	public Text playerMoney;				//Reference to UI text
	private int availableMoney;

	void Awake ()
	{
		//Updates UI text with saved values fetched from playerprefs
		availableMoney = PlayerPrefs.GetInt("PlayerMoney");
		playerMoney.text = "" + availableMoney;
		canTap = true;
	}

	void Update ()
	{	
		if(Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene("Menu-c#");
	}

	void playSfx ( AudioClip _clip  )
    {
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying)
        {
			GetComponent<AudioSource>().Play();
		}
	}

	public void ClickOnBackButton()
	{
		if (!canTap)
			return;

		StartCoroutine(ClickOnBackButtonCo());
	}

	public IEnumerator ClickOnBackButtonCo()
	{
		canTap = false;
		playSfx(tapSfx);
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene("Menu-c#");
	}
    
	public void ClickOnPlusButton()
	{
		if (!canTap)
			return;

		SceneManager.LoadScene("BuyCoinPack-c#");
	}

	public void ClickOnBuyFormationButton()
    {
		if (!canTap)
			return;

		StartCoroutine(ClickOnBuyFormationButtonCo());
	}

	public IEnumerator ClickOnBuyFormationButtonCo()
	{
		canTap = false;
		playSfx(tapSfx);
		yield return new WaitForSeconds(0.5f);
		SceneManager.LoadScene("ShopFormation-c#");
	}

	public void ClickOnBuyTeamButton()
	{
		StartCoroutine(ClickOnBuyTeamButtonCo());
	}

	public IEnumerator ClickOnBuyTeamButtonCo()
	{
		canTap = false;
		playSfx(tapSfx);
		yield return new WaitForSeconds(0.5f);
		SceneManager.LoadScene("ShopTeam-c#");
	}

}