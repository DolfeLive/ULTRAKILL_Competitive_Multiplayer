using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

namespace ULTRAKILL_Competitive_Multiplayer;

public class CustomEndlessCube : MonoBehaviour
{
    [SerializeField]
    public MeshRenderer meshRenderer;
    [SerializeField]
    public MeshFilter meshFilter;
    public Vector2Int positionOnGrid;
    public bool blockedByPrefab;
    public Vector3 targetPos;
    public Transform tf;
    public bool active;
    public float speed;

    private CustomCybergrind eg;

    public MeshRenderer MeshRenderer => meshRenderer;
    public MeshFilter MeshFilter => meshFilter;

    public void Awake()
    {
        tf = base.transform;

        eg = GetComponentInParent<CustomCybergrind>();
        
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
    }

    public void Update()
    {
        if (active)
        {
            if (tf == null) tf = transform;

            tf.position = Vector3.MoveTowards(tf.position, targetPos,
                (Vector3.Distance(tf.position, targetPos) * 1.75f + speed) * Time.deltaTime);

            if (tf.position == targetPos)
            {
                eg.OneDone();
                active = false;
            }
        }
    }

    public void SetTarget(float target)
    {
        if (tf == null) tf = transform;

        targetPos = new Vector3(tf.position.x, target, tf.position.z);
        speed = Random.Range(9, 11);
        Invoke("StartMoving", Random.Range(0f, 0.5f));
    }

    public void StartMoving()
    {
        active = true;
    }
}