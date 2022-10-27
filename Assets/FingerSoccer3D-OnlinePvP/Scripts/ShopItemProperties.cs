using UnityEngine;
using System.Collections;

public class ShopItemProperties : MonoBehaviour
{
    internal int isInUse;
    public int itemIndex;
    public int itemPrice;
    internal Material flagMat;

    //childs
    public GameObject flagUI;
    public GameObject priceUI;
    public GameObject powerUI;
    public GameObject timeUI;
    public GameObject aimUI;
    public GameObject buyLabelUI;
    public GameObject coinIcon;

    //materials for buy button
    public Material[] stateMat;

    void Start()
    {
        //set flag
        flagUI.GetComponent<Renderer>().material = flagMat;

        //set buy button state mat
        GetComponent<Renderer>().material = stateMat[isInUse];

        //hide coin & price if we already purchased this item
        if (isInUse == 1)
        {
            priceUI.GetComponent<Renderer>().enabled = false;
            coinIcon.GetComponent<Renderer>().enabled = false;
        }

        //set info on UI
        priceUI.GetComponent<TextMesh>().text = itemPrice.ToString();
        powerUI.GetComponent<TextMesh>().text = "" + GlobalGameSettings.instance.availableTeams[itemIndex - 1000].teamAttributes.x /*+ "/10"*/;
        timeUI.GetComponent<TextMesh>().text = "" + GlobalGameSettings.instance.availableTeams[itemIndex - 1000].teamAttributes.y /*+ "/10"*/;
        aimUI.GetComponent<TextMesh>().text = "" + GlobalGameSettings.instance.availableTeams[itemIndex - 1000].teamAttributes.z /*+ "/10"*/;
    }

}