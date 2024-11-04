using System;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

struct TransformData
{
    public string checksum;
    public string name;
    public TransformJson transform;
    public double timestamp;
}

public class ModelPublishable : MonoBehaviour
{
    static readonly float POSITION_THRESHOLD = 0.001f;
    static readonly float ROTATION_THRESHOLD = 0.5f;
    static readonly float SCALE_THRESHOLD = .001f;

    private Quaternion lastRotation;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private string checksum;
    private string objName;
    private readonly Throttler throttler = new(50);

    void Start()
    {
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
        lastScale = transform.localScale;
        if (GameObjectHelpers.GetChildren(gameObject).Count > 0)
        {
            checksum = gameObject.name;
            objName = null;
        }
        else
        {
            checksum = transform.parent.gameObject.name;
            objName = gameObject.name;
        }
    }

    void Update()
    {
        PublishChanges();
    }

    private void PublishChanges()
    {
        if (lastRotation == null || lastPosition == null) return;

        var positionDiff = (lastPosition - transform.localPosition).Abs();
        var rotationDiff = (lastRotation.eulerAngles - transform.localEulerAngles).Abs();
        var scaleDiff = (lastScale - transform.localScale).Abs();

        if (positionDiff.MaxComponent() > POSITION_THRESHOLD ||
            rotationDiff.MaxComponent() > ROTATION_THRESHOLD ||
            scaleDiff.MaxComponent() > SCALE_THRESHOLD)
        {
            lastPosition = transform.localPosition;
            lastRotation = transform.localRotation;
            lastScale = transform.localScale;
            throttler.Exec(() =>
            {
                _ = MultiplayerManager.PublishTransform(checksum, objName, transform);
            });
        }
    }


}