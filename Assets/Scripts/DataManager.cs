using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public string WebAPIKey;
    public string WebAPIKeyFilePath;
    public string SteamID;

    public List<int> IgnoredAppIDs = new List<int>();
    public List<string> Genres = new List<string>();
    public List<string> BlockedGenres = new List<string>();

    [HideInInspector]
    public string json;

    public List<SteamApplication> steamApplications = new List<SteamApplication>();
    public Response Games;

    public List<Game> OwnedGames = new List<Game>();

    public bool IsDone = false;

    [Header("WARNING: chart will not be rendered properly with games having 0 minutes in")]
    public bool IgnoreZeroPlaytime = true;

    public GameObject TogglePrefab;

    public List<Friend> Friends = new List<Friend>();

    // Temporary list of games used for storing steamappid's extracted in IEnumerator
    List<Game> tempGames = new List<Game>();

    public DataManager(bool isDone)
    {
        IsDone = isDone;
    }

    private void Awake()
    {
        Instance = this;
        WebAPIKey = System.IO.File.ReadAllText(Application.dataPath + "/" + WebAPIKeyFilePath);

        _getData = GetData();

        if (!IsDone)
            StartCoroutine(_getData);
    }

    IEnumerator _getData;
    IEnumerator GetData()
    {
        string requestURLFriends = $"https://api.steampowered.com/ISteamUser/GetFriendList/v1/?key={WebAPIKey}&steamid={SteamID}";
        UnityWebRequest wwwFriends = UnityWebRequest.Get(requestURLFriends);

        yield return wwwFriends.SendWebRequest();

        string friendsJson = wwwFriends.downloadHandler.text;

        RootFriends tempRoot = JsonUtility.FromJson<RootFriends>(friendsJson);

        foreach (Friend friend in tempRoot.friendslist.friends)
        {
            yield return GetGamesForSteamID(friend.steamid);
            friend.games = this.tempGames;
        }

        Friends = tempRoot.friendslist.friends;

        yield return GetGamesForSteamID(SteamID);
        Games.games = tempGames;
        Games.game_count = tempGames.Count;

        OwnedGames = Games.games;

        foreach (Game game in Games.games)
        {
            if (IgnoredAppIDs.Contains(game.appid))
                continue;

            if (IgnoreZeroPlaytime && game.playtime_forever <= 0)
                continue;

            string requestURL = $"https://store.steampowered.com/api/appdetails?appids=" + game.appid.ToString() + "&l=english";
            UnityWebRequest www = UnityWebRequest.Get(requestURL);

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

            bool skipApp = false;

            foreach (string genre in app.genres)
            {
                if (BlockedGenres.Contains(genre))
                    skipApp = true;
            }

            if (skipApp)
                continue;

            foreach (string genre in app.genres)
            {
                if (!Genres.Contains(genre))
                    Genres.Add(genre);
            }

            app.genres.Sort();

            steamApplications.Add(app);
        }

        Debug.Log("Done scrapping");
        IsDone = true;

        int index = 0;
        foreach(string genre in Genres)
        {
            GameObject var = Instantiate(TogglePrefab, Modal.Instance.ToggleTransform);
            if (index < Modal.Instance.GenreColors.Count)
                var.GetComponent<GenreToggle>().SetData(genre, Modal.Instance.GenreColors[index]);
            else
                var.GetComponent<GenreToggle>().SetData(genre);
            index++;
        }
    }   
    
    IEnumerator GetGamesForSteamID(string steamID)
    {
        string requestURL = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={WebAPIKey}&steamid={steamID}&include_appinfo=false&include_played_free_games=true&count=3";
        UnityWebRequest www = UnityWebRequest.Get(requestURL);

        yield return www.SendWebRequest();
        json = www.downloadHandler.text;
        Root root = JsonUtility.FromJson<Root>(json);
        tempGames = root.response.games;
    }


    [Serializable]
    public class Root
    {
        public Response response;
    }

    [Serializable]
    public class RootFriends
    {
        public ResponseFriends friendslist;
    }

    [Serializable]
    public class Response
    {
        public int game_count;
        public List<Game> games = new List<Game>();
    }
    [Serializable]
    public class ResponseFriends
    {
        public List<Friend> friends = new List<Friend>();
    }

    [Serializable]
    public class Friend
    {
        public string steamid;
        public List<Game> games = new List<Game>();
    }


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
