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
    private Transform followXForm;
    [SerializeField]
    private Vector3 offset = new Vector3(0f, 1.5f, 0f);

    //Private global only
    private Vector3 lookDir;
    private Vector3 targetPosition;

    //Smoothing and damping
    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float camSmoothDampTime = 0.1f;

    #endregion

    #region Properties (public)

    #endregion

    #region Unity event functions
    void Start()
    {
        followXForm = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
       
    }

    void OnDrawGizmos()
    {

    }

    void LateUpdate()
    {
        Vector3 characterOffset = followXForm.position + offset;

        //Calculate direction from camera to player, kill y, and normalize to give a valid direction with a unit magnitude
        lookDir = characterOffset - this.transform.position;
        lookDir.y = 0;
        lookDir.Normalize();
        Debug.DrawRay(this.transform.position, lookDir, Color.green);


        targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
        //Debug.DrawRay(follow.position, Vector3.up * distanceUp, Color.red);
        //Debug.DrawRay(follow.position, -1f * follow.forward * distanceAway, Color.blue);
        Debug.DrawLine(followXForm.position, targetPosition, Color.magenta);



        smoothPosition(this.transform.position, targetPosition);

        //Make sure the camera is looking the right way
        transform.LookAt(followXForm);
    }
    #endregion

    #region Methods
    private void smoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        //Making a smooth transition between the camera's current position and the position it wants to be in
        this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }
    #endregion
}
