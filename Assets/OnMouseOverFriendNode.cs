using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseOverFriendNode : MonoBehaviour
{
    public Color MouseOverColor = Color.red;
    Color OriginalColor;

    MeshRenderer meshRenderer;
    public string SteamID;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        OriginalColor = meshRenderer.material.color;
    }

    private void OnMouseEnter()
    {
        foreach (Transform friendItem in Modal.Instance.FriendsContainer)
        {
            FriendItem friend = friendItem.GetComponent<FriendItem>();
            if (friend != null)
            {
                if (friend.FriendData.steamid == SteamID)
                {
                    friendItem.gameObject.SetActive(true);
                    Modal.Instance.GameFriendsCount.text = friend.FriendName.text;
                }
                else
                    friendItem.gameObject.SetActive(false);

            }
        }
        meshRenderer.material.color = MouseOverColor;
    }

    private void OnMouseExit()
    {
        meshRenderer.material.color = OriginalColor;
        Modal.Instance.GameFriendsCount.text = "";
        foreach (Transform friendItem in Modal.Instance.FriendsContainer)
        {
            FriendItem friend = friendItem.GetComponent<FriendItem>();
            friendItem.gameObject.SetActive(true);
        }
    }
}
