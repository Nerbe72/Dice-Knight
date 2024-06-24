using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathData
{
    public (int c, int r, int b) num;
    public bool targeted;
    public List<(int x, int y)> path;

    public PathData((int c, int r, int b) _num, bool _targeted = false)
    {
        num = _num;
        targeted = _targeted;
        path = new List<(int x, int y)>();
    }

    public (int x, int y) LatestPath()
    {
        return path[path.Count - 1];
    }
}
