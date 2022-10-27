using UnityEngine;
using System.Collections;

public class FormationItemProperties : MonoBehaviour {

    /// <summary>
    /// A simple value holder for different shop items.
    /// You can easily add/edit item properties via this controller.
    /// </summary>

    internal int isInUse;
	public int itemIndex;
	public int itemPrice;

	//childs
	public GameObject formationUI;
	public GameObject priceUI;
	public GameObject buyLabelUI;
	public GameObject coinIcon;

	//materials for buy button
	public Material[] stateMat;

	void Start ()
    {
		//hide coin & price if we already purchased this item
		if (isInUse == 1)
        {
			priceUI.GetComponent<Renderer> ().enabled = false;
			coinIcon.GetComponent<Renderer> ().enabled = false;
		}

		//set buy button state mat
		GetComponent<Renderer>().material = stateMat[isInUse];
		priceUI.GetComponent<TextMesh> ().text = itemPrice.ToString ();
		formationUI.GetComponent<TextMesh> ().text = GlobalGameSettings.instance.availableFormations[itemIndex - 100].formationName;
	}

}