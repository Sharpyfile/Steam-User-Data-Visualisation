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

    public Transform PositionTransform;

    public int XOffset = -40;
    public float Time = 0.3f;

    Vector3 _originPosition;

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

    private void Update()
    {
        if (percent < 1)
        {
            PositionTransform.position = Vector3.Lerp(oldPosition, newPosition, percent);

            percent = (timeLeft / Time);
            timeLeft += UnityEngine.Time.deltaTime;
        }
    }


    float percent = 1.0f;
    float timeLeft;
    Vector3 oldPosition;
    Vector3 newPosition;

    public void RollOut()
    {
        float distancePoint = Mathf.Abs((_originPosition.x - PositionTransform.position.x));
        // [0 - 1], it gets for how long it needs to be moved
        percent = distancePoint / Mathf.Abs(XOffset);
        timeLeft = Time * percent;
        Debug.Log(percent);
        oldPosition = PositionTransform.position;
        newPosition = oldPosition;
        newPosition.x = _originPosition.x + XOffset;
    }

    public void RollIn()
    {
        float distancePoint = Mathf.Abs((_originPosition.x - PositionTransform.position.x));

        // [0 - 1], it gets for how long it needs to be moved
        percent = 1 - distancePoint / Mathf.Abs(XOffset);
        timeLeft = Time * percent;
        Debug.Log(percent);
        oldPosition = PositionTransform.position;
        newPosition = oldPosition;
        newPosition.x = _originPosition.x;
    }
}
