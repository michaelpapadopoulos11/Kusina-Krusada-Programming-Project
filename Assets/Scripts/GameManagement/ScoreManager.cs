using System;

// Static score manager: no GameObject required, no DontDestroyOnLoad side-effects.
public static class ScoreManager
{
    // Current score
    public static int Score { get; private set; } = 0;

    // Add points to the global score
    public static void AddPoints(int amount)
    {
        Score += amount;
    }

    // Reset score to zero
    public static void ResetScore()
    {
        Score = 0;
    }
}
