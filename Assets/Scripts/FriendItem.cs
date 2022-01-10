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

    public void SetData(Texture2D texture, string name, Friend data)
    {
        FriendData = data;
        FriendName.text = name;
        FriendAvatar.texture = texture;
    }
}
