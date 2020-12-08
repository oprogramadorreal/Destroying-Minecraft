using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minecraft
{
    [RequireComponent(typeof(Slider))]
    public sealed class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private TerrainManager terrainManager;

        [SerializeField]
        private Text loadingText;

        private Slider slider;

        private readonly Queue<Tuple<string, float>> messagesQueue = new Queue<Tuple<string, float>>(new[] {
            new Tuple<string, float>("Loading blocks...", 0.0f),
            new Tuple<string, float>("Charging your ki...", 0.5f),
            new Tuple<string, float>("It's over 9000!!!", 0.9f),
        });

        private void Start()
        {
            slider = GetComponent<Slider>();
        }

        private void Update()
        {
            var currentProgress = terrainManager.LoadingProgress;

            slider.value = currentProgress;

            if (messagesQueue.Count > 0 && currentProgress >= messagesQueue.Peek().Item2)
            {
                loadingText.text = messagesQueue.Dequeue().Item1;
            }
        }
    }
}