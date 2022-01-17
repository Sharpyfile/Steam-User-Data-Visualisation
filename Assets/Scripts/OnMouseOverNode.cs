using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OnMouseOverNode : MonoBehaviour
{
    public Color MouseOverColor = Color.red;
    Color OriginalColor;

    MeshRenderer meshRenderer;
    GameInfo gameInfo;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        gameInfo = GetComponent<GameInfo>();
        OriginalColor = meshRenderer.material.color;
    }

    private void OnMouseOver()
    {
        if (gameInfo.GameApplication.steam_appid == 0)
            Modal.Instance.GameName.text = "Other games, zoom for more info";
        else
            Modal.Instance.GameName.text = gameInfo.GameApplication.name;

        float hours = gameInfo.OwnedGame.playtime_forever / 60.0f;
        Modal.Instance.GamePlaytime.text = $"{(float)Mathf.Round(hours * 100.0f) / 100f} h";
        string genres = "";

        foreach(string genre in gameInfo.GameApplication.genres)
        {
            genres += genre + "\n";
        }

        int friendsCount = 0;
        foreach (Transform friendItem in Modal.Instance.FriendsContainer)
        {
            FriendItem friend = friendItem.GetComponent<FriendItem>();
            if (friend != null)
            {
                if (friend.FriendData.games.FirstOrDefault(item => item.appid == gameInfo.GameApplication.steam_appid) == null)
                    friendItem.gameObject.SetActive(false);
                else
                    ++friendsCount;
            }
        }

        Modal.Instance.GameFriendsCount.text = $"Friends owning game: {friendsCount}";

        Modal.Instance.GameGenres.text = genres;
        meshRenderer.material.color = MouseOverColor;
    }

    private void OnMouseExit()
    {
        meshRenderer.material.color = OriginalColor;
        Modal.Instance.GameName.text = "";
        Modal.Instance.GamePlaytime.text = "";
        Modal.Instance.GameGenres.text = "";
        Modal.Instance.GameFriendsCount.text = "";

        foreach (Transform friendItem in Modal.Instance.FriendsContainer)
        {
            friendItem.gameObject.SetActive(true);
        }
    }

}
