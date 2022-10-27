using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MoneyController : MonoBehaviour
{
    /// <summary>
    /// Main CoinPack purchase Controller.
    /// This class handles all touch events on coin packs.
    /// You can easily integrate your own (custom) IAB system to deliver a nice 
    /// IAP options to the player.
    /// </summary>

    public static bool canTap;					//flag to prevent double purchase
	public AudioClip coinsCheckout;				//purchase sound
	public AudioClip tapSfx;					//purchase sound

	//Reference to GameObjects
	public Text playerMoney;		
	private int availableMoney;		

	void Awake ()
	{
		canTap = true;
		availableMoney = PlayerPrefs.GetInt("PlayerMoney");
		playerMoney.text = "" + availableMoney;
	}


	public void playSfx (AudioClip _clip)
	{
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying)
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
		SceneManager.LoadScene("Menu-c#");
	}



	public void BuyCoinpackByID(Coinpacks cp)
    {
		StartCoroutine(BuyCoinpackByIDCo(cp));
	}

	public IEnumerator BuyCoinpackByIDCo(Coinpacks cp)
    {
		if (!canTap)
			yield break;
		canTap = false;

/*		//IAB only works in Android build. (Editor is for test purposes)
		#if UNITY_WEBGL
					Debug.LogWarning("IAB only works in an Android build.");
					return;
		#endif*/

		#if UNITY_EDITOR || UNITY_ANDROID
				//play sfx
				playSfx(coinsCheckout);
		#endif

		//Here you should implement your own in-app purchase routines.
		//But for simplicity, we add the basic functionality

		//** Required steps **
		//Lead the player to the in-app gate and after the purchase is done, go to next line.
		//You should open the pay gateway, make the transaction, close the gateway, get the response and then consume the purchased item.
		//Then you can grant the user access to the item.
		//For security, you can avoid having money or similar purchasable items in plant text (string) and encode them with custom hash.

		//add the purchased coins to the available user money
		availableMoney += cp.packAmount;

		//save new amount of money
		PlayerPrefs.SetInt("PlayerMoney", availableMoney);

		//play sfx
		playSfx(coinsCheckout);

		//Wait
		yield return new WaitForSeconds(1.5f);

		//Reload the level
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

}