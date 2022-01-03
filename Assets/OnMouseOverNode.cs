using System.Collections;
using System.Collections.Generic;
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
        Modal.Instance.GameName.text = gameInfo.GameApplication.name;
        Modal.Instance.GamePlaytime.text = $"{gameInfo.OwnedGame.playtime_forever / 60.0f} h";
        string genres = "";

        foreach(string genre in gameInfo.GameApplication.genres)
        {
            genres += genre + "\n";
        }

        Modal.Instance.GameGenres.text = genres;
        meshRenderer.material.color = MouseOverColor;
    }

    private void OnMouseExit()
    {
        meshRenderer.material.color = OriginalColor;
        Modal.Instance.GameName.text = "";
        Modal.Instance.GamePlaytime.text = "";
        Modal.Instance.GameGenres.text = "";
    }
}
