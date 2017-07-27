using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;

public class GameSparksAccountDetailsListener : MonoBehaviour {


    void Start()
    {
        new AccountDetailsRequest().Send((response) =>
        {
            Debug.Log("Requesting Account Details...");
            if (!response.HasErrors)
            {
                Debug.Log("Got Player Details");
                Debug.Log(response);
                JSONObject jsonmessage = new JSONObject(response.JSONString);
                Debug.Log(jsonmessage);
                Debug.Log(jsonmessage["scriptData"]["equippedItems"]);
                PlayerPrefs.SetString("playerId", jsonmessage["userId"].ToString());
                PlayerPrefs.SetString("equipped", jsonmessage["scriptData"]["equippedItems"].ToString());
                PlayerPrefs.SetString("virtualgoods", jsonmessage["virtualGoods"].ToString());
                PlayerPrefs.SetString("rank", jsonmessage["scriptData"]["rank"].ToString());
                PlayerPrefs.SetString("stars", jsonmessage["scriptData"]["stars"].ToString());
                PlayerPrefs.SetString("gems", jsonmessage["currency1"].ToString());
            }
            else
            {
                Debug.Log("Error Receiving Account Details");
            }
        });

    }
}

