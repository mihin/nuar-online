
public enum EGameState
{
    NONE,           // Default
    IDLE,           // Before start
    ERROR,          // Idle with error message
    GAME_START,
    TURN_IDLE,      // User picks an action
    TURN_MOVE,
    TURN_SHOOT,
    TURN_ASK,
    TURN_OPPORNENT_CONFIRMED,
    TURN_FINISH,
    GAME_ANIMATION,
    GAME_PAUSE,
    GAME_FINISH
}