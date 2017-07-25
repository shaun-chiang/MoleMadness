using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager{

    public enum GameState { SPAWNINGMOTHER, SPAWNINGBABY, PLAYERTURN, OPPONENTTURN, RESPAWNBABY };

    public GameState currentGameState;
    public static GameManager instance;

    public GameManager(GameState gameState)
    {
        currentGameState = gameState;
        if (instance == null)
        {
            instance = this;
        }
    }

    public static GameManager getInstance()
    {
        return instance;
    }

}
