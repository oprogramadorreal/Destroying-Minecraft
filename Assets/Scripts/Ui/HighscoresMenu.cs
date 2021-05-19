using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Minecraft
{
    public sealed class HighscoresMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject mainMenu;

        [SerializeField]
        private Highscores highscores;

        [SerializeField]
        private List<Entry> entries;

        private void Start()
        {
            highscores.HighscoresDownloaded += Highscores_HighscoresDownloaded;
        }

        private void OnEnable()
        {
            foreach (var e in entries)
            {
                e.name.text = "...";
                e.score.text = "...";
            }

            highscores.DownloadHighscores();
        }

        private void Highscores_HighscoresDownloaded(object sender, Highscores.HighscoresDownloadedEventArgs e)
        {
            for (var i = 0; i < Mathf.Min(entries.Count, e.Highscores.Count); ++i)
            {
                entries[i].name.text = e.Highscores[i].UserName;
                entries[i].score.text = e.Highscores[i].Score.ToString();
            }
        }

        [System.Serializable]
        public sealed class Entry
        {
            public TMP_Text name;
            public TMP_Text score;
        }
    }
}