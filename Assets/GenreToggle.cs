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
    private Color _originalColor;

    public void SetData(string genreName)
    {
        GenreName = genreName;
        Genre.text = GenreName;
    }

    public void OnGenreToggleClick()
    {
        var val = Toggle.isOn;
        
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
                _originalColor = gameInfo.GetComponent<MeshRenderer>().material.color;
                gameInfo.GetComponent<MeshRenderer>().material.color = HoverColor;
                ++count;
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
                gameInfo.GetComponent<MeshRenderer>().material.color = _originalColor;
            }
        }

        GenreCount.text = "";
    }
}
