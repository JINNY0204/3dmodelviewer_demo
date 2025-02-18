using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlexibleScale : MonoBehaviour
{
    public float minScale;
    public float maxScale;
    public float fixedRate = 0.5f;
    Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (Camera.main != null)
        {
            if (transform.localScale.x >= minScale && transform.localScale.x <= maxScale)
            {
                Plane plane = new Plane(Camera.main.transform.forward, Camera.main.transform.position);
                float dist = plane.GetDistanceToPoint(transform.position);
                transform.localScale = originalScale * dist * fixedRate;
            }

            if (transform.localScale.x < minScale)
            {
                Vector3 calScale = transform.localScale;
                calScale.x = minScale;
                calScale.y = minScale;
                calScale.z = minScale;
                transform.localScale = calScale;
            }
            if (transform.localScale.x > maxScale)
            {
                Vector3 calScale = transform.localScale;
                calScale.x = maxScale;
                calScale.y = maxScale;
                calScale.z = maxScale;
                transform.localScale = calScale;
            }
        }
    }
}
