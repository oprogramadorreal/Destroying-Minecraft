using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Minecraft
{
    /// <summary>
    /// Based on https://youtu.be/KZuqEyxYZCc
    /// </summary>
    public sealed class Highscores : MonoBehaviour
	{
		// Access http://dreamlo.com to create your private and public key.
		private const string privateCode = "pStXN7------------P1nQe4HE------------vjBzfw";
		private const string publicCode = "5fbad-----36fd-----e5819";
		private const string webURL = "http://dreamlo.com/lb/";

		private List<Entry> highscoresList;

		public event EventHandler<HighscoresDownloadedEventArgs> HighscoresDownloaded;
		public event EventHandler<NewHighscoreUploadedEventArgs> NewHighscoreUploaded;

		public void AddNewHighscore(string userName, int score)
		{
			StartCoroutine(UploadNewHighscore(userName, score));
		}

		public void DownloadHighscores()
		{
			StartCoroutine(nameof(DownloadHighscoresFromDatabase));
		}

		private IEnumerator UploadNewHighscore(string userName, int score)
		{
			userName = userName.ToUpper();

			var www = UnityWebRequest.Get(webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(userName) + "/" + score);
			yield return www.SendWebRequest();

			if (string.IsNullOrEmpty(www.error))
			{
				OnNewHighscoreUploaded(userName, score);
			}
			else
			{
				Debug.Log("Error uploading: " + www.error);
			}
		}

		private IEnumerator DownloadHighscoresFromDatabase()
		{
			var www = UnityWebRequest.Get(webURL + publicCode + "/pipe/");
			yield return www.SendWebRequest();

			if (string.IsNullOrEmpty(www.error))
			{
				highscoresList = FormatHighscores(www.downloadHandler.text);
				//highscoresList.Sort();

				OnHighscoresDownloaded();
			}
			else
			{
				Debug.Log("Error Downloading: " + www.error);
			}
		}

		private static List<Entry> FormatHighscores(string textStream)
		{
			var entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
			var highscoresList = new List<Entry>(entries.Length);

			for (var i = 0; i < entries.Length; i++)
			{
				var entryInfo = entries[i].Split(new char[] { '|' });

				if (entryInfo.Length >= 2)
				{
					var userName = entryInfo[0].Replace('+', ' ');
					var score = entryInfo[1];
					int finalScore = -1;
					try
					{
						finalScore = int.Parse(score);
					}
					catch (OverflowException e1)
					{
						finalScore = int.MaxValue;
					}
					finally
					{
						highscoresList.Add(new Entry(userName, finalScore));
					}
				}
			}

			return highscoresList;
		}

		private void OnNewHighscoreUploaded(string userName, int score)
		{
			var handler = NewHighscoreUploaded;
			handler?.Invoke(this, new NewHighscoreUploadedEventArgs(userName, score));
		}

		private void OnHighscoresDownloaded()
        {
			var handler = HighscoresDownloaded;
			handler?.Invoke(this, new HighscoresDownloadedEventArgs(highscoresList));
		}

		public sealed class Entry : IEquatable<Entry>, IComparable<Entry>
		{
			public string UserName { get; private set; }

			public int Score { get; private set; }

			public Entry(string userName, int score)
			{
				UserName = userName;
				Score = score;
			}

            bool IEquatable<Entry>.Equals(Entry other)
            {
                if (other == null)
                {
					return false;
                }

				return Score == other.Score
					&& UserName.Equals(other.UserName);
			}

            int IComparable<Entry>.CompareTo(Entry other)
            {
				if (other == null)
                {
					return 1;
                }

				return Score.CompareTo(other.Score);
            }
        }

		public sealed class HighscoresDownloadedEventArgs : EventArgs
		{
			public HighscoresDownloadedEventArgs(IList<Entry> highscores)
			{
				Highscores = highscores;
			}

			public IList<Entry> Highscores { get; private set; }
		}

		public sealed class NewHighscoreUploadedEventArgs : EventArgs
		{
			public string UserName { get; private set; }

			public int Score { get; private set; }

			public NewHighscoreUploadedEventArgs(string userName, int score)
			{
				UserName = userName;
				Score = score;
			}
		}
	}
}
