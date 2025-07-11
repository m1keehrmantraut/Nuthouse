using UnityEngine;
using UnityEngine.Events;

public class StairController : MonoBehaviour
{
    [SerializeField] private Collider2D upperStairCollider;
    [SerializeField] private Collider2D lowerStairCollider;
    
    public UnityEvent OnStairsNeedToChange;
    
    public void DisableUpperStair()
    {
        if (upperStairCollider != null)
            upperStairCollider.enabled = false;
    }
    
    public void DisableLowerStair()
    {
        if (lowerStairCollider != null)
            lowerStairCollider.enabled = false;
    }
    
    public void SetStairs(bool upperEnabled, bool lowerEnabled)
    {
        if (upperStairCollider != null)
            upperStairCollider.enabled = upperEnabled;
        
        if (lowerStairCollider != null)
            lowerStairCollider.enabled = lowerEnabled;
    }
}