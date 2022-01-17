using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DataManager;

public class FriendItem : MonoBehaviour
{
    public RawImage FriendAvatar;
    public Text FriendName;
    public Friend FriendData;
    public Toggle Toggle;
    public Image Background;

    [HideInInspector]
    public Color BackgroundColor;
    public Color SelectedColor;


    public void SetData(Texture2D texture, string name, Friend data)
    {
        FriendData = data;
        FriendName.text = name;
        FriendAvatar.texture = texture;
        BackgroundColor = Background.color;
    }

    public void OnFriendToggleClick()
    {
        var val = Toggle.isOn;
        if (val && !ChartRenderer.Instance.FilterFriends.Contains(FriendData))
        {
            ChartRenderer.Instance.FilterFriends.Add(FriendData);
            Background.color = SelectedColor;

            foreach(Transform friendItem in Modal.Instance.FriendsContainer)
            {
                FriendItem friend = friendItem.GetComponent<FriendItem>();
                if (friend != null)
                {
                    if (friend.Toggle.isOn)
                        friend.transform.SetAsFirstSibling();
                }
            }
            Modal.Instance.FriendsContainer.localPosition = Vector3.zero;

            transform.SetAsFirstSibling();
        }
        else
        {
            ChartRenderer.Instance.FilterFriends.Remove(FriendData);
            Background.color = BackgroundColor;

            foreach (Transform friendItem in Modal.Instance.FriendsContainer)
            {
                FriendItem friend = friendItem.GetComponent<FriendItem>();
                if (friend != null)
                {
                    if (friend.Toggle.isOn)
                        friend.transform.SetAsFirstSibling();
                }
            }
            Modal.Instance.FriendsContainer.localPosition = Vector3.zero;
        }

        Modal.Instance.DrawOnValueChanged();
        Modal.Instance.DrawPoints();
    }
}
