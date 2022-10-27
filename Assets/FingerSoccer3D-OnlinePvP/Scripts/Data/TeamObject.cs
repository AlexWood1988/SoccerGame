using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TeamObject
{
    public Texture2D teamIcon;          //Used for 3d models & texture-based elements
    public Sprite teamSprite;           //Used for 2D/UI elements
    public Material teamMaterial;
    public int teamPrice;
    public Vector4 teamAttributes;
}
