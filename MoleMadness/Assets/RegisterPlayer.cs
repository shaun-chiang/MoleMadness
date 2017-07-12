using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;

public class RegisterPlayer : MonoBehaviour
{

    public Text displayNameInput, userNameInput, passwordInput, notificationText;
    public void RegisterPlayerBttn()
    {
        Debug.Log("Registering..");

        new RegistrationRequest()
          .SetDisplayName(displayNameInput.text)
          .SetPassword(passwordInput.text)
          .SetUserName(displayNameInput.text)
          .Send((response) =>
          {
              if (!response.HasErrors)
              {
                  Debug.Log("Player Registered");
                  notificationText.text = "Player Registered";
              }
              else
              {
                  Debug.Log("Error Registering Player");
                  notificationText.text = "Error Registering Player";
              }
          }
        );
    }

    public void AuthorizePlayerBttn()
    {
        Debug.Log("Authorizing Player...");
        new AuthenticationRequest()
            .SetUserName(userNameInput.text)
            .SetPassword(passwordInput.text)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Player Authenticated...");
                    notificationText.text = "Player Authenticated";
                    PlayerPrefs.SetString("PlayerName", userNameInput.text);
                    SceneManager.LoadScene(1);

                }
                else
                {
                    Debug.Log("Error Authenticating Player...");
                    notificationText.text = "Authentication Error";

                }
            });
    }
}
