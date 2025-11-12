using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public Transform player; // Kéo Player GameObject vào đây

    void LateUpdate()
    {
        // Icon sẽ xoay theo hướng của player
        transform.rotation = player.rotation;
    }
}