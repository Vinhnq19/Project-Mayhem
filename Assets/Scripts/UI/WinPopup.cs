using System;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class WinPopup : MonoBehaviour
{
    [SerializeField] private GameObject popup;

    [SerializeField] private UnityEngine.UI.Image playerImage;

    [SerializeField] private TextMeshProUGUI winText;
    // Start is called before the first frame update

    Action<int> onEndGame;
    void Start()
    {
        onEndGame = (id) => OnEndGame(id);

        EventBus.On("Lose", onEndGame);
    }

    public void OnEndGame(int id)
    {
        popup.SetActive(true);

         popup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

        playerImage.color = id == 2 ? GameData.Instance.player1Color : GameData.Instance.player2Color;

        winText.text = "Player " + (id == 2 ? 1 : 2) + " Win";
    }


    void OnDestroy()
    {
        EventBus.Off("Lose", onEndGame);
    }
}
