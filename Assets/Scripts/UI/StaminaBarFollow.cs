using UnityEngine;

public class StaminaBarFollow : MonoBehaviour
{
    [SerializeField] private Transform target; 
    [SerializeField] private Vector3 offset = new Vector3(0, 2.0f, 0);
    [SerializeField] private Camera cam; 

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (cam == null)
            cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || cam == null) return;

        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        rectTransform.position = screenPos;
    }
}