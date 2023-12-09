using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    [SerializeField]
    List<Vector3> points = new List<Vector3>();

    public enum ControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }
    [SerializeField]
    List<ControlPointMode> modes = new List<ControlPointMode>();

    public bool loop;
    public bool Loop {
        get => loop;
        set {
            loop = value;
            if (value == true)
            {
                modes[^1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    [Header("Gizmos")]
    public bool displayCurvature;
    public float directionDisplayLength = .5f;


    public void Reset()
    {
        points = new List<Vector3> {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, 0f, 2f),
            new Vector3(0f, 0f, 3f)
        };
        modes = new List<ControlPointMode>() {
            ControlPointMode.Free,
            ControlPointMode.Free
        };
    }


    public int ControlPointCount => points.Count;
    /// <summary>
    /// Number of points without handles.
    /// </summary>
    public int NodeCount => GetNode(points.Count - 1) + 1;

    public Vector3 GetControlPoint(int index) => points[index];

    public void SetControlPoint(int index, Vector3 point, Space space = Space.World)
    {
        // Get point in local space if the point is not givent in local space.
        if (space != Space.Self)
        {
            point = transform.InverseTransformPoint(point);
        }

        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (loop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Count - 2] += delta;
                    points[points.Count - 1] = point;
                }
                else if (index == points.Count - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                    points[index - 1] += delta;
                if (index + 1 < points.Count)
                    points[index + 1] += delta;
            }
        }
        points[index] = point;
        EnforceMode(index);
    }

    public ControlPointMode GetControlPointMode(int index)
    {
        return modes[GetNode(index)];
    }

    public void SetControlPointMode(int index, ControlPointMode mode)
    {
        int modeIndex = GetNode(index);
        modes[modeIndex] = mode;
        if (loop)
        {
            if (modeIndex == 0)
                modes[modes.Count - 1] = mode;
            else if (modeIndex == modes.Count - 1)
                modes[0] = mode;
        }
        EnforceMode(index);
    }

    void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        ControlPointMode mode = modes[modeIndex];
        if (mode == ControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Count - 1))
            return;

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
                fixedIndex = points.Count - 2;
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Count)
                enforcedIndex = 1;
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Count)
                fixedIndex = 1;
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
                enforcedIndex = points.Count - 2;
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == ControlPointMode.Aligned)
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public int CurveCount => (points.Count - 1) / 3;

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public (Vector3 position, Quaternion rotation) GetPositionAndRotation(float t)
    {
        Vector3 position = GetPoint(t);
        Quaternion rotation = Quaternion.LookRotation(GetVelocity(t));
        return (position, rotation);
    }


    public void AddCurve()
    {
        Vector3 point = points[points.Count - 1];
        point.x += 1f;
        points.Add(point);
        point.x += 1f;
        points.Add(point);
        point.x += 1f;
        points.Add(point);

        modes.Add(modes[modes.Count - 2]);
        EnforceMode(points.Count - 4);

        if (loop)
        {
            points[points.Count - 1] = points[0];
            modes[modes.Count - 1] = modes[0];
            EnforceMode(0);
        }
    }

    public void DeleteNode(int index)
    {
        // Stop if the index is not a node.
        if (!GetPointIsNode(index * 3))
        {
            print("Index is not a node");
            return;
        }

        int[] indices = new int[] { index * 3 - 1, index * 3, index * 3 + 1 };
        for (int i = indices.Length - 1; i >= 0; i--)
        {
            if (indices[i] >= 0 && indices[i] <= points.Count - 1)
            {
                points.RemoveAt(indices[i]);
            }
        }

        modes.RemoveAt(index);
    }

    public bool GetPointIsNode(int index) => GetNode(index) * 3 == index;

    public int GetNode(int index) => (index + 1) / 3;
}