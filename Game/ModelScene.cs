
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ChildData
{
    public UnityEngine.Vector3 InitialPos;
    public Quaternion InitialRotation;
}

public class ModelScene : MonoBehaviour
{
    static readonly float BREAKDOWN_GAP = 0.2f;
    private readonly Dictionary<string, ChildData> childrenData = new();

    private readonly List<GameObject> children = new();
    public bool IsJoined = true;

    void Awake()
    {
        children.AddRange(GameObjectHelpers.GetChildren(gameObject));
        // AttachModelObjectScriptToChildren();
    }

    void Start()
    {
        AssignChildrenMaterials();
        SetChildrenHostTransform(true);
        PopulateChildrenData();
        // InstantiateLabels();
    }

    void Update()
    {
        UpdateControls();
    }

    private void ResetObjectPositioning()
    {
        gameObject.transform.SetLocalPositionAndRotation(UnityEngine.Vector3.zero, Quaternion.identity);
    }

    private void PopulateChildrenData()
    {
        foreach (GameObject child in children)
        {
            childrenData.Add(child.name, new ChildData
            {
                InitialPos = child.transform.localPosition,
                InitialRotation = child.transform.localRotation,
            });
        }
    }

    private void AttachModelObjectScriptToChildren()
    {
        foreach (GameObject child in children)
        {
            // child.AddComponent<ModelObject>();
        }
    }

    private void AssignChildrenMaterials()
    {
        foreach (GameObject child in children)
        {
            // var mr = child.GetComponent<MeshRenderer>();
            // Material[] materials = new Material[mr.materials.Length];
            // for (var i = 0; i < mr.materials.Length; i++)
            // {
            //   var color = mr.materials[i].color;
            //   var matFilename = color.a > 0.99f ? "MRTK_Standard_Blue" : "MRTK_Standard_TransparentBlue";
            //   var res = Resources.Load<Material>(matFilename);
            //   var mat = new Material(res)
            //   {
            //     color = color
            //   };
            //   materials[i] = mat;
            // }
            // mr.materials = materials;
        }
    }

    private void SetChildrenHostTransform(bool parent)
    {
        foreach (GameObject child in children)
        {
            // var man = child.GetComponent<ObjectManipulator>();
            // var host = parent ? gameObject : child;
            // man.HostTransform = host.transform;
        }
    }

    public void ToggleJoined()
    {
        if (children.Count < 2) return;

        if (IsJoined)
        {
            IsJoined = false;
            SetChildrenSpaced(BREAKDOWN_GAP);
            SetChildrenHostTransform(false);
        }
        else
        {
            IsJoined = true;
            ResetObjectPositioning();
            ResetChildrenPositioning();
            SetChildrenHostTransform(true);
        }
    }

    public void ToggleLocked()
    {
        SetChildrenManipulationsAllowed(IsLocked());
    }

    private void SetChildrenSpaced(float gap)
    {
        var ordered = children.OrderBy(child => child.transform.position.x).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var curPos = ordered[i].transform.position;
            var newPos = curPos + new UnityEngine.Vector3(gap * i - gap * (ordered.Count - 1) / 2.0f, 0, 0);
            ordered[i].transform.position = newPos;
        }
    }

    private void ResetChildrenPositioning()
    {
        foreach (var child in children)
        {
            var data = childrenData[child.name];
            child.transform.SetLocalPositionAndRotation(data.InitialPos, data.InitialRotation);
        }
    }

    private bool IsLocked()
    {
        return false;
        // return children[0].GetComponent<ObjectManipulator>().AllowedManipulations == TransformFlags.None;
    }

    private void SetChildrenManipulationsAllowed(bool allowed)
    {
        foreach (var child in children)
        {
            // var man = child.GetComponent<ObjectManipulator>();
            // man.AllowedManipulations = allowed ? TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale : TransformFlags.None;
        }
    }

    private void UpdateControls()
    {
        if (GameRoom.Room().LockOwnerID != MultiplayerManager.ApiClient.GetSession().SessionId)
        {
            return;
        }

        if (Input.GetKey(KeyCode.J))
        {
            ToggleJoined();
            if (GameManager.IsMultiplayerMode)
            {
                if (IsJoined)
                {
                    MultiplayerManager.JoinScene(gameObject.name);
                }
                else
                {
                    MultiplayerManager.UnjoinScene(gameObject.name);
                }
            }
        }

        var vector = UnityEngine.Vector3.zero;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            vector = UnityEngine.Vector3.right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            vector = UnityEngine.Vector3.left;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            vector = UnityEngine.Vector3.back;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            vector = UnityEngine.Vector3.forward;
        }
        gameObject.transform.Translate(3 * Time.deltaTime * vector);
    }
}
