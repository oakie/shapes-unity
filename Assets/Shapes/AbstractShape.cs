using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class AbstractShape : MonoBehaviour {
  public Color Color { get; protected set; }
  public float Width { get; protected set; }

  protected Mesh Mesh;
  protected readonly List<int> Tris = new List<int>();
  protected readonly List<Vector3> Vertices = new List<Vector3>();
  protected readonly List<Vector3> Normals = new List<Vector3>();
  protected readonly List<Vector2> Uvs = new List<Vector2>();

  protected abstract void GenerateMesh();

  protected void Start() {
    Refresh();
  }

  public void Refresh() {
    InitMesh();

    Vertices.Clear();
    Tris.Clear();
    Normals.Clear();
    Uvs.Clear();

    GenerateMesh();

    Mesh.Clear();
    Mesh.vertices = Vertices.ToArray();
    Mesh.triangles = Tris.ToArray();
    Mesh.normals = Normals.ToArray();
    Mesh.uv = Uvs.ToArray();
  }

  protected void InitMesh() {
    MeshRenderer mr = GetComponent<MeshRenderer>();
    if(!mr) mr = gameObject.AddComponent<MeshRenderer>();
    if(!mr.sharedMaterial) {
      mr.sharedMaterial = new Material(Shader.Find("Standard"));
      mr.sharedMaterial.SetFloat("_Glossiness", 0);

    }
    if(mr.sharedMaterial.color != Color) mr.sharedMaterial.color = Color;

    MeshFilter mf = GetComponent<MeshFilter>();
    if(!mf) mf = gameObject.AddComponent<MeshFilter>();

    Mesh = mf.sharedMesh;
    if(!Mesh) Mesh = mf.sharedMesh = new Mesh();
  }

  protected int AddVertex(Vector3 v) {
    Vertices.Add(v);
    return Vertices.Count - 1;
  }

  protected void AddVertices(Vector3[] v) {
    foreach(Vector3 p in v) Vertices.Add(p);
  }

  protected void AddTri(int v1, int v2, int v3) {
    Tris.Add(v1);
    Tris.Add(v2);
    Tris.Add(v3);
  }

  protected Vector3[] TransformPointArray(Vector3[] a) {
    Vector3[] t = new Vector3[a.Length];
    for(int i = 0; i < a.Length; ++i) { t[i] = transform.TransformPoint(a[i]); }
    return t;
  }

#if UNITY_EDITOR
  protected abstract void DrawGizmos();

  protected void OnDrawGizmos() {
    DrawGizmos();
  }

  protected void DrawLines(Vector3[] points, bool loop = false) {
    Vector3[] t = TransformPointArray(points);
    for(int i = 1; i < t.Length; i++) Gizmos.DrawLine(t[i-1], t[i]);
    if(loop) Gizmos.DrawLine(t[t.Length - 1], t[0]);
  }

  protected void DrawLabel(string text, Vector3 position) {
    GUIContent textContent = new GUIContent(text);
    GUIStyle style = new GUIStyle();
    style.normal.textColor = Color.white;
    style.fontSize = 16;
    Vector2 textSize = style.CalcSize(textContent);
    Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

    if(!(screenPoint.z > 0)) return;
    Vector3 worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f, screenPoint.z));
    Handles.Label(worldPosition, textContent, style);
  }
#endif
}
