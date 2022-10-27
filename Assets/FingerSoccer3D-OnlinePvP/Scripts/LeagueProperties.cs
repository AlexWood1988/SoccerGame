using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeagueProperties : MonoBehaviour
{
    public int leagueID = 1;
    public int leagueEntryfee = 50;
    public int leaguePrize  = 100;
    public int leagueDuration = 120;

    public Text leagueEntreFeeUI;
    public Text leaguePrizeUI;
    public Text leagueDurationUI;

    void Start()
    {
        //update UI text for each league item
        leagueEntreFeeUI.text = "Entry fee: " + leagueEntryfee + "";
        leaguePrizeUI.text = "Prize: " + leaguePrize + "";
        leagueDurationUI.text = "Duration: " + leagueDuration + " Sec";
    }
}
