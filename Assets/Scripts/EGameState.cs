﻿
public enum EGameState
{
    NONE,           // Default
    IDLE,           // Before start
    TURN_IDLE,      // User picks an action
    TURN_MOVE,
    TURN_SHOOT,
    TURN_ASK,
    GAME_ANIMATION,
    GAME_PAUSE,
    GAME_FINISH
}