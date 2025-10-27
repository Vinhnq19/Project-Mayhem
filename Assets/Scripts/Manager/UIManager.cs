using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region UI Panels
    public MainMenuPanel mainMenuPanel { get; private set; }
    public IngameHUDPanel inGameHUDPanel { get; private set; }
    public GameSetupPanel gameSetupPanel { get; private set; }
    public PostMatchPanel postMatchPanel { get; private set; }
    #endregion

    //UI cho mỗi người chơi
    [SerializeField] private List<PlayerHUD> playerHUDs;
    [SerializeField] private TextMeshProUGUI winnerText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        mainMenuPanel = GetComponentInChildren<MainMenuPanel>();
        inGameHUDPanel = GetComponentInChildren<IngameHUDPanel>();
        gameSetupPanel = GetComponentInChildren<GameSetupPanel>();
        postMatchPanel = GetComponentInChildren<PostMatchPanel>();
    }

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.gameObject.SetActive(true);
    }

    public void ShowGameSetup()
    {
        gameSetupPanel.gameObject.SetActive(true);
    }

    public void ShowInGameHUD()
    {
        inGameHUDPanel.gameObject.SetActive(true);
    }

    public void ShowPostMatchScreen(string winnerName)
    {
        if (postMatchPanel != null)
        {
            postMatchPanel.gameObject.SetActive(true);
            winnerText.text = $"{winnerName} Wins!";
        }
    }

    /// <summary>
    /// Cập nhật HUD cho một người chơi cụ thể.
    /// </summary>
    /// <param name="playerIndex">Index của người chơi (0-3).</param>
    /// <param name="damagePercent">Phần trăm sát thương hiện tại.</param>
    /// <param name="livesRemaining">Số mạng còn lại.</param>
    public void UpdatePlayerHUD(int playerIndex, float damagePercent, int livesRemaining, int currentAmmo, int maxAmmo)
    {
        if (playerIndex < 0 || playerIndex >= playerHUDs.Count)
        {
            Debug.LogError("Invalid player index for HUD update.");
            return;
        }
        playerHUDs[playerIndex].UpdateDamageText(damagePercent);
        playerHUDs[playerIndex].UpdateLivesText(livesRemaining);
        playerHUDs[playerIndex].UpdateAmmoText(currentAmmo, maxAmmo);

    }


}
