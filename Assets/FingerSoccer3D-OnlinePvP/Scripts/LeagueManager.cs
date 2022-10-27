using UnityEngine;
using System.Collections;

public class LeagueManager : MonoBehaviour
{
    /// <summary>
    /// Main single player League manager.
    /// </summary>

    public static int totalLeagues = 9;		//total number of available leagues

	/// <summary>
	/// Gets the league settings.
	/// x = Prize
	/// y = Entry fee
	/// z = game time (in seconds)
	/// </summary>
	public static Vector3 GetLeaguesSettings(int id)
    {
		Vector3 settings = Vector3.zero;
		switch (id)
        {
		    case 0:
			    settings = new Vector3(25, 0, 120);
			    break;
		    case 1:
			    settings = new Vector3(80, 25, 60);
			    break;
		    case 2:
			    settings = new Vector3(150, 50, 120);
			    break;
		    case 3:
			    settings = new Vector3(500, 200, 150);
			    break;
		    case 4:
			    settings = new Vector3(1000, 300, 180);
			    break;
		    case 5:
			    settings = new Vector3(1500, 500, 90);
			    break;
		    case 6:
			    settings = new Vector3(800, 250, 120);
			    break;
		    case 7:
			    settings = new Vector3(1500, 500, 90);
			    break;
		    case 8:
			    settings = new Vector3(5000, 2000, 120);
			    break;

		    default:
			    settings = new Vector3(100, 50, 120);
			    break;
		}
		return settings;
	}
}