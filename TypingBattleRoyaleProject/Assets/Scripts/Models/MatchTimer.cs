using System;

public class MatchTimer
{
    public float TimeRemaining { get; private set; }
    public bool IsRunning { get; private set; }

    public Action OnTimerEnd;

    public void Start(float duration)
    {
        TimeRemaining = duration;
        IsRunning = true;
    }

    public void Tick(float deltaTime)
    {
        if (!IsRunning) return;

        TimeRemaining -= deltaTime;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;

            OnTimerEnd?.Invoke();
        }
    }
}
