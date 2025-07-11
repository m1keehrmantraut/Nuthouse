using UnityEngine;

public class UIFollowNPC : MonoBehaviour
{
    public Transform npcTransform; 
    public RectTransform uiElement; 
    public Camera uiCamera; 

    public Vector3 offset = new Vector3(0, 2f, 0); 

    void Update()
    {
        Vector3 worldPosition = npcTransform.position + offset;
        Vector3 screenPoint = uiCamera.WorldToScreenPoint(worldPosition);
        
        if (screenPoint.z > 0)
        {
            uiElement.gameObject.SetActive(true);
            uiElement.position = screenPoint;
        }
        else
        {
            uiElement.gameObject.SetActive(false);
        }
    }
}