using UnityEngine;

public class DraggingModule : MonoBehaviour
{
    public string moduleDataName;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.touchCount > 1)
        {
            Destroy(gameObject);
        }
    }
}
