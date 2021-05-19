using UnityEngine;

namespace Minecraft
{
    [RequireComponent(typeof(GameState))]
    public sealed class GameManager : MonoBehaviour, IGameState
    {
        private bool isGodMode = false;
        private bool isPaused = false;
        private bool isGameStarted = false;
        private bool isGameOver = false;

        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private PlayerMovement playerMovement;

        [SerializeField]
        private Kamehameha kamehameha;

        [SerializeField]
        private FoesManager foesManager;

        [SerializeField]
        private TerrainManager terrainManager;

        [SerializeField]
        private PostProcessingController postProcessing;

        [SerializeField]
        private GameObject uiIntroScreen;

        [SerializeField]
        private MainMenu uiMainMenu;

        [SerializeField]
        private Scoreboard uiScoreboard;

        [SerializeField]
        private GameOverMenu uiGameOverMenu;

        [SerializeField]
        private SunController sunController;

        [SerializeField]
        private AudioManager audioManager;

        private AudioSource introMusic;
        private AudioSource dayLightMusic;

        bool IGameState.IsGodMode => isGodMode;

        bool IGameState.IsPaused => isPaused || isGameOver;

        int IGameState.BlocksCount => kamehameha.BlocksCount;

        int IGameState.FoesCount => kamehameha.FoesCount;

        int IGameState.TotalScore => Mathf.FloorToInt(kamehameha.BlocksCount / 1000.0f) + kamehameha.FoesCount* 100;

        bool IGameState.IsNight => sunController.IsNight();

        private void Awake()
        {
            GetComponent<GameState>().SetImpl(this);
            uiIntroScreen.SetActive(true);
        }

        private void Start()
        {
            introMusic = audioManager.CreateAudioSource("Intro");

            SetPaused(true);
            SetGodMode(true);

            terrainManager.FirstLoadFinished += (s, e) =>
            {
                uiMainMenu.EnablePlayButton();
            };

            playerController.Killed += (s, e) =>
            {
                GameOver();
            };

            sunController.DayLightChanged += SunController_DayLightChanged;
        }

        private void SunController_DayLightChanged(object sender, SunController.DayLightChangedEventArgs e)
        {
            if (e.IsNight)
            {
                audioManager.Fade(dayLightMusic, 5.0f, 0.0f);
            }
            else
            {
                audioManager.Fade(dayLightMusic, 5.0f, 0.15f);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (isGameStarted && !isGameOver)
                {
                    SetPaused(!isPaused);
                }
            }

            //if (Input.GetKeyDown(KeyCode.Tab))
            //{
            //    SwitchGodMode();
            //}
        }

        private void GameOver()
        {
            if (!isGameOver)
            {
                SetPaused(true);
                SetMouseCursorVisible(true);
                postProcessing.TurnBlurOn();
                uiScoreboard.HideScoreboard();
                uiGameOverMenu.ShowMenu();

                isGameOver = true;

                audioManager.CreateTemporaryAudioSource("GameOver");
            }
        }

        public void PlayGame()
        {
            if (!isGameStarted)
            {
                kamehameha.ClearCounters();
                foesManager.StartSpawning();

                uiIntroScreen.SetActive(false);
                uiScoreboard.ShowScoreboard();
                postProcessing.TurnBlurOff(4.0f);

                SetPaused(false);
                SetGodMode(false);

                isGameStarted = true;
                introMusic.Stop();

                dayLightMusic = audioManager.CreateAudioSource("DayLight");
            }
        }

        private void SetPaused(bool paused)
        {
            if (paused != isPaused)
            {
                if (paused)
                {
                    Time.timeScale = 0.0f;
                }
                else
                {
                    Time.timeScale = 1.0f;
                }

                isPaused = paused;
            }
        }

        private void SwitchGodMode()
        {
            if (!isGameStarted)
            {
                uiIntroScreen.SetActive(false);
                PlayGame();
            }
            else
            {
                SetGodMode(!isGodMode);
            }
        }

        private void SetGodMode(bool godMode)
        {
            if (godMode != isGodMode)
            {
                SetMouseCursorVisible(godMode);
                playerMovement.SetGodMode(godMode);

                isGodMode = godMode;
            }
        }

        private void SetMouseCursorVisible(bool visible)
        {
            Cursor.visible = visible;

            if (visible)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}