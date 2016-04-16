using UnityEngine;
using System.Collections;


public class CharacterControllerLogic : MonoBehaviour
{
    #region Variables (private)
    //Inspector serialiazed
    [SerializeField]
    private Animator animator; //Reference for the animator
    [SerializeField]
    private float directionDampTime = .25f; //This is a value for a delay between the controller and the mecanim
    [SerializeField]
    private ThirdPersonCamera gamecam;
    [SerializeField]
    private float directionSpeed = 1.5f;
    [SerializeField]
    private float rotationDegreePerSecond = 120f;

    //Private global only
    private float speed = 0.0f; //Value of speed
    private float direction = 0f; //Value of direction
    private float horizontal = 0.0f; //Value for the horizontal axis
    private float vertical = 0.0f; //Value for the vertical axis
    private AnimatorStateInfo stateInfo;

    //Hashes
    private int m_LocomotionId = 0;
    #endregion

    #region Properties (public)

    public Animator Animator
    {
        get
        {
            return this.animator;
        }
    }

    #endregion

    #region Unity event functions
    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator.layerCount >= 2)
        {
            animator.SetLayerWeight(1, 1);
        }

        //Hash all animation names for performances
        m_LocomotionId = Animator.StringToHash("Base Layer.Locomotion");
    }

    void Update()
    {
        if (animator)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            //Pull values from the keyboard/controller
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            //Translate the controls stick cordinate into world/cam/character space
            StickToWorldSpace(this.transform, gamecam.transform, ref direction, ref speed);

            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);

        }
    }

    /// <summary>
    /// Any code that moves the character needs to be checked against physics
    /// </summary>
    void FixedUpdate()
    {
        //Rotate the character's model if stick is tilted right or left, but only if character is moving in that direction
        if(IsInLocomotion() && ((direction >= 0 && horizontal >= 0) || (direction < 0 && horizontal < 0))) //Improve this to work if character moves downward as well
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegreePerSecond * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            this.transform.rotation = (this.transform.rotation * deltaRotation);

        }
    }

    void OnDrawGizmos()
    {

    }
    #endregion

    #region Methods
    public void StickToWorldSpace(Transform root, Transform camera, ref float directionOut, ref float speedOut)
    {
        Vector3 rootDirection = root.forward;

        Vector3 stickDirection = new Vector3(horizontal, 0, vertical);

        speedOut = stickDirection.sqrMagnitude;

        //Get camera rotation
        Vector3 CameraDirection = camera.forward;
        CameraDirection.y = 0.0f; //Kill y
        Quaternion referencialShift = Quaternion.FromToRotation(Vector3.forward, CameraDirection);

        //Convert joystick input in Worldspace coordinates
        Vector3 moveDirection = referencialShift * stickDirection;
        Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), moveDirection, Color.green);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), rootDirection, Color.magenta);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), stickDirection, Color.blue);

        float angleRootToMove = Vector3.Angle(rootDirection, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

        angleRootToMove /= 180f;

        directionOut = angleRootToMove * directionSpeed;
    }

    public bool IsInLocomotion()
    {
        return stateInfo.nameHash == m_LocomotionId;
    }
    #endregion
}
