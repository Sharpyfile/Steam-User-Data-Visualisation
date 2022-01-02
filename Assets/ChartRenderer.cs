using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChartRenderer : MonoBehaviour
{
    // Start is called before the first frame update

    public DataManager Data;
    public float RadiusInner;
    public float RadiusOuter;



    private void Start()
    {
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

        // Get all applications and assign them a degree
        float arcDegrees = 360.0f / Data.steamApplications.Count;
        arcDegrees = arcDegrees * Mathf.Deg2Rad;
        DrawSunburstChild(arcDegrees);

        yield break;
    }

    /// <summary>
    /// Draws sunburst chart
    /// </summary>
    /// <param name="arcDegrees"> Arc degree, needs to be in radians</param>
    void DrawSunburstChild(float arcDegrees)
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        List<Vector3> lowerArc = new List<Vector3>();
        List<Vector3> upperArc = new List<Vector3>();

        Vector3 center = transform.position;

        for (int i = 0; i < Data.steamApplications.Count; ++i)
        {
            float innerX = Mathf.Cos(arcDegrees * i) * RadiusInner + center.x;
            float innerY = center.y;
            float innerZ = Mathf.Sin(arcDegrees * i) * RadiusInner + center.z;

            float outerX = Mathf.Cos(arcDegrees * i) * RadiusOuter + center.x;
            float outerY = center.y;
            float outerZ = Mathf.Sin(arcDegrees * i) * RadiusOuter + center.z;

            lowerArc.Add(new Vector3(innerX, innerY, innerZ));
            upperArc.Add(new Vector3(outerX, outerY, outerZ));
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        // Define vertices
        for (int i = 0; i < lowerArc.Count - 1; ++i)
        {
            vertices.Add(upperArc[i]);      // 1
            vertices.Add(lowerArc[i]);      // 0            
            vertices.Add(lowerArc[i + 1]);  // 2

            vertices.Add(upperArc[i]);      // 1
            vertices.Add(lowerArc[i + 1]);  // 2
            vertices.Add(upperArc[i + 1]);  // 3
        }

        // Define indices
        for (int i = 0; i < vertices.Count - 2; ++i)
        {
            indices.Add(i); //
            indices.Add(i + 1); // 2
            indices.Add(i + 2); // 3
        }
        indices.Add(vertices.Count - 1);
        indices.Add(vertices.Count - 2);
        indices.Add(1);


        indices.Add(vertices.Count - 1);
        indices.Add(1);        
        indices.Add(0);


        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
    }
}
