using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Minecraft
{
    public sealed class GameOverMenu : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private TMP_Text finalScoreText;

        [SerializeField]
        private TMP_Text blocksDestroyedText;

        [SerializeField]
        private TMP_Text foesKilledText;

        [SerializeField]
        private Text userNameText;

        [SerializeField]
        private GameObject inputField;

        [SerializeField]
        private Highscores highscore;

        [SerializeField]
        private TMP_Text thanksText;

        private void Start()
        {
            highscore.NewHighscoreUploaded += Highscore_NewHighscoreUploaded; ;
        }

        private void Highscore_NewHighscoreUploaded(object sender, Highscores.NewHighscoreUploadedEventArgs e)
        {
            inputField.SetActive(false);

            thanksText.text = string.Format("Thanks, {0}", e.UserName);
            thanksText.gameObject.SetActive(true);
        }

        public void ShowMenu()
        {
            finalScoreText.text = gameState.TotalScore.ToString();
            blocksDestroyedText.text = gameState.BlocksCount.ToString();
            foesKilledText.text = gameState.FoesCount.ToString();

            gameObject.SetActive(true);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void AddNewHighscore()
        {
            highscore.AddNewHighscore(userNameText.text, gameState.TotalScore);
        }
    }
}