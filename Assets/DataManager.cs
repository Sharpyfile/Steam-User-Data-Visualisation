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

    public List<int> IgnoredAppIDs = new List<int>();

    [HideInInspector]
    public string json;

    public List<SteamApplication> steamApplications = new List<SteamApplication>();
    public Response Games;

    public int MinimumPlaytime = 60;

    public bool IsDone = false;

    private void Awake()
    {
        WebAPIKey = System.IO.File.ReadAllText(Application.dataPath + "/" + WebAPIKeyFilePath);

        _getData = GetData();
        StartCoroutine(_getData);
    }

    IEnumerator _getData;
    IEnumerator GetData()
    {
        string requestURL = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={WebAPIKey}&steamid={SteamID}&include_appinfo=false&include_played_free_games=true&count=3";
        UnityWebRequest www = UnityWebRequest.Get(requestURL);

        yield return www.SendWebRequest();
        json = www.downloadHandler.text;

        Root root = JsonUtility.FromJson<Root>(json);
        Debug.Log(root.response.game_count);

        // Remove all games that have 0 playtime
        root.response.games.RemoveAll(item => item.playtime_forever <= MinimumPlaytime);
        root.response.game_count = root.response.games.Count;

        Games = root.response;

        foreach (Game game in root.response.games)
        {
            if (IgnoredAppIDs.Contains(game.appid))
                continue;

            requestURL = $"https://store.steampowered.com/api/appdetails?appids=" + game.appid.ToString() + "&l=english";
            www = UnityWebRequest.Get(requestURL);

            yield return www.SendWebRequest();
            json = www.downloadHandler.text;

            SteamApplication app = new SteamApplication();
            int indexof = json.IndexOf("name\":");
            if (indexof < 0)
                continue;

            app.steam_appid = game.appid;

            app.name = json.Substring(indexof + 7);
            app.name = app.name.Split(',')[0];
            app.name = app.name.Trim('"');
            app.name = app.name.Trim('\'');

            app.ExtractGenres(json);

            steamApplications.Add(app);
        }

        Debug.Log("Done scrapping");
        IsDone = true;
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
        public List<string> genres = new List<string>();
        public string name;
        public int steam_appid;

        public void ExtractGenres(string json)
        {
            // Check scrapping for Sanctum 2, it gives from description as it seems
            string temp = json.Substring(json.IndexOf("genres") + 8);
            temp = temp.Split(']')[0];
            string[] genres = temp.Split('}');

            for (int i = 0; i < genres.Length - 1; ++i)
            {
                int indexOf = genres[i].IndexOf("description");
                if (indexOf < 0)
                    continue;

                string temp2 = genres[i].Substring(genres[i].IndexOf("description") + "description".Length + 2);
                temp2 = temp2.Replace('"', ' ');
                temp2 = temp2.Trim();
                this.genres.Add(temp2);
            }                       
        }

        public override string ToString()
        {
            string temp = "";
            foreach(string genre in genres)
            {
                temp += genre + " ";
            }
            return $"AppID: {steam_appid}, name: {name}, genres: {temp}";
        }
    }
}
