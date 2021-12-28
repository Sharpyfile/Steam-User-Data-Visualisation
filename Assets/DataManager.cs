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

    public List<SteamApplication> steamApplications = new List<SteamApplication>();

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

        json = www.downloadHandler.text;

        Root root = JsonUtility.FromJson<Root>(json);
        Debug.Log(root.response.game_count);

        

        foreach(Game game in root.response.games)
        {
            requestURL = $"https://store.steampowered.com/api/appdetails?appids=" + game.appid.ToString() + "&l=english";
            www = UnityWebRequest.Get(requestURL);

            yield return www.SendWebRequest();
            json = www.downloadHandler.text;

            SteamApplication app = new SteamApplication();
            int indexof = json.IndexOf("name");
            if (indexof < 0)
                continue;

            app.name = json.Substring(indexof + 6);
            app.name = app.name.Split(',')[0];
            app.name = app.name.Trim('"');
            app.name = app.name.Trim('\'');
            app.ExtractGenres(json);

            app.steam_appid = game.appid;

            steamApplications.Add(app);
        }

        Debug.Log("Done scrapping");
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
    [Serializable]
    public class SteamApplication
    {
        public string genre;
        public string name;
        public int steam_appid;

        public void ExtractGenres(string json)
        {
            genre = json.Substring(json.IndexOf("genres") + 8);
            genre = genre.Split(']')[0];
            string[] genres = genre.Split('}');

            genre = "";

            for (int i = 0; i < genres.Length - 1; ++i)
            {
                int indexOf = genres[i].IndexOf("description");
                if (indexOf < 0)
                    continue;

                genre += genres[i].Substring(genres[i].IndexOf("description") + "description".Length + 2);
                if (i < genres.Length - 2)
                    genre += "-";
            }

            genre = genre.Replace('"', ' ');
            genre = genre.Trim();
            
            Debug.Log(genre);
        }

        public override string ToString()
        {
            return $"AppID: {steam_appid}, name: {name}, genre: {genre}";
        }
    }
}
