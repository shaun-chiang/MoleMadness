using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using Facebook.Unity;

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
                    SceneManager.LoadScene(2);

                }
                else
                {
                    Debug.Log("Error Authenticating Player...");
                    notificationText.text = "Authentication Error";

                }
            });
    }

    public void FacebookConnect_bttn()
    {
        Debug.Log("Connecting Facebook With GameSparks...");// first check if FB is ready, and then login //
                                                            // if it's not ready we just init FB and use the login method as the callback for the init method //
        if (!FB.IsInitialized)
        {
            Debug.Log("Initializing Facebook...");
            FB.Init(ConnectGameSparksToGameSparks, null);
        }
        else
        {
            FB.ActivateApp();
            ConnectGameSparksToGameSparks();
        }
    }

    ///<summary>
    ///This method is used as the delegate for FB initialization. It logs in FB
    /// </summary>
    private void ConnectGameSparksToGameSparks()
    {
        if (FB.IsInitialized) {
            FB.ActivateApp();
            Debug.Log("Logging Into Facebook...");
            var perms = new List<string>() { "public_profile", "email", "user_friends" };
            FB.LogInWithReadPermissions(perms, (result) => {
                if (FB.IsLoggedIn){
                    Debug.Log("Logged In, Connecting GS via FB..");
                    new GameSparks.Api.Requests.FacebookConnectRequest().SetAccessToken(AccessToken.CurrentAccessToken.TokenString)
                    .SetDoNotLinkToCurrentPlayer(false)// we don't want to create a new account so link to the player that is currently logged in
                    .SetSwitchIfPossible(true)//this will switch to the player with this FB account id they already have an account from a separate login
                    .Send((fbauth_response) => {
                        if (!fbauth_response.HasErrors){
                            notificationText.text = "GameSparks Authenticated With Facebook...";
                            Debug.Log(fbauth_response.DisplayName);

                            SceneManager.LoadScene(1);
                        }
                        else{
                            Debug.LogWarning(fbauth_response.Errors.JSON);//if we have errors, print them out
                        }
                    });
                }
                else{
                    Debug.LogWarning("Facebook Login Failed:" + result.Error);
                }
            });// lastly call another method to login, and when logged in we have a callback
        }
        else{
            FacebookConnect_bttn();// if we are still not connected, then try to process again
        }
    }
}
