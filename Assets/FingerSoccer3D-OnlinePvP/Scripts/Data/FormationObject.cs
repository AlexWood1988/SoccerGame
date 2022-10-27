using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FormationObject
{
    public string formationName;
    public Texture2D formationImage;
    public int formationPrice;
    public int isUnlocked;  //0=locked & 1=unlocked
}
