using UnityEngine;

public class InteractionRequest
{
    public Vector3 playerPos;
    public Vector3 targetPos;
    public PlayerStats playerStats;
    
    public static bool IsInRange(Vector3 playerPos, Vector3 targetPos, float maxRange)
    {
        return Vector3.Distance(playerPos, targetPos) <= maxRange;
    }
}
