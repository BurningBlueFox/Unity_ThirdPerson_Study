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
        Vector3 characterOffset = followXForm.position + new Vector3(0f, distanceUp, 0f);

        //Calculate direction from camera to player, kill y, and normalize to give a valid direction with a unit magnitude
        lookDir = characterOffset - this.transform.position;
        lookDir.y = 0;
        lookDir.Normalize();
        Debug.DrawRay(this.transform.position, lookDir, Color.green);


        targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
        //Debug.DrawRay(follow.position, Vector3.up * distanceUp, Color.red);
        //Debug.DrawRay(follow.position, -1f * follow.forward * distanceAway, Color.blue);
        Debug.DrawLine(followXForm.position, targetPosition, Color.magenta);
        CompensateForWalls(characterOffset, ref targetPosition);



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

    private void CompensateForWalls (Vector3 fromObject, ref Vector3 toTarget) //Improve this method later needs to change distance awayy based on collision
    {
        Debug.DrawLine(fromObject, toTarget, Color.cyan);
        //Compensates for walls between camera
        RaycastHit wallHit = new RaycastHit();
        if(Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
        }
    }
    #endregion
}
