using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InGameTimer
{
    [Tooltip("Each element = (minutes, seconds) for a phase")]
    public Vector2Int[] phasesDurations;

    private int _secondsRemaining;
    private int _minutesRemaining;
    private int _currentPhaseIndex;
    private bool _isRunning;

    public int SecondsRemaining => _secondsRemaining;
    public int MinutesRemaining => _minutesRemaining;
    public int CurrentPhase => _currentPhaseIndex;
    public int TotalPhases => phasesDurations?.Length ?? 0;
    public bool IsRunning => _isRunning;

    public Action OnSecondElapsed;

    public IEnumerator CountTime()
    {
        _isRunning = true;

        for (_currentPhaseIndex = 0; _currentPhaseIndex < phasesDurations.Length; _currentPhaseIndex++)
        {
            Vector2Int phase = phasesDurations[_currentPhaseIndex];

            int totalSeconds = (phase.x * 60) + phase.y;

            while (totalSeconds > 0)
            {
                yield return new WaitForSeconds(1f);

                totalSeconds--;

                _minutesRemaining = totalSeconds / 60;
                _secondsRemaining = totalSeconds % 60;

                OnSecondElapsed?.Invoke();
            }

            
            _minutesRemaining = 0;
            _secondsRemaining = 0;
        }

        _isRunning = false;
        
    }
}
