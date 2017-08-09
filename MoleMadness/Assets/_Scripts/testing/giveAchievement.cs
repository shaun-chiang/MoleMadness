using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
public class giveAchievement : MonoBehaviour {
	// Use this for initialization
	public void achieveButton()
    {
        new LogEventRequest().SetEventKey("AWARD_ACHIEVEMENT")
            .SetEventAttribute("ACH_NAME", "FIRST_WIN") // change string to whatever achievement shortcode you need
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Achievement awarded");
                }
                else
                {
                    Debug.Log("Error with Achievement awarding");
                }
            });
    }

}

