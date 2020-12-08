using UnityEngine;

namespace Minecraft
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private GameObject playButton;

        [SerializeField]
        private GameObject progressBar;

        [SerializeField]
        private GameObject highscoresMenu;

        private void Start()
        {
            playButton.SetActive(false);
            progressBar.SetActive(true);
        }

        public void EnablePlayButton()
        {
            playButton.SetActive(true);
            progressBar.SetActive(false);
        }

        public void PlayGame()
        {
            gameManager.PlayGame();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}