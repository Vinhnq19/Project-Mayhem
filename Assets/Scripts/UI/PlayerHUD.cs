using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using ProjectMayhem.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public int id;

    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private TextMeshProUGUI ammoText;

    [SerializeField] private UnityEngine.UI.Image avatar;
    [SerializeField] private BasePlayer player;
    private PlayerCombat playerCombat;

    private Action<int> updateLife;
    private Action<int> updateAmmo;
    // Start is called before the first frame update
    void Start()
    {
        playerCombat = player.GetComponent<PlayerCombat>();
        if (player.PlayerID == 1)
        {
            avatar.color = GameData.Instance.player1Color;
        }
        else
        {
            avatar.color = GameData.Instance.player2Color;
        }
        UpdateLife(GameData.Instance.playerLife);

        updateLife = (amount) => UpdateLife(amount);

        EventBus.On("PlayerDie" + this.player.PlayerID, updateLife);
    }

    private void UpdateLife(int amount)
    {
        lifeText.text = amount + "";
    }

    private void UpdateAmmo(int amount)
    {
        ammoText.text = amount + "";
    }

    private void UpdateAvator(int color)
    {

    }

    // Update is called once per frame
    void Update()
    {
        ammoText.text = playerCombat.CurrentWeapon.CurrentAmmo.ToString();
    }

    void OnDestroy()
    {
        EventBus.Off("PlayerDie" + this.player.PlayerID, updateLife);
    }
}
//