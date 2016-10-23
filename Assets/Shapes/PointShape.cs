using UnityEngine;

public enum PointSymbol {
  Square,
  Circle,
  Triangle
}

public class PointShape : AbstractShape {
  public PointSymbol Symbol { get; protected set; }

  private int _cornersInCircle = 20;

  public void SetStyle(float width, Color color, PointSymbol symbol) {
    Width = width;
    Color = color;
    Symbol = symbol;
  }

  protected override void GenerateMesh() {
    if(Symbol == PointSymbol.Square) {
      AddVertices(GenerateSquare());
      AddTri(0, 1, 2);
      AddTri(0, 2, 3);
    } else if(Symbol == PointSymbol.Triangle || Symbol == PointSymbol.Circle) {
      int corners = Symbol == PointSymbol.Triangle ? 3 : _cornersInCircle;
      AddVertices(GeneratePolygon(corners));
      for(int i = 1; i < corners; ++i) {
        AddTri(0, i - 1, i);
      }
    }

    for(int i = 0; i < Vertices.Count; ++i) {
      Normals.Add(Vector3.up);
      Uvs.Add(new Vector2(0.5f, 0.5f));
    }
  }

  protected Vector3[] GenerateSquare() {
    Vector3[] points = new Vector3[4];
    float radius = 0.5f * Width;
    points[0] = new Vector3(-radius, 0, -radius);
    points[1] = new Vector3(-radius, 0, radius);
    points[2] = new Vector3(radius, 0, radius);
    points[3] = new Vector3(radius, 0, -radius);
    return points;
  }

  protected Vector3[] GeneratePolygon(int corners) {
    Vector3[] points = new Vector3[corners];
    float radius = 0.5f * Width;
    for(int i = 0; i < corners; ++i) points[i] = Quaternion.AngleAxis(i * 360.0f / corners, Vector3.up) * (radius * Vector3.forward);
    return points;
  }

#if UNITY_EDITOR
  protected override void DrawGizmos() {
    Gizmos.color = Color.magenta;
    if(Symbol == PointSymbol.Square) {
      DrawLines(GenerateSquare(), true);
    } else if(Symbol == PointSymbol.Triangle) {
      DrawLines(GeneratePolygon(3), true);
    } else if(Symbol == PointSymbol.Circle) {
      DrawLines(GeneratePolygon(_cornersInCircle), true);
    }
  }
#endif
}