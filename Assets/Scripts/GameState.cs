using UnityEngine;

namespace Minecraft
{
    public interface IGameState
    {
        bool IsGodMode { get; }

        bool IsPaused { get; }

        int BlocksCount { get; }

        int FoesCount { get; }

        int TotalScore { get; }

        bool IsNight { get; }
    }

    public sealed class GameState : MonoBehaviour, IGameState
    {
        private IGameState impl = null;

        public void SetImpl(IGameState newImpl)
        {
            impl = newImpl;
        }

        public bool IsGodMode => impl.IsGodMode;

        public bool IsPaused => impl.IsPaused;

        public int BlocksCount => impl.BlocksCount;

        public int FoesCount => impl.FoesCount;

        public int TotalScore => impl.TotalScore;

        public bool IsNight => impl.IsNight;
    }
}