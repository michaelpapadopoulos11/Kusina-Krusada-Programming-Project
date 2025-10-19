using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;

    public static int Score { get; private set; } = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void AddPoints(int amount)
    {
        Score += amount;
    }

    public static void ResetScore()
    {
        Score = 0;
    }
}
