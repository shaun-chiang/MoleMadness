using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api.Requests;
using GameSparks.Api.Messages;
using UnityEngine.SceneManagement;

public class FindMatch : MonoBehaviour {

    public Text NotificationText;
    public void Awake()
    {
        MatchNotFoundMessage.Listener += GetMessages;
    }

    public void Start()
    {
        Debug.Log("Looking for Match");
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

    public void GetMessages(MatchNotFoundMessage message)
    {
        NotificationText.text = "Can't find a match :(";
        StartCoroutine(ExecuteAfterTime(3));
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Prematch");
    }
}
