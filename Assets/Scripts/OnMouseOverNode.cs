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
    //LineRenderer lineRenderer;


    public Vector3 Point;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        gameInfo = GetComponent<GameInfo>();
        //lineRenderer = GetComponent<LineRenderer>();
        OriginalColor = meshRenderer.material.color;
    }

    private void OnMouseEnter()
    {
        if (gameInfo.GameApplication.steam_appid == 0)
            Modal.Instance.GameName.text = "Other games, zoom for more info";
        else
            Modal.Instance.GameName.text = gameInfo.GameApplication.name;

        Modal.Instance.GameInfoContainer.SetActive(true);

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

        //lineRenderer.gameObject.SetActive(true);
        //foreach(Transform gameItem in transform.parent)
        //{
        //    if (gameItem.GetInstanceID() == this.transform.GetInstanceID())
        //        continue;

        //    int index = 0;

        //    GameInfo gameInfo2 = gameItem.GetComponent<GameInfo>();
        //    if (gameInfo2 != null && gameInfo.GameApplication.genres.Intersect(gameInfo2.GameApplication.genres).ToList().Count > 0)
        //    {
        //        Vector3 point = gameInfo.GetComponent<OnMouseOverNode>().Point;
        //        lineRenderer.SetPosition(index, Point);
        //        lineRenderer.SetPosition(index + 1, point);
        //        index += 2;
                
        //    }

        //}



        foreach (Transform genreItem in Modal.Instance.ToggleTransform)
        {
            GenreToggle toggle = genreItem.GetComponent<GenreToggle>();
            {
                if (toggle != null)
                {
                    if (gameInfo.GameApplication.genres.Contains(toggle.GenreName))
                        toggle.RollOut();
                }    
            }
        }

        string count = "";
        if (friendsCount == 1)
            count = $"{friendsCount}\nFriends owning game ";
        else
            count = $"{friendsCount}\nFriend owning game ";
        Modal.Instance.GameFriendsCount.text = count;

        Modal.Instance.GameGenres.text = genres;
        meshRenderer.material.color = MouseOverColor;
    }

    private void OnMouseExit()
    {
        //Modal.Instance.GameInfoContainer.SetActive(false);

        meshRenderer.material.color = OriginalColor;
        Modal.Instance.GameName.text = "";
        Modal.Instance.GamePlaytime.text = "";
        Modal.Instance.GameGenres.text = "";
        Modal.Instance.GameFriendsCount.text = "";
        //lineRenderer.gameObject.SetActive(false);

        foreach (Transform friendItem in Modal.Instance.FriendsContainer)
        {
            friendItem.gameObject.SetActive(true);
        }

        foreach (Transform genreItem in Modal.Instance.ToggleTransform)
        {
            GenreToggle toggle = genreItem.GetComponent<GenreToggle>();
            {
                if (toggle != null)
                {
                    if (gameInfo.GameApplication.genres.Contains(toggle.GenreName))
                        toggle.RollIn();
                }
            }
        }
    }

}
