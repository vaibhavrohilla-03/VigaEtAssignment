using UnityEngine;
public class AdjustHeight : MonoBehaviour
{
    public CharacterController characterController;
    public float verticalOffset = 0.1f;

    private void Start()
    {
        if (characterController == null)
        {
            Debug.LogWarning("AdjustHeight: Controller not Assigned");
            return;
        }
    }
    private void LateUpdate()
    {
        Vector3 controllerCenter = characterController.center;
        Vector3 controllerTopPoint = controllerCenter + Vector3.up * (characterController.height / 2f);
        Vector3 controllerTopPointWorld = characterController.transform.TransformPoint(controllerTopPoint);

        float targetVisualsWorldY = controllerTopPointWorld.y - verticalOffset;
        Vector3 targetVisualsWorldPosition = new Vector3(transform.position.x, targetVisualsWorldY, transform.position.z); ;
        transform.position = targetVisualsWorldPosition;
    }
}