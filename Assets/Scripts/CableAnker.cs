using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableAnker : MonoBehaviour
{
    [SerializeField] private MeshRenderer cableAnker;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private float yIncrease;

    private void Update()
    {
        var targetPos = new Vector3(cableAnker.bounds.center.x, cableAnker.bounds.center.y + yIncrease);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
    }
}
