using UnityEngine;

public class SpriteResolutionChanger : MonoBehaviour
{
    [Header("������� ���� ������� ������ ����������")]
    [SerializeField] Sprite[] sprites;

    [Header("�������")]
    [SerializeField] Vector2 cameraSize;
    [SerializeField] Vector2 spiteSize;
    [SerializeField] Vector2 targetResolution;

    SpriteRenderer spriteRenderer;
    Camera camera_;
    Vector2 lastCameraSize;
    float screenWidthToScreeHeight;

    private void Start()
    {
        screenWidthToScreeHeight = Screen.width / Screen.height;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteResolutionChanger �� ���� ����� SpriteRenderer �� ������� " + gameObject.name);
            enabled = false;
        }
    }

    void Update()
    {
        if (camera_ == null)
        {
            camera_ = DataOperator.instance.GetCamera();
        }
        else
        {
            float cameraOrthographicSize = DataOperator.instance.cameraSize;
            cameraSize = new Vector2(cameraOrthographicSize * 2 * screenWidthToScreeHeight, cameraOrthographicSize * 2);

            if (Mathf.Abs(1 - (cameraSize.x / lastCameraSize.x)) > 0.01f)
            {
                spiteSize = new Vector2(spriteRenderer.sprite.textureRect.width, spriteRenderer.sprite.textureRect.height) * transform.localScale / spriteRenderer.sprite.pixelsPerUnit;
                targetResolution = spiteSize / cameraSize * new Vector2(Screen.width, Screen.height);
                lastCameraSize = cameraSize;

                Vector2 bestResolutionFounded = new Vector2();
                int bestSuitingSpriteNum = 0;
                for (int spriteNum = 0; spriteNum < sprites.Length; spriteNum++)
                {
                    if (bestResolutionFounded.x < targetResolution.x) //��� �� ����� ������� � �������� �� ������� ����������
                    {
                        if (sprites[spriteNum].textureRect.width > bestResolutionFounded.x)
                        {
                            bestResolutionFounded = new Vector2(sprites[spriteNum].textureRect.width, sprites[spriteNum].textureRect.height);
                            bestSuitingSpriteNum = spriteNum;
                        }
                    }
                    else //����� ������ � �������� ������� ����������
                    {
                        if (sprites[spriteNum].textureRect.width < bestResolutionFounded.x && sprites[spriteNum].textureRect.width > targetResolution.x)
                        {
                            bestResolutionFounded = new Vector2(sprites[spriteNum].textureRect.width, sprites[spriteNum].textureRect.height);
                            bestSuitingSpriteNum = spriteNum;
                        }
                    }
                }
                spriteRenderer.sprite = sprites[bestSuitingSpriteNum];
            }
        }
    }
}
