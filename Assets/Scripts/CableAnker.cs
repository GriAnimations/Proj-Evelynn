using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableAnker : MonoBehaviour
{
    [SerializeField] private MeshRenderer cableAnker;

    private void Update()
    {
        gameObject.transform.position = cableAnker.bounds.center;
    }
}
