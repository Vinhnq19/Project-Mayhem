using UnityEngine;
using ProjectMayhem.Player;

namespace ProjectMayhem.Manager
{

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : GenericSingleton<GameManager>
    {
        [Header("Game Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float roundTimeLimit = 300f; 

        [Header("Player Data")]
        [SerializeField] private int player1Lives;
        [SerializeField] private int player2Lives;
        [SerializeField] private int player1Score;
        [SerializeField] private int player2Score;

        // Game state
        private GameState currentGameState = GameState.MainMenu;
        private float currentRoundTime;
        private BasePlayer winner;

        // Properties
        public GameState CurrentGameState => currentGameState;
        public int Player1Lives => player1Lives;
        public int Player2Lives => player2Lives;
        public int Player1Score => player1Score;
        public int Player2Score => player2Score;
        public float CurrentRoundTime => currentRoundTime;
        public BasePlayer Winner => winner;

        protected override void Awake()
        {
            base.Awake();
            InitializeGame();
        }

        private void Start()
        {
            // EventBus.Subscribe<PlayerKilledEvent>(OnPlayerKilled);
        }

        private void OnEnable()
        {
            // EventBus.Subscribe<PlayerKilledEvent>(OnPlayerKilled);
        }

        private void OnDisable()
        {   
            // EventBus.Unsubscribe<PlayerKilledEvent>(OnPlayerKilled);
        }

        private void Update()
        {
            if (currentGameState == GameState.Playing)
            {
                UpdateRoundTimer();
            }
        }

        private void InitializeGame()
        {
            player1Lives = maxLives;
            player2Lives = maxLives;
            player1Score = 0;
            player2Score = 0;
            currentRoundTime = roundTimeLimit;
            currentGameState = GameState.MainMenu;
        }

        public void StartGame()
        {
            InitializeGame();
            currentGameState = GameState.Playing;
            Debug.Log("[GameManager] Game started!");
        }

        public void PauseGame()
        {
            if (currentGameState == GameState.Playing)
            {
                currentGameState = GameState.Paused;
                Time.timeScale = 0f;
                Debug.Log("[GameManager] Game paused!");
            }
        }

        public void ResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                currentGameState = GameState.Playing;
                Time.timeScale = 1f;
                Debug.Log("[GameManager] Game resumed!");
            }
        }

        public void EndRound(BasePlayer winner)
        {
            this.winner = winner;
            currentGameState = GameState.GameOver;
            
            if (winner != null)
            {
                if (winner.PlayerID == 1)
                    player1Score++;
                else if (winner.PlayerID == 2)
                    player2Score++;
            }

            Debug.Log($"[GameManager] Round ended! Winner: {(winner != null ? $"Player {winner.PlayerID}" : "None")}");
        }

        private void OnPlayerKilled(PlayerKilledEvent e)
        {
            BasePlayer killedPlayer = e.player;
            
            if (killedPlayer.PlayerID == 1)
            {
                player1Lives--;
                Debug.Log($"[GameManager] Player 1 killed! Lives remaining: {player1Lives}");
                
                // EventBus.Raise(new PlayerLivesUpdatedEvent(1, player1Lives));
            }
            else if (killedPlayer.PlayerID == 2)
            {
                player2Lives--;
                Debug.Log($"[GameManager] Player 2 killed! Lives remaining: {player2Lives}");
                
                // EventBus.Raise(new PlayerLivesUpdatedEvent(2, player2Lives));
            }

            CheckGameOverConditions();
        }

        private void CheckGameOverConditions()
        {
            if (player1Lives <= 0 || player2Lives <= 0)
            {
                BasePlayer winner = null;
                if (player1Lives > 0) winner = FindPlayerByID(1);
                else if (player2Lives > 0) winner = FindPlayerByID(2);
                
                EndRound(winner);
            }
            else if (currentRoundTime <= 0)
            {
                BasePlayer winner = null;
                if (player1Lives > player2Lives) winner = FindPlayerByID(1);
                else if (player2Lives > player1Lives) winner = FindPlayerByID(2);
                
                EndRound(winner);
            }
        }

        private void UpdateRoundTimer()
        {
            currentRoundTime -= Time.deltaTime;
            if (currentRoundTime <= 0)
            {
                currentRoundTime = 0;
                CheckGameOverConditions();
            }
        }

        private BasePlayer FindPlayerByID(int playerID)
        {
            BasePlayer[] players = FindObjectsOfType<BasePlayer>();
            foreach (BasePlayer player in players)
            {
                if (player.PlayerID == playerID)
                    return player;
            }
            return null;
        }

        public void ResetGame()
        {
            InitializeGame();
            Time.timeScale = 1f;
            Debug.Log("[GameManager] Game reset!");
        }
    }

    public struct PlayerKilledEvent
    {
        public BasePlayer player;
        
        public PlayerKilledEvent(BasePlayer player)
        {
            this.player = player;
        }
    }

    public struct PlayerDamageUpdatedEvent
    {
        public int playerID;
        public float newDamagePercent;
        
        public PlayerDamageUpdatedEvent(int playerID, float newDamagePercent)
        {
            this.playerID = playerID;
            this.newDamagePercent = newDamagePercent;
        }
    }

    public struct PlayerLivesUpdatedEvent
    {
        public int playerID;
        public int newLives;
        
        public PlayerLivesUpdatedEvent(int playerID, int newLives)
        {
            this.playerID = playerID;
            this.newLives = newLives;
        }
    }

    public struct PlaySoundEvent
    {
        public AudioClip clip;
        public Vector3 position;
        
        public PlaySoundEvent(AudioClip clip, Vector3 position)
        {
            this.clip = clip;
            this.position = position;
        }
    }

    public struct PlayMusicEvent
    {
        public AudioClip clip;
        
        public PlayMusicEvent(AudioClip clip)
        {
            this.clip = clip;
        }
    }
}
