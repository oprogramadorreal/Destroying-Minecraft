using UnityEngine;
using UnityEngine.UI;

namespace Minecraft
{
    public sealed class Scoreboard : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private Text totalScoreText;

        [SerializeField]
        private Text blocksCountText;

        [SerializeField]
        private Text foesCountText;

        [SerializeField]
        private Text fpsText;

        private readonly float[] fpsBuffer = new float[20];
        private int fpsBufferCurrent = 0;
        private float averageFPS = 0.0f;

        private void Update()
        {
            totalScoreText.text = string.Format("Score: {0}", gameState.TotalScore);
            blocksCountText.text = string.Format("Blocks: {0}", gameState.BlocksCount);
            foesCountText.text = string.Format("Saiyans: {0}", gameState.FoesCount);

            //UpdateAverageFPS();
            //fpsText.text = string.Format("FPS: {0}", (int)averageFPS);
        }

        private void UpdateAverageFPS()
        {
            if (fpsBufferCurrent < fpsBuffer.Length)
            {
                fpsBuffer[fpsBufferCurrent] = 1.0f / Time.unscaledDeltaTime;
                ++fpsBufferCurrent;
            }
            else
            {
                averageFPS = 0.0f;

                for (var i = 0; i < fpsBuffer.Length; ++i)
                {
                    averageFPS += fpsBuffer[i];
                }

                averageFPS /= fpsBuffer.Length;
                fpsBufferCurrent = 0;
            }
        }

        public void ShowScoreboard()
        {
            gameObject.SetActive(true);
        }

        public void HideScoreboard()
        {
            gameObject.SetActive(false);
        }
    }
}