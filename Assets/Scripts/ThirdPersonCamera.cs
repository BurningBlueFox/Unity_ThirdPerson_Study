using UnityEngine;
using System.Collections;

/// <summary>
/// Struct to hold data for aligning camera
/// </summary>
struct CameraPosition
{
    //Position to align camera to, probably somewhere behind the character
    //or position to point the camera at, probably somewhere along the character's axis
    private Vector3 position;
    // Transform used for any rotation
    private Transform xForm;
    public Vector3 Position { get { return position; } set { position = value; } }
    public Transform XForm { get { return xForm; } set { xForm = value; } }


    public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
    {
        position = pos;
        xForm = transform;
        xForm.name = camName;
        xForm.parent = parent;
        xForm.localPosition = Vector3.zero;
        xForm.localPosition = position;
    }
}

[RequireComponent(typeof(BarsEffect))]
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
    private CharacterControllerLogic follow;
    [SerializeField]
    private float widescreen = 0.2f;
    [SerializeField]
    private float targetingTime = 0.5f;
    [SerializeField]
    private float firstPersonThreshold = 0.5f;
    [SerializeField]
    private Vector2 firstPersonXAxisClamp = new Vector2(-30.0f, 40.0f);


    public GameObject rig;
    public float deltaMouseX;
    public float oldMouseX;
    public float deltaMouseY;
    public float oldMouseY;
    //Smoothing and damping
    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float camSmoothDampTime = 0.1f;

    //Private global only
    private Vector3 lookDir;
    private Vector3 targetPosition;
    private BarsEffect barEffect;
    private CamStates camState = CamStates.Behind;
    private float xAxisRot = 0.0f;
    private CameraPosition firstPersonCamPos;
    private float lookWeight;
    private const float TARGETING_THRESHOLD = 0.01f;
    [SerializeField]
    private float firstPersonLookSpeed = 0.05f;

    #endregion

    #region Properties (public)
    public enum CamStates
    {
        Behind,
        FirstPerson,
        Target,
        Free
    }
    #endregion

    #region Unity event functions
    void Start()
    {
        oldMouseX = 0f;
        oldMouseY = 0f;
        follow = GameObject.FindWithTag("Beta").GetComponent<CharacterControllerLogic>();

        followXForm = GameObject.FindWithTag("Player").transform;
        lookDir = followXForm.forward;

        barEffect = GetComponent<BarsEffect>();
        if (barEffect == null)
        {
            Debug.LogError("attach a widescreen bars effect on the camera", this);
        }

        //position and parent of GameObject where first person view should be
        firstPersonCamPos = new CameraPosition();
        firstPersonCamPos.Init
            (
                "First Person Camera",
                new Vector3(0.0f, 1.6f, 0.2f),
                new GameObject().transform,
                followXForm
            );

    }

    void FixedUpdate()
    {
        deltaMouseX = Input.mousePosition.x - oldMouseX;
        oldMouseX = Input.mousePosition.x;
        deltaMouseY = Input.mousePosition.y - oldMouseY;
        oldMouseY = Input.mousePosition.y;
    }

    void OnDrawGizmos()
    {

    }

    void LateUpdate()
    {
        rig.transform.Rotate(0, deltaMouseX * 0.3f, 0, Space.Self);
       // rig.transform.Rotate(deltaMouseY * 0.3f, deltaMouseX * 0.3f, 0, Space.Self);
        //Pulls values from controller / keyboard
        float rightX = Input.GetAxis("RightStickX");
        float rightY = Input.GetAxis("RightStickY");
        float leftX = Input.GetAxis("Horizontal");
        float leftY = Input.GetAxis("Vertical");

        Vector3 characterOffset = followXForm.position + new Vector3(0f, distanceUp, 0f);
        Vector3 lookAt = characterOffset;

        //Determine camera state
        if (Input.GetAxis("Target") > TARGETING_THRESHOLD)
        {
            barEffect.coverage = Mathf.SmoothStep(barEffect.coverage, widescreen, targetingTime);

            camState = CamStates.Target;
        }
        else
        {
            barEffect.coverage = Mathf.SmoothStep(barEffect.coverage, 0f, targetingTime);

            // *First Person*
            if (Mathf.Abs(rightY) > firstPersonThreshold && !follow.IsInLocomotion() || Mathf.Abs(rightX) > firstPersonThreshold && !follow.IsInLocomotion())
            {
                //Resets look befor entering in first person mode
                xAxisRot = 0;
                lookWeight = 0f;
                camState = CamStates.FirstPerson;
            }

            // *Behind the back*
            if ((camState == CamStates.FirstPerson && Input.GetButton("ExitFPV")) ||
                camState == CamStates.Target && (Input.GetAxis("Target") <= TARGETING_THRESHOLD))
            {
                camState = CamStates.Behind;
            }
        }

        //Set the look ath weight - amount to use look at ik vs using the heads animation
        follow.Animator.SetLookAtWeight(lookWeight);

        //Executes camera state
        switch (camState)
        {
            case CamStates.Behind:

                //Calculate direction from camera to player, kill y, and normalize to give a valid direction with a unit magnitude
                lookDir = characterOffset - this.transform.position;
                lookDir.y = 0;
                lookDir.Normalize();
                Quaternion q =  Quaternion.AngleAxis(30f, lookDir);
                Vector3 angle = q.ToEulerAngles();
                angle.y = 0;
                angle.Normalize();
                Debug.DrawRay(this.transform.position, angle, Color.yellow);
                Debug.DrawRay(this.transform.position, lookDir, Color.green);

                targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
                Debug.DrawLine(followXForm.position, targetPosition, Color.magenta);

                break;
            case CamStates.FirstPerson:
                
                //Look up and down
                //Calculate the amount of rotation and apply to teh firstPersonCamPos gameObject
                xAxisRot += (leftY * 0.7f * firstPersonLookSpeed);
                xAxisRot = Mathf.Clamp(xAxisRot, firstPersonXAxisClamp.x, firstPersonXAxisClamp.y);
                firstPersonCamPos.XForm.localRotation = Quaternion.Euler(xAxisRot, 0, 0);

                //Superrimpose firstPersonCamPos GameObject's rotation on camera
                Quaternion rotatationShift = Quaternion.FromToRotation(this.transform.forward, firstPersonCamPos.XForm.forward);
                this.transform.rotation = rotatationShift * this.transform.rotation;

                //Move the firstPersonCamPos
                targetPosition = firstPersonCamPos.XForm.position;

                //Choose lookAt target based on distance
                lookAt = (Vector3.Lerp(this.transform.position + this.transform.forward, lookAt, Vector3.Distance(this.transform.position, firstPersonCamPos.XForm.position)));

                break;
            case CamStates.Target:
                lookDir = followXForm.forward; //Remove this line if you want tha camera to lock it's y rotation axis
                targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;


                break;
            case CamStates.Free:
                break;
        }


        CompensateForWalls(characterOffset, ref targetPosition);

        smoothPosition(this.transform.position, targetPosition);

        //Make sure the camera is looking the right way
        transform.LookAt(lookAt);
    }
    #endregion

    #region Methods
    private void smoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        //Making a smooth transition between the camera's current position and the position it wants to be in
        this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget) //Improve this method later needs to change distance awayy based on collision
    {
        Debug.DrawLine(fromObject, toTarget, Color.cyan);
        //Compensates for walls between camera
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
        }
    }
    #endregion
}
