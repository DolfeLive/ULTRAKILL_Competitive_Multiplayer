using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

public class CustomEndlessPrefabAnimator : MonoBehaviour
{
    public Vector3 origPos;

    public bool moving;

    public bool reverse;

    public bool reverseOnly;

    public CustomCybergrind eg;

    public CyberPooledPrefab pooledId;

    public void Start()
    {
        if (!pooledId)
        {
            pooledId = GetComponent<CyberPooledPrefab>();
        }

        origPos = base.transform.position;
        if (!reverseOnly)
        {
            base.transform.position = origPos - Vector3.up * 20f;
            moving = true;
        }
    }

    public void Update()
    {
        if (moving)
        {
            base.transform.position = Vector3.MoveTowards(base.transform.position, origPos, Time.deltaTime * 2f + 5f * Vector3.Distance(base.transform.position, origPos) * Time.deltaTime);
            if (base.transform.position == origPos)
            {
                moving = false;
                eg = GetComponentInParent<CustomCybergrind>();
                eg.OnePrefabDone();
            }
        }
        else
        {
            if (!reverse)
            {
                return;
            }

            base.transform.position = Vector3.MoveTowards(base.transform.position, origPos - Vector3.up * 20f, Time.deltaTime * 2f + 5f * Vector3.Distance(base.transform.position, origPos) * Time.deltaTime);
            if (base.transform.position == origPos - Vector3.up * 20f)
            {
                if ((bool)pooledId)
                {
                    base.gameObject.SetActive(value: false);
                }
                else
                {
                    Destroy(base.gameObject);
                }
            }
        }
    }
}
