using UnityEngine;

public enum LineCaps { Sharp, Round, Bevel }

public class LineShape : AbstractShape {
  public LineCaps Caps { get; protected set; }
  public bool Loop { get; protected set; }

  protected Vector3[] _anchors = { };
  protected int _capresolution = 5;
  
  public void SetPoints(Vector3[] points) {
    _anchors = points;
  }

  public void SetStyle(float width, Color color, LineCaps caps, bool loop) {
    Width = width;
    Color = color;
    Caps = caps;
    Loop = loop;
  }

  protected override void GenerateMesh() {
    for(int i = 0; i < _anchors.Length; ++i) _anchors[i].y = 0;
    if(_anchors.Length < 2) return;

    int iterations = _anchors.Length + (Loop ? 1 : 0);
    for(int i = 0; i < iterations; ++i) {
      Vector3 t1 = Vector3.up, t2 = Vector3.up;
      Vector3 anchor = GetAnchor(i);

      if(i > 0 || Loop) {
        t1 = (GetAnchor(i - 1) - anchor).normalized;
      }
      if(i < _anchors.Length - 1 || Loop) {
        t2 = (GetAnchor(i + 1) - anchor).normalized;
      }

      if(i == 0 && !Loop) {
        MakeCap(anchor, -t2);
      } else if(i == _anchors.Length - 1 && !Loop) {
        MakeCap(anchor, t1);
      } else {
        MakeCorner(anchor, t1, t2);
      }
    }

    for(int i = 0; i < Vertices.Count; ++i) {
      Normals.Add(Vector3.up);
      Uvs.Add(new Vector2(0.5f, 0.5f));
    }
  }

  protected void MakeCorner(Vector3 anchor, Vector3 t1, Vector3 t2) {
    int prevl = Vertices.Count - 2, prevr = Vertices.Count - 1;
    Vector3 n1 = Vector3.Cross(Vector3.up, t1).normalized;
    Vector3 n2 = Vector3.Cross(Vector3.up, t2).normalized;
    float angle = 180 - Vector3.Angle(n1, n2);
    float step = angle / _capresolution;

    int anchorIdx = AddVertex(anchor);

    // straight bit
    Vector3 miter = (t1 != t2) ? (t1 + t2).normalized : -n2;
    float length = 0.5f * Width / Vector3.Dot(miter, n2);
    bool right = Vector3.Cross(miter, t1).y > 0;
    int lIdx = 0, rIdx = 0;
    if(right) {
      lIdx = AddVertex(anchor + 0.5f * Width * n1);
      rIdx = AddVertex(anchor + length * miter);
    } else {
      lIdx = AddVertex(anchor - length * miter);
      rIdx = AddVertex(anchor - 0.5f * Width * n1);
    }

    if(prevl >= 0 && prevr >= 0) {
      AddTri(prevl, lIdx, prevr);
      AddTri(prevr, lIdx, rIdx);
    }

    // start of curve
    AddTri(lIdx, anchorIdx, rIdx);

    if(Caps == LineCaps.Round) {
      if(right) {
        Vector3 rad = Vertices[lIdx] - anchor;
        for(int i = 1; i <= _capresolution; ++i) {
          Vector3 v = Quaternion.AngleAxis(i * step, Vector3.up) * rad;
          int idx = AddVertex(anchor + v);
          AddTri(lIdx, idx, anchorIdx);
          lIdx = idx;
        }
      } else {
        Vector3 rad = Vertices[rIdx] - anchor;
        for(int i = 1; i <= _capresolution; ++i) {
          Vector3 v = Quaternion.AngleAxis(i * -step, Vector3.up) * rad;
          int idx = AddVertex(anchor + v);
          AddTri(idx, rIdx, anchorIdx);
          rIdx = idx;
        }
        lIdx = AddVertex(anchor - length * miter);
        rIdx = AddVertex(Vertices[rIdx]);
      }
    } else if(Caps == LineCaps.Sharp) {
      if(right) {
        int cornerIdx = AddVertex(anchor - length * miter);
        AddTri(lIdx, cornerIdx, anchorIdx);
        lIdx = AddVertex(anchor - 0.5f * Width * n2);
        AddTri(cornerIdx, lIdx, anchorIdx);
      } else {
        int cornerIdx = AddVertex(anchor + length * miter);
        AddTri(rIdx, anchorIdx, cornerIdx);
        rIdx = AddVertex(anchor + 0.5f * Width * n2);
        AddTri(rIdx, cornerIdx, anchorIdx);
      }
    } else if(Caps == LineCaps.Bevel) {
      if(right) {
        int idx = AddVertex(anchor - 0.5f * Width * n2);
        AddTri(lIdx, idx, anchorIdx);
        lIdx = idx;
      } else {
        int idx = AddVertex(anchor + 0.5f * Width * n2);
        AddTri(idx, rIdx, anchorIdx);
        rIdx = idx;
      }
    }

    if(right) {
      rIdx = AddVertex(anchor + length * miter);
    } else {
      lIdx = AddVertex(anchor - length * miter);
      rIdx = AddVertex(Vertices[rIdx]);
    }

    // end of curve
    AddTri(anchorIdx, lIdx, rIdx);
  }

  protected void MakeCap(Vector3 anchor, Vector3 tangent) {
    int prevl = Vertices.Count - 2, prevr = Vertices.Count - 1;
    bool endcap = Vertices.Count != 0;
    Vector3 normal = Vector3.Cross(Vector3.up, tangent).normalized;
    int lIdx = 0, rIdx = 0;

    if(endcap || Caps != LineCaps.Round) {
      lIdx = AddVertex(anchor + 0.5f * Width * normal);
      rIdx = AddVertex(anchor - 0.5f * Width * normal);
      if(endcap) {
        AddTri(prevl, lIdx, prevr);
        AddTri(lIdx, rIdx, prevr);
      }
    }

    if(Caps != LineCaps.Round) return;

    if(!endcap) {
      lIdx = rIdx = AddVertex(anchor + 0.5f * Width * tangent);
    }
    float step = 90.0f / _capresolution;
    MakeRoundedCap(step, anchor, lIdx, rIdx);
  }

  protected void MakeRoundedCap(float step, Vector3 anchor, int lIdx, int rIdx) {
    Vector3 lRad = Vertices[lIdx] - anchor;
    Vector3 rRad = Vertices[rIdx] - anchor;
    int anchorIdx = AddVertex(anchor);
    for(int i = 0; i <= _capresolution; ++i) {
      int l = AddVertex(anchor + Quaternion.AngleAxis(i * step, Vector3.up) * lRad);
      int r = AddVertex(anchor + Quaternion.AngleAxis(i * step, Vector3.down) * rRad);
      AddTri(anchorIdx, lIdx, l);
      AddTri(anchorIdx, r, rIdx);
      lIdx = l;
      rIdx = r;
    }
  }

  protected Vector3 GetAnchor(int index) {
    if(index < 0) index += _anchors.Length;
    if(index >= _anchors.Length) index -= _anchors.Length;
    return _anchors[index];
  }

#if UNITY_EDITOR
  protected override void DrawGizmos() {
    if(_anchors.Length < 1) return;
    DrawLabel("<" + 0 + ">", transform.TransformPoint(_anchors[0]));

    for(int i = 1; i < _anchors.Length; i++) {
      Vector3 p1 = transform.TransformPoint(_anchors[i - 1]);
      Vector3 p2 = transform.TransformPoint(_anchors[i]);
      Gizmos.color = Color.magenta;
      Gizmos.DrawLine(p1, p2);
      DrawLabel("<" + i + ">", p2);
    }
  }
#endif
}
