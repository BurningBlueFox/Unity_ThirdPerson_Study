using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{

    #region Variables (private)
    [SerializeField]
    private float distanceAway;
    [SerializeField]
    private float distanceUp;
    [SerializeField]
    private float smooth;
    [SerializeField]
    private Transform follow;
    private Vector3 targetPosition;

    #endregion

    #region Properties (public)

    #endregion

    #region Unity event functions
    void Start()
    {
        follow = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
       
    }

    void OnDrawGizmos()
    {

    }

    void LateUpdate()
    {
        //settings the target position to be the correct offset from the hovercraft
        targetPosition = follow.position + follow.up * distanceUp - follow.forward * distanceAway;
        Debug.DrawRay(follow.position, Vector3.up * distanceUp, Color.red);
        Debug.DrawRay(follow.position, -1f * follow.forward * distanceAway, Color.blue);
        Debug.DrawLine(follow.position, targetPosition, Color.magenta);

        //Making a smooth transition between it's current position and the position it wants to be in
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smooth);

        //Make sure the camera is looking the right way
        transform.LookAt(follow);
    }
    #endregion
}
