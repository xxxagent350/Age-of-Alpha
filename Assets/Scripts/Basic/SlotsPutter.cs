using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotsPutter : MonoBehaviour
{

    [SerializeField] Image mainSlotButton;
    [SerializeField] GameObject mainSlotPrefab;
    [SerializeField] Image universalSlotButton;
    [SerializeField] GameObject universalSlotPrefab;
    [SerializeField] Image engineSlotButton;
    [SerializeField] GameObject engineSlotPrefab;
    [SerializeField] AudioSource buttonSoundSource;
    int choosenSlotType = -1;

    [SerializeField] AudioClip[] slotPutSounds;
    [SerializeField] AudioSource slotPutSoundSource;
    Camera camera_;
    bool mousePressed;
    float timerPressed;
    const float maxTimerToIncludePress = 0.2f;
    Vector2 startPressPoint;
    Vector2 pressPoint;
    const float maxDistanceToIncludePress = 10;

    private void Start()
    {
        camera_ = GetComponent<Camera>();
    }

    public void ChooseMainSlotType()
    {
        if (choosenSlotType != 0)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            choosenSlotType = 0;
            buttonSoundSource.Play();
        }
    }
    public void ChooseUniversalSlotType()
    {
        if (choosenSlotType != 1)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            choosenSlotType = 1;
            buttonSoundSource.Play();
        }
    }
    public void ChooseEngineSlotType()
    {
        if (choosenSlotType != 2)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            choosenSlotType = 2;
            buttonSoundSource.Play();
        }
    }

    private void Update()
    {
        GetInputs();
        ButtonsUI();
    }

    void ButtonsUI()
    {
        

            
    }

    void GetInputs()
    {
        timerPressed += Time.deltaTime;
        if (Input.GetMouseButtonDown(0) == true)
        {
            timerPressed = 0;
            mousePressed = true;
            startPressPoint = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0) == true)
        {
            mousePressed = false;
            CheckClick();
        }
        if (mousePressed)
        {
            pressPoint = Input.mousePosition;
        }
    }

    void CheckClick()
    {
        if (timerPressed < maxTimerToIncludePress && Vector2.Distance(startPressPoint, pressPoint) < maxDistanceToIncludePress)
        {
            Click(pressPoint);
        }
    }

    void Click(Vector2 point)
    {
        if (GameObject.FindGameObjectWithTag("Ship") != null && choosenSlotType != -1)
        {
            float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
            Vector2 pointInUnits = point / pixelsPerUnit;
            pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
            pointInUnits += new Vector2(transform.position.x, transform.position.y);
            
            if (!GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().cellsShift)
            {
                pointInUnits -= new Vector2(0.5f, 0.5f);
            }
            int slotPutSoundNum = Random.Range(0, slotPutSounds.Length);
            slotPutSoundSource.clip = slotPutSounds[slotPutSoundNum];
            slotPutSoundSource.Play();

            Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(pointInUnits.x), Mathf.RoundToInt(pointInUnits.y));
            if (choosenSlotType == 0)
            {
                Instantiate(mainSlotPrefab, roundedPointInUnits, Quaternion.identity);
            }
            if (choosenSlotType == 1)
            {
                Instantiate(universalSlotPrefab, roundedPointInUnits, Quaternion.identity);
            }
            if (choosenSlotType == 2)
            {
                Instantiate(engineSlotPrefab, roundedPointInUnits, Quaternion.identity);
            }

            //Debug.Log("X: " + Mathf.RoundToInt(pointInUnits.x) + " Y: " + Mathf.RoundToInt(pointInUnits.y));
        }
    }

}
