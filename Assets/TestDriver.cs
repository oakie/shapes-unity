using UnityEngine;

[ExecuteInEditMode]
public class TestDriver : MonoBehaviour {

  public LineShape Line;
  public LineCaps Caps = LineCaps.Sharp;
  private Vector3[] _points = { new Vector3(0, 0, 0), new Vector3(0, 0, 4), new Vector3(3, 0, 0), new Vector3(3, 0, 5) };

  public PointShape Point;
  public PointSymbol Symbol = PointSymbol.Circle;

  private void Update() {
    Line.SetStyle(1, Color.cyan, Caps, true);
    Line.SetPoints(_points);
    Line.Refresh();

    Point.SetStyle(2, Color.blue, Symbol);
    Point.Refresh();
  }
}
