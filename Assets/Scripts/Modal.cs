using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DataManager;
using UnityEngine.Networking;

public class Modal : MonoBehaviour
{
    public static Modal Instance;

    public GameObject GameInfoContainer;

    public Text GameName;
    public Text GamePlaytime;
    public Text GameGenres;
    public Text GameFriendsCount;

    public GameObject ProgressContainer;
    public Text ProgressText;

    public GameObject SliderContainer;
    public Text MinPlaytimeSliderText;
    public Text MaxPlaytimeSliderText;
    public Text MinPlaytimeText;
    public Text MaxPlaytimeText;
    public Text SelectedPlaytime;

    public Slider MinSlider;
    public Slider MaxSlider;

    public Transform ToggleTransform;

    public Transform PointContainer;
    public Transform MinPoint;
    public Transform MaxPoint;

    public GameObject PointPrefab;

    public Transform FriendsContainer;
    public FriendItem FriendsPrefab;

    public List<Color> GenreColors = new List<Color>();


    public void Awake()
    {
        Instance = this;
        _sliderUpdate = SliderUpdate();
        StartCoroutine(_sliderUpdate);

        _friendsUpdate = FriendsUpdate();
        StartCoroutine(_friendsUpdate);
    }

    IEnumerator _friendsUpdate;
    IEnumerator FriendsUpdate()
    {
        for (; ; )
        {
            yield return new WaitForSecondsRealtime(0.1f);

            ProgressText.text = $"Progress: {DataManager.Instance.steamApplications.Count}/{DataManager.Instance.OwnedGames.Count}";

            if (DataManager.Instance.IsDone)
                break;
        }

        ProgressContainer.SetActive(false);

        foreach(Friend friend in DataManager.Instance.Friends)
        {
            var friendItem = Instantiate(FriendsPrefab, FriendsContainer);

            string requestURL = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={DataManager.Instance.WebAPIKey}&steamids={friend.steamid}";
            UnityWebRequest wwwFriends = UnityWebRequest.Get(requestURL);

            yield return wwwFriends.SendWebRequest();

            string json = wwwFriends.downloadHandler.text;
            string friendName = "";
            int indexOf = json.IndexOf("personaname\":"); // 11 + 3
            if (indexOf < 0)
                continue;

            friendName = json.Substring(indexOf + 14);
            friendName = friendName.Split(',')[0];
            friendName = friendName.Trim('"');
            friendName = friendName.Trim('\'');

            string avatarURL = "";
            indexOf = json.IndexOf("avatarfull\":"); // 10 + 3
            if (indexOf < 0)
                continue;

            avatarURL = json.Substring(indexOf + 13);
            avatarURL = avatarURL.Split(',')[0];
            avatarURL = avatarURL.Trim('"');
            avatarURL = avatarURL.Trim('\'');

            wwwFriends = UnityWebRequest.Get(avatarURL);
            wwwFriends.downloadHandler = new DownloadHandlerTexture();
            yield return wwwFriends.SendWebRequest();

            Texture2D temp = DownloadHandlerTexture.GetContent(wwwFriends);

            friendItem.SetData(temp, friendName, friend);
            //friendItem.SetData(,,friend);
        }
    }

    IEnumerator _sliderUpdate;
    IEnumerator SliderUpdate()
    {
        for(; ; )
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (DataManager.Instance.IsDone)
                break;
        }

        SliderContainer.SetActive(true);

        // make setup - First 
        MaxSlider.minValue = 1;
        MaxSlider.value = 1;
        MaxSlider.maxValue = DataManager.Instance.OwnedGames.Max(item => item.playtime_forever);
        // Add 1 to the max so it can be selected
        MaxSlider.maxValue = (MaxSlider.maxValue / 60) + 1;
        MaxSlider.value = MaxSlider.maxValue;
        MinSlider.minValue = 1;
        MinSlider.maxValue = MaxSlider.maxValue;
        MinSlider.onValueChanged.AddListener(delegate { DrawOnValueChanged(); });
        MaxSlider.onValueChanged.AddListener(delegate { DrawOnValueChanged(); });

        DrawPoints();

        for (; ; )
        {
            // Sanitize input from sliders so minSlider wont pass maxSlider and vice versa
            if (MinSlider.value > MaxSlider.value)
                MinSlider.value = MaxSlider.value;

            MinPlaytimeText.text = (MinSlider.minValue).ToString() + "h";
            MinPlaytimeSliderText.text = (MinSlider.value).ToString() + "h";
            MaxPlaytimeText.text = ((int)(MaxSlider.maxValue + 1)).ToString() + "h";
            MaxPlaytimeSliderText.text = (MaxSlider.value).ToString() + "h";

            yield return new WaitForEndOfFrame();
        }
    }

    public void DrawOnValueChanged()
    {
        ChartRenderer.Instance.DrawSunburst(MinSlider.value * 60, MaxSlider.value * 60);
        SetToggleCount();
    }

    public void SetToggleCount()
    {
        foreach (Transform toggle in ToggleTransform)
        {
            toggle.gameObject.GetComponent<GenreToggle>().SetGenreCount();
        }
    }


    public void DrawPoints()
    {
        for(int i = PointContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(PointContainer.GetChild(i).gameObject);
        }        

        List<Game> allowedGames = new List<Game>();

        foreach (DataManager.Game game in DataManager.Instance.OwnedGames)
        {
            bool skipGame = false;

            var app = DataManager.Instance.steamApplications.FirstOrDefault(item => item.steam_appid == game.appid);

            if (app == null)
                continue;

            foreach (string genre in ChartRenderer.Instance.FilterGenres)
            {
                if (!app.genres.Contains(genre))
                {
                    skipGame = true;
                    break;
                }
            }

            if (skipGame)
                continue;

            foreach (Friend friend in ChartRenderer.Instance.FilterFriends)
            {
                if (friend.games.FirstOrDefault(item => item.appid == app.steam_appid) == null)
                {
                    skipGame = true;
                    break;
                }
            }

            if (skipGame)
                continue;

            allowedGames.Add(game);
        }

        if (allowedGames.Count <= 0)
            return;

        int maxTime = allowedGames.Max(item => item.playtime_forever);

        // make setup - First 
        MaxSlider.minValue = allowedGames.Min(item => item.playtime_forever);
        if (MaxSlider.minValue == 0)
            MaxSlider.minValue = 1;
        MaxSlider.minValue = allowedGames.Min(item => item.playtime_forever);
        MaxSlider.value = MaxSlider.minValue;
        MaxSlider.maxValue = allowedGames.Max(item => item.playtime_forever) + 1;
        // Add 1 to the max so it can be selected
        MaxSlider.maxValue = (MaxSlider.maxValue / 60) + 1;
        MaxSlider.value = MaxSlider.maxValue;
        MinSlider.minValue = MaxSlider.minValue;
        MinSlider.maxValue = MaxSlider.maxValue;

        foreach(Game game1 in allowedGames)
        {
            var point = Instantiate(PointPrefab, PointContainer);
            point.gameObject.transform.position = Vector2.Lerp(MinPoint.position, MaxPoint.position, (game1.playtime_forever * 1.0f) / maxTime);
        }
    }
}
