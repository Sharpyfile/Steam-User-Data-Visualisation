using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



public class DataManager : MonoBehaviour
{
    public string WebAPIKey;
    public string WebAPIKeyFilePath;
    public string SteamID;

    [HideInInspector]
    public string json;

    private void Awake()
    {
        WebAPIKey = System.IO.File.ReadAllText(Application.dataPath + "/" + WebAPIKeyFilePath);
        
        _getData = GetData();
        StartCoroutine(_getData);
    }

    IEnumerator _getData;
    IEnumerator GetData()
    {
        string requestURL = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={WebAPIKey}&steamid={SteamID}&include_appinfo=false&include_played_free_games=false&count=3";
        UnityWebRequest www = UnityWebRequest.Get(requestURL);

        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        Debug.Log(www.downloadHandler.text.Length);
        json = www.downloadHandler.text;

        Root root = JsonUtility.FromJson<Root>(json);
        Debug.Log(root.response.game_count);
        Debug.Log(root.response.games[0].ToString());
        Debug.Log(root.response.games[315].ToString());
        Debug.Log(root.response.games.GetType().Name);
        Debug.Log(root.response.games.Count);

    }

    [Serializable]
    public class Root
    {
        public Response response;
    }

    [Serializable]
    public class Response
    {
        public int game_count;
        public List<Game> games = new List<Game>();
    }

    // You dont need to create class that takes into account everything in json


    [Serializable]
    public class Game
    {
        public int appid;
        public int playtime_forever;
        public int playtime_windows_forever;
        public int playtime_mac_forever;
        //public int playtime_linux_forever;


        public override string ToString()
        {
            //return $"AppID: {appid}, playtime_forever: {playtime_forever}, playtime_windows_forever: {playtime_windows_forever}, playtime_mac_forever: {playtime_mac_forever}, playtime_linux_forever: {playtime_linux_forever}";
            return $"AppID: {appid}, playtime_forever: {playtime_forever}, playtime_windows_forever: {playtime_windows_forever}, playtime_mac_forever: {playtime_mac_forever}";
        }
    }
}
