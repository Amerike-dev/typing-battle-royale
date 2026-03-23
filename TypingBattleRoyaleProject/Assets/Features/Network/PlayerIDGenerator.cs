using System.Collections.Generic;
using UnityEngine;

public static class PlayerIDGenerator
{
    private static HashSet<string> _usersID = new HashSet<string>();

    private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string _prefix = "WIZ-";
    private const int _idLenght = 4;
    private const int _maxAttempts = 10;

    public static string GenerateID()
    {
        for (int attempt = 0; attempt < _maxAttempts; attempt++)
        {
            string newId = _prefix + GenerateRandomSegment();

            if (!_usersID.Contains(newId))
            {
                _usersID.Add(newId);
                return newId;
            }
        }

        Debug.LogError("Cant generate ID");
        return null;
    }

    public static void ResetIDs()
    {
        _usersID.Clear();
    }

    private static string GenerateRandomSegment()
    {
        char[] segment = new char[_idLenght];

        for (int i = 0; i < _idLenght; i++)
        {
            segment[i] = _chars[Random.Range(0, _chars.Length)];
        }

        return new string(segment);
    }
}
