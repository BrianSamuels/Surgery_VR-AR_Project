using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spline))]
public class SplineInspector : Editor
{
    int StepsPerCurve => ((Spline)target).displayCurvature ? HighStepsPerCurve : LowStepsPerCurve;
    const int LowStepsPerCurve = 10;
    const int HighStepsPerCurve = 300;
    float DirectionScale => ((Spline)target).directionDisplayLength;
    const float HandleSize = .04f;
    const float PickSize = .06f;

    static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    Spline spline;
    Transform handleTransform;
    Quaternion handleRotation;
    int selectedIndex = -1;


    void OnSceneGUI()
    {
        spline = target as Spline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }

        ShowDirections();
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            DrawSelectedPointInspector();
        }

        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }

    void DrawSelectedPointInspector()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Selected Point", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }

        EditorGUI.BeginChangeCheck();
        Spline.ControlPointMode mode = (Spline.ControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }

        if (spline.GetPointIsNode(selectedIndex) && GUILayout.Button("Delete point"))
        {
            spline.DeleteNode(spline.GetNode(selectedIndex));
            EditorUtility.SetDirty(spline);
        }
    }

    void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = spline.GetPoint(0f);
        Handles.DrawLine(point, point + spline.GetDirection(0f) * DirectionScale);
        int steps = StepsPerCurve * spline.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * DirectionScale);
        }
    }

    Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
            size *= 2f;
        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, size * HandleSize, size * PickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point), Space.Self);
            }
        }
        return point;
    }
}