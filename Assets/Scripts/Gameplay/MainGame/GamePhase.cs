namespace Game.Gameplay
{
    public enum GamePhase
    {
        EchoLocation,
        Planning,
        Perform
    }

    public enum GameState
    {
        Idle,
        Loading,
        Start,
        EchoLocation,
        Plan,
        Perform,
        End
    }
}