using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coinpacks : MonoBehaviour
{
    public int packID;
    public int packAmount = 100;
    public float packPrice = 0.99f;

    //Gameobject reference
    public Text packAmountUI;
    public Text packPriceUI;

    void Start()
    {
        packAmountUI.text = packAmount + " Coins";
        packPriceUI.text = "$" + packPrice;
    }

    void Update()
    {
        
    }
}
