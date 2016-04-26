using UnityEngine;
using System.Collections;

public class CamRigSnap : MonoBehaviour
{
    [SerializeField]
    Transform target;

    void Update()
    {
        this.transform.position = target.position;
    }
}
