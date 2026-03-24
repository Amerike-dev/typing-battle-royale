using UnityEngine;

public class PlayerStats
{
    private string _id;
    private int _health;

    public PlayerStats(string id, int health)
    {
        this._id = id;
        this._health = health;
    }

    public string ID {  get { return _id; } }
}
