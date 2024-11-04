using UnityEngine;

public class ModelUpdatable : MonoBehaviour
{
    private TransformJson transformToUpdate;

    void Start()
    {
        Debug.Log($"START {name} Local position: {transform.localPosition.ToString()}\nLocal rotation: {transform.localRotation.ToString()}\nLocal scale: {transform.localScale.ToString()}");
    }

    void Update()
    {
        UpdateBySchedule();
    }

    public void ScheduleForUpdate(TransformJson t)
    {
        Debug.Log($"Scheduling for update '${gameObject.name}, transform ${Serializer.Serialize(t)}'");
        transformToUpdate = t;
    }

    private void UpdateBySchedule()
    {
        if (transformToUpdate != null)
        {
            Debug.Log($"[ModelUpdatable] GameObject {name} updates position");
            transformToUpdate.ApplyToGameObject(gameObject);
            transformToUpdate = null;
            Debug.Log($"UPDATED {name} Local position: {transform.localPosition.ToString()}\nLocal rotation: {transform.localRotation.ToString()}\nLocal scale: {transform.localScale.ToString()}");

        }
    }
}