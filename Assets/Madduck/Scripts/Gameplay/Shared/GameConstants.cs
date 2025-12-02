using R3;

namespace Madduck.Shared
{
    public enum GameState
    {
        Normal,
        Paused,
    }
    public static class GameConstants
    {
        private static readonly ReactiveProperty<GameState> _currentGameState = new(GameState.Normal);
        public static ReadOnlyReactiveProperty<GameState> CurrentGameState => 
            _currentGameState.Select(g => g).ToReadOnlyReactiveProperty();

        public static void SetGameState(GameState newState)
        {
            _currentGameState.Value = newState;
        }
    }
}