using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static DataManager;

public class ChartRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    public static ChartRenderer Instance;

    public GameObject WaitingCube;

    public DataManager Data;
    public float RadiusInner;
    public float RadiusOuter;

    public int MinimumPlaytime = 300;
    public int MaximumPlaytime = 300;

    public GameObject NodePrefab;
    public GameObject GenreNodePrefab;
    public GameObject FriendNodePrefab;
    public GameObject SpheresContainer;

    [Header("Height difference between nodes")]
    public float HeightDifference;

    public List<string> FilterGenres = new List<string>();
    public List<Friend> FilterFriends = new List<Friend>();

    public List<SteamApplication> CurrentGames = new List<SteamApplication>();

    private void Start()
    {
        Instance = this;

        _drawSunburstChart = DrawSunburstChart();
        StartCoroutine(_drawSunburstChart);

        _drawWaitingCube = DrawWaitingCube();
        StartCoroutine(_drawWaitingCube);
    }

    IEnumerator _drawWaitingCube;
    IEnumerator DrawWaitingCube()
    {
        for (; ; )
        {
            if (Data.IsDone)
                break;

            WaitingCube.transform.localScale = new Vector3(Mathf.Abs(Mathf.Sin(Time.time)) * 10.0f + 1.0f, 1, Mathf.Abs(Mathf.Cos(Time.time)) * 10.0f + 1.0f);

            yield return new WaitForEndOfFrame();
        }

        Destroy(WaitingCube);
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
        MinimumPlaytime = (int)minPlaytime;
        MaximumPlaytime = (int)maxPlaytime;

        float arcDegrees = 0.0f;
        arcDegrees = arcDegrees * Mathf.Deg2Rad;

        Data.OwnedGames = Data.OwnedGames.OrderByDescending(item => item.playtime_forever).ToList();

        int playtimeSum = 0;
        CurrentGames.Clear();

        foreach (Game game in Data.OwnedGames)
        {
            if (!CheckIfShowGame(game))
                continue;

            playtimeSum += game.playtime_forever;           
        }

        float hours = playtimeSum / 60.0f;
        Modal.Instance.SelectedPlaytime.text = $"Selected playtime: {(float)Mathf.Round(hours * 100.0f) / 100f} h";

        float remainingAngle = 0.0f;
        float remainingHours = 0.0f;

        int count = 1;

        foreach(Game game in Data.OwnedGames)
        {
            if (!CheckIfShowGame(game))
                continue;

            float time = game.playtime_forever * 100.0f / playtimeSum;
            float angle = 360.0f * time / 100.0f;

            if (angle < 1.0f)
            {
                continue;
            }

            ++count;
        }

        if (transform.childCount == count)
            return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        foreach (Game game in Data.OwnedGames)
        {
            var app = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            if (!CheckIfShowGame(game))
                continue;

            float time = game.playtime_forever * 100.0f / playtimeSum;
            float angle = 360.0f * time / 100.0f;

            if (angle < 1.0f)
            {
                remainingAngle += angle;
                remainingHours += game.playtime_forever;
                continue;
            }

            CurrentGames.Add(app);

            float genreDegrees = arcDegrees;
            Vector3 point;
            Mesh mesh = GenerateNodeMesh(angle, arcDegrees, out arcDegrees, time, RadiusInner, RadiusOuter, out point);

            GameObject newObject2 = Instantiate(NodePrefab, transform);
            newObject2.GetComponent<MeshFilter>().mesh = mesh;
            newObject2.GetComponent<GameInfo>().OwnedGame = game;
            newObject2.GetComponent<GameInfo>().GameApplication = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            newObject2.GetComponent<MeshCollider>().sharedMesh = newObject2.GetComponent<MeshFilter>().mesh;
            newObject2.GetComponent<MeshRenderer>().enabled = false;
            newObject2.GetComponent<OnMouseOverNode>().Point = point;

            float innerRadius = RadiusInner;
            float radiusWidth = (RadiusOuter - RadiusInner) / (float)app.genres.Count;
            float temp = 0.0f;

            for (int i = 0; i < app.genres.Count; ++i)
            {
                Mesh genreMesh = GenerateNodeMesh(angle, genreDegrees, out temp, time, innerRadius, innerRadius + radiusWidth, out point);
                GameObject genreObject = Instantiate(GenreNodePrefab);
                genreObject.transform.SetParent(newObject2.transform);

                Color nodeColor = Color.white;
                foreach (Transform toggle in Modal.Instance.ToggleTransform)
                {
                    var genreToggle = toggle.GetComponent<GenreToggle>();
                    if (genreToggle != null && genreToggle.GenreName == app.genres[i])
                    {
                        nodeColor = genreToggle.HoverColor;
                        break;
                    }                        
                }

                genreObject.GetComponent<MeshFilter>().mesh = genreMesh;
                genreObject.GetComponent<MeshRenderer>().material.color = nodeColor;

                innerRadius += radiusWidth; 
            }

            innerRadius = RadiusOuter + 0.1f;
            radiusWidth = 0.1f;

            List<Friend> friendsOwningGame = new List<Friend>();

            foreach(Friend friend in DataManager.Instance.Friends)
            {
                if (friend.games.FirstOrDefault(item => item.appid == app.steam_appid) != null)
                {
                    friendsOwningGame.Add(friend);
                }
            }

            foreach(Friend friend in friendsOwningGame)
            {
                Mesh friendMesh = GenerateNodeMesh(angle, genreDegrees, out temp, time, innerRadius, innerRadius + radiusWidth, out point);
                GameObject genreObject = Instantiate(FriendNodePrefab);
                genreObject.transform.SetParent(newObject2.transform);

                Color nodeColor = Color.white;

                genreObject.GetComponent<MeshFilter>().mesh = friendMesh;
                genreObject.GetComponent<MeshRenderer>().material.color = nodeColor;
                genreObject.GetComponent<MeshCollider>().sharedMesh = genreObject.GetComponent<MeshFilter>().mesh;
                genreObject.GetComponent<OnMouseOverFriendNode>().SteamID = friend.steamid;

                innerRadius += radiusWidth;
                innerRadius += radiusWidth;
            }
        }

        Vector3 point2;
        Mesh otherMesh = GenerateNodeMesh(remainingAngle, arcDegrees, out arcDegrees, remainingHours * 100.0f / playtimeSum, RadiusInner, RadiusOuter, out point2);

        GameObject newObject = Instantiate(NodePrefab, transform);
        newObject.GetComponent<MeshFilter>().mesh = otherMesh;
        newObject.GetComponent<MeshCollider>().sharedMesh = newObject.GetComponent<MeshFilter>().mesh;

        newObject.GetComponent<MeshRenderer>().material.color = Color.white;
        newObject.GetComponent<GameInfo>().OriginalColor = Color.white;
    }

    Mesh GenerateNodeMesh(float angle, float arcDegrees, out float outArcDegrees, float time, float radiusInner, float radiusOuter, out Vector3 point)
    {
        Mesh mesh = new Mesh();
        List<Vector3> lowerArc = new List<Vector3>();
        List<Vector3> upperArc = new List<Vector3>();

        Vector3 center = transform.position;

        int count = (int)(angle * 2.0f);

        for (int i = 0; i < count; i++)
        {
            float innerX = Mathf.Cos(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * radiusInner + center.x;
            float innerY = center.y;
            float innerZ = Mathf.Sin(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * radiusInner + center.z;

            float outerX = Mathf.Cos(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * radiusOuter + center.x;
            float outerY = center.y;
            float outerZ = Mathf.Sin(arcDegrees + (0.5f * i) * Mathf.Deg2Rad) * radiusOuter + center.z;

            lowerArc.Add(new Vector3(innerX, innerY, innerZ));
            upperArc.Add(new Vector3(outerX, outerY, outerZ));
        }

        float newArcDegrees = arcDegrees * Mathf.Rad2Deg;
        newArcDegrees += (360.0f * time / 100.0f);
        outArcDegrees = newArcDegrees * Mathf.Deg2Rad;

        float midAngle = arcDegrees * Mathf.Rad2Deg + (360.0f * time / 200.0f);
        midAngle = midAngle * Mathf.Deg2Rad;
        point = new Vector3();
        float midPoint = Mathf.Lerp(radiusInner, radiusOuter, 0.5f);
        point.x = (Mathf.Cos(midAngle) * Mathf.Deg2Rad) * midPoint + center.x;
        point.y = center.y;
        point.z = (Mathf.Sin(midAngle) * Mathf.Deg2Rad) * midPoint + center.z;

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

    public bool CheckIfShowGame(Game game)
    {
        var app = DataManager.Instance.steamApplications.Find(item => item.steam_appid == game.appid);
        if (app == null)
            return false;

        if (game.playtime_forever <= MinimumPlaytime || game.playtime_forever > MaximumPlaytime)
            return false;

        foreach (string genre in FilterGenres)
        {
            if (!app.genres.Contains(genre))
            {
                return false;
            }
        }

        foreach (Friend friend in FilterFriends)
        {
            if (friend.games.FirstOrDefault(item => item.appid == app.steam_appid) == null)
            {
                return false;
            }
        }

        return true;
    }
}
