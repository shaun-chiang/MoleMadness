using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api.Requests;

public class FindMatch : MonoBehaviour {

    public Text FindMatchStatus;
    public void FindMatchBttn()
    {
        Debug.Log("Button Pressed");
        new LogEventRequest().SetEventKey("FINDMATCH")
           .SetEventAttribute("match", "matchRanked")
           .Send((response) =>
           {
               if (!response.HasErrors)
               {
                   Debug.Log("Match Challenged");
                   FindMatchStatus.text = "Match Challenged";
               }
               else
               {
                   Debug.Log("Error Challenging");
                   FindMatchStatus.text = "Error Challenging";
               }
           });

    }
}
