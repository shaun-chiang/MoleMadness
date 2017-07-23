using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;

public class FindMatch : MonoBehaviour {

	public void FindMatchBttn()
    {
        new LogEventRequest().SetEventKey("FINDMATCH")
           .SetEventAttribute("match", "matchRanked")
           .Send((response) =>
           {
               if (!response.HasErrors)
               {
                   Debug.Log("Match Challenged");
               }
               else
               {
                   Debug.Log("Error Challenging");
               }
           });

    }
}
