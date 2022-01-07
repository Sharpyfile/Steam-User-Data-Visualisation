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


        DrawSunburst(0, 999999);

        yield break;
    }

    /// <summary>
    /// Draws sunburst chart
    /// </summary>
    /// <param name="arcDegrees"> Arc degree, needs to be in radians</param>
    public void DrawSunburst(float minPlaytime, float maxPlaytime)
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        float arcDegrees = 0.0f;
        arcDegrees = arcDegrees * Mathf.Deg2Rad;

        int playtimeSum = 0;

        Data.Games.games = Data.Games.games.OrderByDescending(item => item.playtime_forever).ToList();

        foreach (Game game in Data.Games.games)
        {
            var app = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            if (app == null)
                continue;

            if (game.playtime_forever < minPlaytime || game.playtime_forever > maxPlaytime)
                continue;

            playtimeSum += game.playtime_forever;           
        }        

        foreach (Game game in Data.Games.games)
        {
            var app = Data.steamApplications.Find(item => item.steam_appid == game.appid);
            if (app == null)
                continue;

            if (game.playtime_forever < minPlaytime || game.playtime_forever > maxPlaytime)
                continue;

            Mesh mesh = new Mesh();
            List<Vector3> lowerArc = new List<Vector3>();
            List<Vector3> upperArc = new List<Vector3>();

            Vector3 center = transform.position;

            float innerX = Mathf.Cos(arcDegrees) * RadiusInner + center.x;
            float innerY = center.y;
            float innerZ = Mathf.Sin(arcDegrees) * RadiusInner + center.z;

            float outerX = Mathf.Cos(arcDegrees) * RadiusOuter + center.x;
            float outerY = center.y;
            float outerZ = Mathf.Sin(arcDegrees) * RadiusOuter + center.z;

            lowerArc.Add(new Vector3(innerX, innerY, innerZ));
            upperArc.Add(new Vector3(outerX, outerY, outerZ));

            float newArcDegrees = arcDegrees * Mathf.Rad2Deg;
            float time = game.playtime_forever * 100.0f / playtimeSum;
            
            newArcDegrees += (360.0f * time / 100.0f);
            Debug.Log(newArcDegrees);
            arcDegrees = newArcDegrees * Mathf.Deg2Rad;

            innerX = Mathf.Cos(arcDegrees) * RadiusInner + center.x;
            innerY = center.y;
            innerZ = Mathf.Sin(arcDegrees) * RadiusInner + center.z;

            outerX = Mathf.Cos(arcDegrees) * RadiusOuter + center.x;
            outerY = center.y;
            outerZ = Mathf.Sin(arcDegrees) * RadiusOuter + center.z;

            lowerArc.Add(new Vector3(innerX, innerY, innerZ));
            upperArc.Add(new Vector3(outerX, outerY, outerZ));

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            vertices.Add(lowerArc[0]);
            vertices.Add(upperArc[1]);      
            vertices.Add(upperArc[0]);
            vertices.Add(lowerArc[1]);

            indices.Add(0);
            indices.Add(1);
            indices.Add(2);

            indices.Add(0);
            indices.Add(3);
            indices.Add(1);

            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();

            GameObject newObject = Instantiate(NodePrefab, transform);
            newObject.GetComponent<MeshFilter>().mesh = mesh;
            newObject.GetComponent<MeshCollider>().sharedMesh = newObject.GetComponent<MeshFilter>().mesh;
            newObject.GetComponent<GameInfo>().OwnedGame = game;
            newObject.GetComponent<GameInfo>().GameApplication = Data.steamApplications.Find(item => item.steam_appid == game.appid);
        }        
    }
}
