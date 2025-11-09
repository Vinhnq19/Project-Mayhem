using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{

    public static GameData Instance;

    public int playerLife = 3;

    // public static float mapTime = 180f

    public Color player1Color;
    public Color player2Color;

    void Awake()
    {
        if (Instance) Destroy(this.gameObject);

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetPlayer1Color(Color color)
    {
        player1Color = color;
    }

    public void SetPlayer2Color(Color color)
    {
        player2Color = color;
    }


}
