﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

public class CustomEndlessStairs : MonoBehaviour
{
    [SerializeField]
    public MeshRenderer primaryMeshRenderer;

    [SerializeField]
    public MeshRenderer secondaryMeshRenderer;

    [SerializeField]
    public MeshFilter primaryMeshFilter;

    [SerializeField]
    public MeshFilter secondaryMeshFilter;

    public Transform primaryStairs;

    public Transform secondaryStairs;

    public LayerMask lmask;

    public bool activateFirst;

    public bool activateSecond;

    public bool moving;

    public MeshRenderer PrimaryMeshRenderer => primaryMeshRenderer;

    public MeshRenderer SecondaryMeshRenderer => secondaryMeshRenderer;

    public MeshFilter PrimaryMeshFilter => primaryMeshFilter;

    public MeshFilter SecondaryMeshFilter => secondaryMeshFilter;

    public bool ActivateFirst => activateFirst;

    public bool ActivateSecond => activateSecond;

    public void Start()
    {
        lmask = 16777216;
        primaryStairs = base.transform.GetChild(0);
        secondaryStairs = base.transform.GetChild(1);
        if (RayCastCheck(base.transform.forward))
        {
            if (!RayCastCheck(base.transform.forward * -1f))
            {
                activateFirst = true;
                primaryStairs.forward = base.transform.forward;
            }
        }
        else if (RayCastCheck(base.transform.forward * -1f))
        {
            activateFirst = true;
            primaryStairs.forward = base.transform.forward * -1f;
        }

        if (RayCastCheck(base.transform.right))
        {
            if (!RayCastCheck(base.transform.right * -1f))
            {
                activateSecond = true;
                secondaryStairs.forward = base.transform.right;
            }
        }
        else if (RayCastCheck(base.transform.right * -1f))
        {
            activateSecond = true;
            secondaryStairs.forward = base.transform.right * -1f;
        }

        if (activateFirst && RayCastCheck(primaryStairs.forward, 4f))
        {
            primaryStairs.localScale = new Vector3(1f, 2f, 1f);
        }

        if (activateSecond && RayCastCheck(secondaryStairs.forward, 4f))
        {
            secondaryStairs.localScale = new Vector3(1f, 2f, 1f);
        }

        if (primaryStairs.localScale.y == 2f && activateSecond && secondaryStairs.localScale.y == 1f)
        {
            activateFirst = false;
        }
        else if (secondaryStairs.localScale.y == 2f && activateFirst && primaryStairs.localScale.y == 1f)
        {
            activateSecond = false;
        }

        Invoke("ActivationTime", 0.1f);
    }

    public bool RayCastCheck(Vector3 direction, float height = 1f)
    {
        if (Physics.Raycast(base.transform.position + Vector3.up * height, direction, 3f, lmask) && !Physics.Raycast(base.transform.position + Vector3.up * 6f, direction, 3f, lmask))
        {
            return true;
        }

        return false;
    }

    public void ActivationTime()
    {
        moving = true;
        if (activateFirst)
        {
            primaryStairs.position = base.transform.position - Vector3.up * 5f;
            primaryStairs.gameObject.SetActive(value: true);
        }

        if (activateSecond)
        {
            secondaryStairs.position = base.transform.position - Vector3.up * 5f;
            secondaryStairs.gameObject.SetActive(value: true);
        }
    }

    public void Update()
    {
        if (moving)
        {
            if (activateFirst)
            {
                primaryStairs.position = Vector3.MoveTowards(primaryStairs.position, base.transform.position, Time.deltaTime * 2f + 5f * Vector3.Distance(primaryStairs.position, base.transform.position) * Time.deltaTime);
            }

            if (activateSecond)
            {
                secondaryStairs.position = Vector3.MoveTowards(secondaryStairs.position, base.transform.position, Time.deltaTime * 2f + 5f * Vector3.Distance(secondaryStairs.position, base.transform.position) * Time.deltaTime);
            }

            CustomCybergrind parentCybergrind = GetComponentInParent<CustomCybergrind>();
            if ((!activateFirst || primaryStairs.position == base.transform.position) && (!activateSecond || secondaryStairs.position == base.transform.position) && parentCybergrind != null)
            {
                moving = false;
                GetComponentInParent<CustomCybergrind>().OnePrefabDone();
            }
        }
    }
}