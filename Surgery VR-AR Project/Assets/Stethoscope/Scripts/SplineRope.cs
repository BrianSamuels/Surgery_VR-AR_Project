using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SplineRope : MonoBehaviour
{
    public Spline spline;
    public Transform root;
    public RopeTransform[] transforms;
    public Vector3 transformsLocalForward = Vector3.forward;
    public Vector3 transformsLocalUp = Vector3.up;
    public Transform transformsUpReference;
    public bool stretch = true;

    [Header("Inspector")]
    public bool drawLines = true;
    public float lineLength = .3f;


    void Update()
    {
        // Update transforms if all elements are present.
        if (spline && transforms.Length > 0)
        {
            if (!stretch)
            {
                UpdateTransformsOnSpline();
            }
            else
            {
                UpdateStretch();
            }
        }
    }


    void UpdateStretch()
    {
        // Set first transform position and rotation.
        transforms[0].transform.position = spline.GetPoint(0);
        transforms[0].transform.forward = spline.GetDirection(0);

        // Get spline length.
        float splineLength = 0;
        {
            float lastT = 0;
            for (float t = 0; t <= 1; t += .001f)
            {
                splineLength += Vector3.Distance(spline.GetPoint(lastT), spline.GetPoint(t));
                lastT = t;
            }
        }

        // Get transforms length.
        float transformsLength = 0;
        for (int i = 1; i < transforms.Length; i++)
        {
            transformsLength += transforms[i].defaultLocalPosition.magnitude;
        }

        // Get stretch factor.
        float stretchFactor = splineLength / transformsLength;

        // Update transforms with stretch factor.
        UpdateTransformsOnSpline(stretchFactor);
    }

    void UpdateTransformsOnSpline(float stretchFactor = 1)
    {
        // Set first transform position and rotation.
        transforms[0].transform.position = spline.GetPoint(0);
        transforms[0].SetForward(
            spline.GetDirection(0),
            transformsLocalForward,
            transformsLocalUp,
            transformsUpReference != null ? transformsUpReference.up : Vector3.up);

        // Set following transforms position and rotation.
        float previousT = 0;
        for (int i = 1; i < transforms.Length; i++)
        {
            // Get target distance.
            float targetDistance = transforms[i].defaultLocalPosition.magnitude * stretchFactor;

            // Find t.
            float iterationT = previousT;
            float iterationDistance = 0;
            {
                // Increase iteration distance and t until the required distance is reached.
                while (iterationDistance < targetDistance && iterationT <= 1)
                {
                    iterationDistance = Vector3.Distance(transforms[i - 1].transform.position, spline.GetPoint(iterationT));
                    iterationT += .001f;
                };

                // Remember iteration t.
                previousT = iterationT;
            }

            // Get position and rotation from t.
            Vector3 position;
            Vector3 rotation;
            {
                // Get position from last point if is over the bounds of the spline, else from the spline.
                if (iterationT >= 1)
                {
                    position = transforms[i - 1].transform.position + stretchFactor * transforms[i].defaultLocalPosition.magnitude * transforms[i - 1].GetForward(transformsLocalForward);
                }
                else
                {
                    position = spline.GetPoint(iterationT);
                }

                // Get rotation from the spline.
                rotation = spline.GetVelocity(iterationT);

            }

            // Apply position and rotation.
            transforms[i].transform.position = position;
            transforms[i].SetForward(rotation, transformsLocalForward, transformsLocalUp, transforms[i - 1].transform.forward);
        }
    }


    #region Get transforms.
    public void GetTransforms(Transform root)
    {
        this.root = root;
        GetTransformsFromRoot();
    }

    [ContextMenu("Get Transforms From Root")]
    public void GetTransformsFromRoot()
    {
        // Get all first children along transform hierarchy.
        List<Transform> transforms = new List<Transform>();
        Transform target = root;
        while (target != null)
        {
            transforms.Add(target);
            target = target.childCount > 0 ? target.GetChild(0) : null;
        }

        // Set rope transforms from transforms.
        this.transforms = transforms.Select(transform => new RopeTransform(transform)).ToArray();
    }
    #endregion

    [System.Serializable]
    public struct RopeTransform
    {
        public Transform transform;
        public Vector3 defaultLocalPosition;

        public RopeTransform(Transform transform) : this(transform, transform.localPosition) { }
        public RopeTransform(Transform transform, Vector3 defaultLocalPosition)
        {
            this.transform = transform;
            this.defaultLocalPosition = defaultLocalPosition;
        }

        public Vector3 GetForward(Vector3 forwardAxis) => Helper.GetAxis(transform, forwardAxis);

        public void SetForward(Vector3 forward, Vector3 forwardAxis, Vector3 upAxis, Vector3 upReference)
        {
            // Set forward axis.
            Helper.SetAxis(transform, forwardAxis, forward);

            // Rotate towards up vector.
            {
                // Keep transform forward.
                var transformForward = Helper.GetAxis(transform, forwardAxis);

                // Get up reference in transform space.
                Vector3 upReferenceInTransformSpace;
                {
                    var transformPlane = new Plane(transformForward, Vector3.zero);
                    upReferenceInTransformSpace = transformPlane.ClosestPointOnPlane(upReference).normalized;
                }

                // Rotate from angle with up reference.
                transform.Rotate(transformForward, Vector3.SignedAngle(Helper.GetAxis(transform, upAxis), upReferenceInTransformSpace, transformForward), Space.World);
            }
        }
    }

    public class Helper
    {
        public static Vector3 GetAxis(Transform transform, Vector3 reference)
        {
            // Get abs references.
            float absX = Mathf.Abs(reference.x);
            float absY = Mathf.Abs(reference.y);
            float absZ = Mathf.Abs(reference.z);

            // Use X.
            if (absX > absY && absX > absZ)
            {
                return transform.right * reference.x;
            }

            // Use Y.
            if (absY > absX && absY > absZ)
            {
                return transform.up * reference.y;
            }

            // Use Z.
            return transform.forward * reference.z;

        }

        public static void SetAxis(Transform transform, Vector3 reference, Vector3 value)
        {
            // Get abs references.
            float absX = Mathf.Abs(reference.x);
            float absY = Mathf.Abs(reference.y);
            float absZ = Mathf.Abs(reference.z);

            // Use X.
            if (absX > absY && absX > absZ)
            {
                transform.right = value * reference.x;
                return;
            }

            // Use Y.
            if (absY > absX && absY > absZ)
            {
                transform.up = value * reference.y;
                return;
            }

            // Use Z.
            transform.forward = value * reference.z;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SplineRope))]
[CanEditMultipleObjects]
class SplineRopeEditor : Editor
{
    void OnSceneGUI()
    {
        // Get script.
        SplineRope script = (SplineRope)target;

        // Draw lines for each transform.
        if (script.drawLines)
        {
            var transforms = script.transforms;
            for (int i = 0; i < transforms.Length; i++)
            {
                // Draw line with previous point.
                if (i > 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(transforms[i - 1].transform.position, transforms[i].transform.position);
                }

                // Draw normal line.
                Handles.color = Color.green;
                Handles.DrawLine(
                    transforms[i].transform.position,
                    transforms[i].transform.position + SplineRope.Helper.GetAxis(transforms[i].transform, script.transformsLocalUp) * Mathf.Max(script.lineLength, 0));
            }
        }
    }
}
#endif
