using UnityEngine;
using System.Collections;


public class CharacterControllerLogic : MonoBehaviour
{
    #region Variables (private)
    [SerializeField]
    private Animator animator; //Reference for the animator
    [SerializeField]
    private float directionDampTime = .25f; //This is a value for a delay between the controller and the mecanim

    private float speed = 0.0f; //Value of speed
    private float h = 0.0f; //Value for the horizontal axis
    private float v = 0.0f; //Value for the vertical axis
    #endregion

    #region Properties (public)

    #endregion

    #region Unity event functions
    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator.layerCount >= 2)
        {
            animator.SetLayerWeight(1, 1);
        }
    }

    void Update()
    {
        if (animator)
        {
            //Pull values from the keyboard/controller
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            speed = new Vector2(h, v).sqrMagnitude;

            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);
        }
    }

    void OnDrawGizmos()
    {

    }
    #endregion
}
