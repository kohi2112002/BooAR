using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
[System.Serializable]
public class Player
{
    public string imagePath;
    public string emailName;
    public int sendCount;

    public Player(string _imagePath, string _emailName, int _SendCount)
    {
        this.imagePath = _imagePath;
        this.emailName = _emailName;
        this.sendCount = _SendCount;
    }
}

public static class PlayerData
{
    public static List<Player> players = new List<Player>();
    public static void AddPlayer(Player _player)
    {
        players.Add(_player);
    }
    public static Player GetPlayer(int _id)
    {
        if (players.Count != 0)
            return players[_id];
        else return null;
    }
    public static bool RemovePlayer(int _id)
    {
        if (players[_id] != null)
        {
            players.RemoveAt(_id);
            return true;
        }
        return false;
    }
    public static bool RemoveAll()
    {
        players.Clear();
        return true;
    }

    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/player");
        bf.Serialize(file, players);
        file.Close();
    }
    public static bool Load()
    {
        if (File.Exists(Application.persistentDataPath + "/player"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player", FileMode.Open);
            players = (List<Player>)bf.Deserialize(file);
            file.Close();
            return true;
        }
        else
        {
            return false;
        }
    }


}
