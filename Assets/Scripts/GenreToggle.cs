using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static DataManager;

public class GenreToggle : MonoBehaviour
{
    // Start is called before the first frame update
    public string GenreName;
    public Text Genre;
    public Text GenreCount;
    public Toggle Toggle;
    public Color HoverColor;
    public Image Background;
    public Image FrameImage;

    public Transform PositionTransform;

    public int XOffset = 40;
    public float Time = 10.0f;

    Vector3 _originPosition;

    [HideInInspector]
    public Color BackgroundColor;

    public void SetData(string genreName, Color color)
    {
        _originPosition = PositionTransform.position;
        GenreName = genreName;
        Genre.text = GenreName;
        HoverColor = color;
        ColorBlock temp = Toggle.colors;
        FrameImage.color = color;

        BackgroundColor = Background.color;
        SetGenreCount();
    }

    public void SetData(string genreName)
    {
        _originPosition = PositionTransform.position;
        GenreName = genreName;
        Genre.text = GenreName;
        ColorBlock temp = Toggle.colors;
        Random.InitState(genreName.GetHashCode());
        temp.normalColor = Random.ColorHSV(0.0f, 1.0f);
        HoverColor = temp.normalColor;
        FrameImage.color = temp.normalColor;

        BackgroundColor = Background.color;
        SetGenreCount();
    }

    public void OnGenreToggleClick()
    {
        var val = Toggle.isOn;
        if (val && !ChartRenderer.Instance.FilterGenres.Contains(GenreName))
        {
            ChartRenderer.Instance.FilterGenres.Add(GenreName);
        }            
        else
        {
            ChartRenderer.Instance.FilterGenres.Remove(GenreName);
        }
            
        Modal.Instance.DrawOnValueChanged();
        Modal.Instance.DrawPoints();
    }

    #region On Hover

    public void OnHoverEnter()
    {
        int count = 0;
        foreach (Transform game in ChartRenderer.Instance.transform)
        {
            GameInfo gameInfo = game.GetComponent<GameInfo>();
            if (gameInfo == null)
                continue;

            if (gameInfo.GameApplication.genres.Contains(GenreName))
            {
                gameInfo.OriginalColor = gameInfo.GetComponent<MeshRenderer>().material.color;
                gameInfo.GetComponent<MeshRenderer>().material.color = HoverColor;
                ++count;

                game.transform.position = new Vector3(game.transform.position.x, ChartRenderer.Instance.HeightDifference, game.transform.position.z);

            }
        }
        RollOut();

        GenreCount.text = $"{count} games";
    }


    public void OnHoverExit()
    {
        foreach (Transform game in ChartRenderer.Instance.transform)
        {
            GameInfo gameInfo = game.GetComponent<GameInfo>();
            if (gameInfo == null)
                continue;

            if (gameInfo.GameApplication.genres.Contains(GenreName))
            {
                gameInfo.GetComponent<MeshRenderer>().material.color = gameInfo.OriginalColor;
                game.transform.position = new Vector3(game.transform.position.x, 0, game.transform.position.z);
            }
        }

        RollIn();
    }

    #endregion

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
        percent = distancePoint / XOffset;
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
        percent = 1 - distancePoint / XOffset;
        timeLeft = Time * percent;
        Debug.Log(percent);
        oldPosition = PositionTransform.position;
        newPosition = oldPosition;
        newPosition.x = _originPosition.x;
    }

    public void SetGenreCount()
    {
        int count = 0;
        foreach (SteamApplication app in ChartRenderer.Instance.CurrentGames)
        {
            if (app.genres.Contains(GenreName))
                ++count;
        }
        GenreCount.text = $"{count} games";
    }
}
