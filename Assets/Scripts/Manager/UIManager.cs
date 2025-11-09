using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ProjectMayhem.Manager;

namespace ProjectMayhem.Manager
{
    public class UIManager : GenericSingleton<UIManager>
    {
        #region UI Panels

        public MainMenuPanel mainMenuPanel { get; private set; }
        public IngameHUDPanel inGameHUDPanel { get; private set; }
        public GameSetupPanel gameSetupPanel { get; private set; }
        public PostMatchPanel postMatchPanel { get; private set; }

        #endregion

        // [SerializeField] private List<PlayerHUD> playerHUDs;
        [SerializeField] private TextMeshProUGUI winnerText;

        protected override void Awake()
        {
            base.Awake();

            mainMenuPanel = GetComponentInChildren<MainMenuPanel>();
            inGameHUDPanel = GetComponentInChildren<IngameHUDPanel>();
            gameSetupPanel = GetComponentInChildren<GameSetupPanel>();
            postMatchPanel = GetComponentInChildren<PostMatchPanel>();
        }

        private void OnEnable()
        {
            // EventBus.Subscribe<PlayerDamageUpdatedEvent>(OnPlayerDamageUpdated);
            // EventBus.Subscribe<PlayerLivesUpdatedEvent>(OnPlayerLivesUpdated);
        }

        private void OnDisable()
        {
            // EventBus.Unsubscribe<PlayerDamageUpdatedEvent>(OnPlayerDamageUpdated);
            // EventBus.Unsubscribe<PlayerLivesUpdatedEvent>(OnPlayerLivesUpdated);
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void OnPlayerDamageUpdated(PlayerDamageUpdatedEvent e)
        {
            int playerIndex = e.playerID - 1;

            // if (playerIndex >= 0 && playerIndex < playerHUDs.Count)
            // {
            //     playerHUDs[playerIndex].UpdateDamageText(e.newDamagePercent);
            //     Debug.Log($"[UIManager] Updated damage for Player {e.playerID}: {e.newDamagePercent}%");
            // }
        }

        private void OnPlayerLivesUpdated(PlayerLivesUpdatedEvent e)
        {
            int playerIndex = e.playerID - 1;

            // if (playerIndex >= 0 && playerIndex < playerHUDs.Count)
            // {
            //     playerHUDs[playerIndex].UpdateLivesText(e.newLives);
            //     Debug.Log($"[UIManager] Updated lives for Player {e.playerID}: {e.newLives}");
            // }
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

        public void UpdatePlayerHUD(int playerIndex, float damagePercent, int livesRemaining, int currentAmmo,
            int maxAmmo)
        {
            // if (playerIndex < 0 || playerIndex >= playerHUDs.Count)
            // {
            //     Debug.LogError("Invalid player index for HUD update.");
            //     return;
            // }

            // playerHUDs[playerIndex].UpdateDamageText(damagePercent);
            // playerHUDs[playerIndex].UpdateLivesText(livesRemaining);
            // playerHUDs[playerIndex].UpdateAmmoText(currentAmmo, maxAmmo);
        }
    }
}