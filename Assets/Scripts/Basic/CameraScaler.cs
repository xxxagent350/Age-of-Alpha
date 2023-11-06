using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{

    [Header("Настройки")]
    [SerializeField] float maxZoom;
    [SerializeField] float minZoom;
    float sensitivity;
    [SerializeField] float mouseScrollSensitivity = 0.1f;
    [SerializeField] Vector2 minPos;
    [SerializeField] Vector2 maxPos;

    Touch touch_1;
    Touch touch_2;
    float last_distance;
    bool ready_to_scale;

    bool ready_to_transform;
    Vector2 last_touch_point;

    int last_touch_count;
    int touch_count;
    bool mouse_pressed;

    private void Awake()
    {

        Application.targetFrameRate = 60;

    }

    private void Update()
    {

        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0) == true)
        {
            mouse_pressed = true;
        }

        if (Input.GetMouseButtonUp(0) == true)
        {
            mouse_pressed = false;
        }

        if (mouse_pressed == false)
        {
            touch_count = Input.touchCount;
        }
        else
        {
            touch_count = 1;
        }

        if (Input.touchCount == 2)
        {
            //зум
            touch_1 = Input.GetTouch(0);
            touch_2 = Input.GetTouch(1);

            if (Vector2.Distance(touch_1.position, touch_2.position) > 0)
            {
                if (ready_to_scale == false)
                {

                    ready_to_scale = true;
                    last_distance = Vector2.Distance(touch_1.position, touch_2.position);

                }
                else
                {

                    float to_scale = Vector2.Distance(touch_1.position, touch_2.position) / last_distance;
                    GetComponent<Camera>().orthographicSize /= to_scale;
                    //transform.localPosition = new Vector3(transform.localPosition.x * to_scale, transform.localPosition.y * to_scale, 0);
                    last_distance = Vector2.Distance(touch_1.position, touch_2.position);

                }
            }

        }
        else
        {
            ready_to_scale = false;
        }

        GetComponent<Camera>().orthographicSize /= 1 + (Input.mouseScrollDelta.y * mouseScrollSensitivity);

        if (GetComponent<Camera>().orthographicSize > maxZoom)
        {

            GetComponent<Camera>().orthographicSize = maxZoom;

        }

        if (GetComponent<Camera>().orthographicSize < minZoom)
        {

            GetComponent<Camera>().orthographicSize = minZoom;

        }



        if (((Input.touchCount != 0 && Input.touchCount <= 2) || mouse_pressed == true) && last_touch_count == touch_count)
        {
            //движение
            if (Input.touchCount != 0)
            {
                if (ready_to_transform == false)
                {

                    ready_to_transform = true;
                    Vector2 average_last_touch = new Vector2(0, 0);

                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        average_last_touch += Input.GetTouch(i).position;
                    }

                    average_last_touch = new Vector2(average_last_touch.x / Input.touchCount, average_last_touch.y / Input.touchCount);

                    last_touch_point = average_last_touch;

                }
                else
                {

                    Vector2 average_now_touch = new Vector2(0, 0);

                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        average_now_touch += Input.GetTouch(i).position;
                    }

                    average_now_touch = new Vector2(average_now_touch.x / Input.touchCount, average_now_touch.y / Input.touchCount);

                    sensitivity = (GetComponent<Camera>().orthographicSize * 2) / Screen.height;
                    Vector2 pos_plus = (average_now_touch - last_touch_point) * sensitivity;
                    transform.localPosition -= new Vector3(pos_plus.x, pos_plus.y, 0);


                    Vector2 average_last_touch = new Vector2(0, 0);

                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        average_last_touch += Input.GetTouch(i).position;
                    }

                    average_last_touch = new Vector2(average_last_touch.x / Input.touchCount, average_last_touch.y / Input.touchCount);

                    last_touch_point = average_last_touch;

                }
            }

            if (mouse_pressed == true)
            {
                if (ready_to_transform == false)
                {

                    ready_to_transform = true;
                    last_touch_point = Input.mousePosition;

                }
                else
                {

                    sensitivity = (GetComponent<Camera>().orthographicSize * 2) / Screen.height;
                    Vector2 pos_plus = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - last_touch_point) * sensitivity;
                    transform.localPosition -= new Vector3(pos_plus.x, pos_plus.y, 0);

                    last_touch_point = Input.mousePosition;

                }
            }

            if (transform.localPosition.x > maxPos.x)
            {
                transform.localPosition = new Vector3(maxPos.x, transform.localPosition.y, transform.localPosition.z);
            }
            if (transform.localPosition.x < minPos.x)
            {
                transform.localPosition = new Vector3(minPos.x, transform.localPosition.y, transform.localPosition.z);
            }
            if (transform.localPosition.y > maxPos.y)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, maxPos.y, transform.localPosition.z);
            }
            if (transform.localPosition.y < minPos.y)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, minPos.y, transform.localPosition.z);
            }

        }
        else
        {
            if (mouse_pressed == false)
            {
                last_touch_count = Input.touchCount;
            }
            else
            {
                last_touch_count = 1;
            }
            
            ready_to_transform = false;
        }


    }

}
