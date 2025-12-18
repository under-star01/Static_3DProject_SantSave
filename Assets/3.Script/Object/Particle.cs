using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 5f);
    }
}
