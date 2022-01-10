using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static DataManager;

public class ChartRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    public static ChartRenderer Instance;

    public DataManager Data;
    public float RadiusInner;
    public float RadiusOuter;

    public int MinimumPlaytime = 300;

    public GameObject NodePrefab;
    public GameObject SpheresContainer;

    [Header("Height difference between nodes")]
    public float HeightDifference;

    public List<string> FilterGenres = new List<string>();

    private void Start()
    {
        Instance = this;

        _drawSunburstChart = DrawSunburstChart();
        StartCoroutine(_drawSunburstChart);
    }

    IEnumerator _drawSunburstChart;
    
    IEnumerator DrawSunburstChart()
    {
        for (; ; )
        {
            if (Data.IsDone)
                break;
            yield return new WaitForSecondsRealtime(0.5f);
        }


        DrawSunburst(1, 999999);

        yield break;
    }

    /// <summary>
    /// Draws sunburst chart
    /// </summary>
    /// <param name="arcDegrees"> Arc degree, needs to be in radians</param>
    public void DrawSunburst(float minPlaytime, float maxPlaytime)
    {
        //if (transform.childCount == Data.Games.games.FindAll(item => item.playtime_forever < minPlaytime || item.playtime_forever > maxPlaytime).Count)
        //    return;

        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        float arcDegrees = 0.0f;
        arcDegrees = arcDegrees * Mathf.Deg2Rad;

        Data.Games.games = Data.Games.games.OrderByDescending(item => item.playtime_forever).ToList();

        int playtimeSum = 0;

        foreach (Game game in Data.Games.games)
        {
            var app = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            if (app == null)
                continue;

            if (game.playtime_forever < minPlaytime || game.playtime_forever > maxPlaytime)
                continue;

            bool skipGame = false;

            foreach(string genre in FilterGenres)
            {
                if (!app.genres.Contains(genre))
                {
                    skipGame = true;
                    break;
                }                    
            }

            if (skipGame)
                continue;

            playtimeSum += game.playtime_forever;           
        }

        float hours = playtimeSum / 60.0f;
        Modal.Instance.SelectedPlaytime.text = $"Selected playtime: {(float)Mathf.Round(hours * 100.0f) / 100f} h";

        float remainingAngle = 0.0f;
        float remainingHours = 0.0f;

        foreach (Game game in Data.Games.games)
        {
            var app = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            if (app == null)
                continue;

            if (game.playtime_forever < minPlaytime || game.playtime_forever > maxPlaytime)
                continue;

            bool skipGame = false;

            foreach (string genre in FilterGenres)
            {
                if (!app.genres.Contains(genre))
                {
                    skipGame = true;
                    break;
                }
            }

            if (skipGame)
                continue;

            float time = game.playtime_forever * 100.0f / playtimeSum;
            float angle = 360.0f * time / 100.0f;

            if (angle < 1.0f)
            {
                remainingAngle += angle;
                remainingHours += game.playtime_forever;
                continue;
            }

            Mesh mesh = GenerateNodeMesh(angle, arcDegrees, out arcDegrees, time);

            GameObject newObject2 = Instantiate(NodePrefab, transform);
            newObject2.GetComponent<MeshFilter>().mesh = mesh;
            newObject2.GetComponent<GameInfo>().OwnedGame = game;
            newObject2.GetComponent<GameInfo>().GameApplication = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            newObject2.GetComponent<MeshCollider>().sharedMesh = newObject2.GetComponent<MeshFilter>().mesh;

            Random.InitState(game.appid);

            Color randomColor = new Color(
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f));

            newObject2.GetComponent<MeshRenderer>().material.color = randomColor;
            newObject2.GetComponent<GameInfo>().OriginalColor = randomColor;
        }

        Mesh otherMesh = GenerateNodeMesh(remainingAngle, arcDegrees, out arcDegrees, remainingHours * 100.0f / playtimeSum);

        GameObject newObject = Instantiate(NodePrefab, transform);
        newObject.GetComponent<MeshFilter>().mesh = otherMesh;
        newObject.GetComponent<MeshCollider>().sharedMesh = newObject.GetComponent<MeshFilter>().mesh;

        newObject.GetComponent<MeshRenderer>().material.color = Color.white;
        newObject.GetComponent<GameInfo>().OriginalColor = Color.white;
    }

    Mesh GenerateNodeMesh(float angle, float arcDegrees, out float outArcDegrees, float time)
    {
        Mesh mesh = new Mesh();
        List<Vector3> lowerArc = new List<Vector3>();
        List<Vector3> upperArc = new List<Vector3>();

        Vector3 center = transform.position;

        int count = (int)(angle * 2.0f);

        for (int i = 0; i < count; i++)
        {
            float innerX = Mathf.Cos(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * RadiusInner + center.x;
            float innerY = center.y;
            float innerZ = Mathf.Sin(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * RadiusInner + center.z;

            float outerX = Mathf.Cos(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * RadiusOuter + center.x;
            float outerY = center.y;
            float outerZ = Mathf.Sin(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * RadiusOuter + center.z;

            lowerArc.Add(new Vector3(innerX, innerY, innerZ));
            upperArc.Add(new Vector3(outerX, outerY, outerZ));
        }

        float newArcDegrees = arcDegrees * Mathf.Rad2Deg;
        newArcDegrees += (360.0f * time / 100.0f);
        outArcDegrees = newArcDegrees * Mathf.Deg2Rad;

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < lowerArc.Count - 1; ++i)
        {
            vertices.Add(upperArc[i]);
            vertices.Add(lowerArc[i]);
            vertices.Add(lowerArc[i + 1]);

            vertices.Add(upperArc[i]);
            vertices.Add(lowerArc[i + 1]);
            vertices.Add(upperArc[i + 1]);
        }

        for (int i = 0; i < vertices.Count - 2; ++i)
        {
            indices.Add(i);
            indices.Add(i + 1);
            indices.Add(i + 2);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        return mesh;
    }
}
