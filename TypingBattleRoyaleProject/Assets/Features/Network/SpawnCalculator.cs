using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class SpawnCalculator
{
    private Vector3[] _shuffle;
    public Vector3[] shufflePoints { get { return _shuffle; } }
    private int _nextIndex;

    public SpawnCalculator(Vector3[] spawnPoints)
    {
        _shuffle = new Vector3[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            _shuffle[i] = spawnPoints[i];
        }

        Shuffle();

        _nextIndex = 0;
    }

    private void Shuffle()
    {
        for (int i = _shuffle.Length - 1; i >= 1; i--)
        {
            int j = Random.Range(0, i + 1);

            Vector3 temp = _shuffle[i];
            _shuffle[i] = _shuffle[j];
            _shuffle[j] = temp;
        }
    }

    public Vector3 GetSpawnPoint()
    {
        if (_nextIndex >= _shuffle.Length)
        {
            Debug.LogWarning("No quedan Spawnpoints disponibles");
            return Vector3.zero;
        }

        Vector3 result = _shuffle[_nextIndex];
        _nextIndex++;

        return result;
    }




}
