using UnityEngine;

public class SmoothTransformer : MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Увеличивает частоту кадров в 2 раза по сравнению с FixedUpdate, интерполируя transform родительского объекта")]
    public bool doubleFrameRate = true;

    [Header("Отладка")]
    [SerializeField] AlphaTransform olderTransform;
    [SerializeField] AlphaTransform newerTransform;

    private Transform spriteGO;
    private Transform sourceGO;
    bool initialized = false;

    private void Start()
    {
        spriteGO = transform;
        Transform parent = transform.parent;
        if (parent != null)
        {
            sourceGO = parent;
        }
        else
        {
            Debug.LogError($"У объекта {gameObject} отсутствует родитель и прикреплён скрипт SmoothTransformer, который должен находиться на объекте с текстурой, и плавно передвигать его относительно родителя");
            enabled = false;
        }
        transform.parent = null;
    }

    private void FixedUpdate()
    {
        transform.position = sourceGO.position;
        transform.rotation = sourceGO.rotation;
        
        if (doubleFrameRate)
        {
            newerTransform = new AlphaTransform(sourceGO);
            if (initialized)
            {
                Invoke(nameof(SetPosBtw2Tranforms), Time.deltaTime / 2);
            }
            initialized = true;
        }
        else
        {
            initialized = false;
        }
    }

    void SetPosBtw2Tranforms()
    {
        AlphaTransform finalPos = new AlphaTransform();

        finalPos.position = (olderTransform.position * 0.5f) + (newerTransform.position * 0.5f);
        //finalPos.position.x = (olderTransform.position.x * olderRatio) + (newerTransform.position.x * newerRatio);
        //finalPos.position.y = (olderTransform.position.y * olderRatio) + (newerTransform.position.y * newerRatio);
        float xRot = olderTransform.rotation.eulerAngles.x + (Mathf.DeltaAngle(olderTransform.rotation.eulerAngles.x, newerTransform.rotation.eulerAngles.x) * 0.5f);
        float yRot = olderTransform.rotation.eulerAngles.y + (Mathf.DeltaAngle(olderTransform.rotation.eulerAngles.y, newerTransform.rotation.eulerAngles.y) * 0.5f);
        float zRot = olderTransform.rotation.eulerAngles.z + (Mathf.DeltaAngle(olderTransform.rotation.eulerAngles.z, newerTransform.rotation.eulerAngles.z) * 0.5f);
        finalPos.rotation.eulerAngles = new Vector3(xRot, yRot, zRot);

        finalPos.SetTransformAtThis(transform);
        
        olderTransform = new AlphaTransform(sourceGO);
    }
}


