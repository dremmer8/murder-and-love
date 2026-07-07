using System;

public enum GameState
{
    Gameplay,
    Dialogue,
    Paused
}

public static class GameStateManager
{
    public static GameState CurrentState { get; private set; } = GameState.Gameplay;
    
    public static event Action<GameState> OnGameStateChanged;

    public static void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}