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

    [HideInInspector]
    public Color BackgroundColor;
    public Color SelectedColor;

    public void SetData(string genreName, Color color)
    {
        GenreName = genreName;
        Genre.text = GenreName;
        HoverColor = color;
        ColorBlock temp = Toggle.colors;
        temp.normalColor = color;
        
        temp.selectedColor = temp.normalColor;
        temp.highlightedColor = temp.normalColor;
        temp.pressedColor = temp.normalColor;
        temp.disabledColor = temp.normalColor;
        Toggle.colors = temp;

        BackgroundColor = Background.color;
        SetGenreCount();
    }

    public void SetData(string genreName)
    {
        GenreName = genreName;
        Genre.text = GenreName;
        ColorBlock temp = Toggle.colors;
        Random.InitState(genreName.GetHashCode());
        temp.normalColor = Random.ColorHSV(0.0f, 1.0f);
        HoverColor = temp.normalColor;
        temp.selectedColor = temp.normalColor;
        temp.highlightedColor = temp.normalColor;
        temp.pressedColor = temp.normalColor;
        temp.disabledColor = temp.normalColor;
        Toggle.colors = temp;

        BackgroundColor = Background.color;
        SetGenreCount();
    }

    public void OnGenreToggleClick()
    {
        var val = Toggle.isOn;
        if (val && !ChartRenderer.Instance.FilterGenres.Contains(GenreName))
        {
            ChartRenderer.Instance.FilterGenres.Add(GenreName);
            Background.color = SelectedColor;
        }            
        else
        {
            ChartRenderer.Instance.FilterGenres.Remove(GenreName);
            Background.color = BackgroundColor;
        }
            
        Modal.Instance.DrawOnValueChanged();
        Modal.Instance.DrawPoints();
    }

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
        GenreCount.text = $"{count} games";
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
    }
}
