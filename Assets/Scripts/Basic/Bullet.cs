using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    [HideInInspector] public string teamID;
    [SerializeField] LayerMask firstInteractionLayers;
    [SerializeField] LayerMask moduleLayer;
    public float damage;
    public float speed;
    public float lifetime;
    float lifetimer;
    Rigidbody2D body;
    float OneFramePosChange;
    Vector2 OneFrameVectorMove;

    public void CustomStart()
    {
        OneFramePosChange = speed * Time.fixedDeltaTime;
        body = GetComponent<Rigidbody2D>();
        CalculateOneFrameVectorMove();
        RaycastDamageTargetTryToFind();
    }

    private void FixedUpdate()
    {
        CalculateOneFrameVectorMove();
        OneFramePosChange = Vector2.Distance(new Vector2(), body.velocity) * Time.deltaTime;
        Lifetime();
        RaycastDamageTargetTryToFind();
    }

    void Lifetime()
    {
        lifetimer += Time.deltaTime;
        if (lifetimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void CalculateOneFrameVectorMove()
    {
        if (body.velocity != new Vector2(0, 0))
        {
            OneFrameVectorMove = body.velocity;
        }
        else
        {
            float radiansAngle = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
            OneFrameVectorMove = new Vector2(Mathf.Cos(radiansAngle), Mathf.Sin(radiansAngle)) * speed;
        }
    }

    void RaycastDamageTargetTryToFind()
    {
        float distance = OneFramePosChange + 1;
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, OneFrameVectorMove, distance, firstInteractionLayers);

        if (hitInfo.collider != null && hitInfo.collider.GetComponent<ShipStats>() != null)
        {
            ShipTouched(hitInfo.collider.GetComponent<ShipStats>());
        }
    }

    void ShipTouched(ShipStats ship) //луч коснулся корабля
    {
        if (ship.teamID != teamID) //если враг
        {
            ship.TakeDamage(damage);
            DamageModules(ship.teamID);
            Destroy(gameObject);
        }
        else //если союзник
        {

            Vector2 startPos = transform.position;
            float passedDistance = 0;
            bool stop = false;
            bool returnToStartPos = false;
            float step = 0.25f;

            while (passedDistance < OneFramePosChange && !stop)
            {
                RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, OneFrameVectorMove, step, firstInteractionLayers);
                if (hitInfo.collider != null)
                {
                    ShipStats shipInfo = hitInfo.collider.GetComponent<ShipStats>();
                    if (shipInfo != null) //луч попал в корабль
                    {
                        if (shipInfo.teamID == teamID) //союзник
                        {
                            //Debug.Log("Союзник");
                            transform.Translate(Vector2.up * step);
                            passedDistance += step;
                            returnToStartPos = true;
                        }
                        else //враг
                        {
                            RaycastDamageTargetTryToFind();
                            //Debug.Log("Враг");
                            stop = true;
                        }
                    }
                    else //не смог получить ShipStats
                    {
                        //Debug.Log("Не получил");
                        RaycastDamageTargetTryToFind();
                        stop = true;
                    }
                }
                else //луч никуда не попал
                {
                    //Debug.Log("Не попал");
                    RaycastDamageTargetTryToFind();
                    stop = true;
                }
            }

            if (returnToStartPos)
            {
                transform.position = startPos;
            }

        }
    }

    void DamageModules(string targetTeamID)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, OneFrameVectorMove, 100, moduleLayer);
        if (hitInfo.collider != null && hitInfo.collider.transform.parent != null && hitInfo.collider.transform.parent.GetComponent<ShipStats>() != null && hitInfo.collider.transform.parent.GetComponent<ShipStats>().teamID == targetTeamID)
        {
            //модуль того же корабля, в который попала пуля
            Module module = hitInfo.collider.GetComponent<Module>();
            if (module != null)
            {
                module.TakeDamage(damage);
            }
        }
    }

}
