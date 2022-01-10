using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenreToggle : MonoBehaviour
{
    // Start is called before the first frame update
    public string GenreName;
    public Text Genre;
    public Text GenreCount;
    public Toggle Toggle;
    public Color HoverColor;

    public void SetData(string genreName)
    {
        GenreName = genreName;
        Genre.text = GenreName;
    }

    public void OnGenreToggleClick()
    {
        var val = Toggle.isOn;
        if (val && !ChartRenderer.Instance.FilterGenres.Contains(GenreName))
            ChartRenderer.Instance.FilterGenres.Add(GenreName);
        else
            ChartRenderer.Instance.FilterGenres.Remove(GenreName);

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

        GenreCount.text = "";
    }
}
