using UnityEngine;
using UnityEngine.InputSystem;

public class FPController : MonoBehaviour
{
    #region General variables
    [Header("Movement & Look")]
    [SerializeField] GameObject CamHolder;
    [SerializeField] float speed = 5f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float maxForce = 8f;//fuerza maxima de aceleracion
    [SerializeField] float sensitivity = 0.1f;//sensibilidad del raton

    [Header("Player State Bools")]
    [SerializeField] bool isSpinting;
    [SerializeField] bool isCrouching;
    #endregion

    Rigidbody rb;

    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Lock del cursor del raton
        Cursor.lockState = CursorLockMode.Locked;//lockea el cursor en el centro de la pantalla
        Cursor.visible = false;//apaga la visualizacion del cursor
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Input Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput= context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {

    }

    public void OnCrouch(InputAction.CallbackContext context)
    {

    }

    public void OnSprint(InputAction.CallbackContext context)
    {

    }
    #endregion 

}
