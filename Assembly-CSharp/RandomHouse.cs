﻿using UnityEngine;

public class RandomHouse : MonoBehaviour
{
    private void Start()
    {
        base.transform.localScale = new Vector3(4f + UnityEngine.Random.Range(0f, 4f), 4f + UnityEngine.Random.Range(0f, 6f), 4f + UnityEngine.Random.Range(2f, 18f));
    }

    private void Update()
    {
    }
}