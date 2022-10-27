using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameSettings : MonoBehaviour
{
    public static GlobalGameSettings instance;

    [Space(10)]
    public TeamObject[] availableTeams;

    [Space(10)]
    public FormationObject[] availableFormations;

    [Space(10)]
    public TimeObject[] availableTimes;

    [Space(10)]
    public FieldObject[] availableFields;

    //[Space(20)]
    [Header("Online Gameplay Configurations")]
    public int onlineModeTimer;                     //Duration of an online match (in seconds)
    public bool useFakeOpponent = false;            //should your player be paired with a fake bot if game is not able to find a real player after some time?
    [Range(5, 30)]
    public int fakeOpponentSearchTime = 20;       //How many seconds should the game search for a human player before selecting a fake bot?
                                                    //this setting only works if "useFakeOpponent" is set to true.

    private void Awake()
    {
        instance = this;
    }
}
