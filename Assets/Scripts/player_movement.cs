using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    [Header("Pause Menu")]
    pause_menu pauseMenu;

    [Header("Base Movement")]
    public float moveSpeed;
    public float maxSpeed;
    private float originalMaxSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private float startHeight;
    bool jumpReady;
    bool doubleJumpReady, doubleJumpend;
    bool moving;


    [Header("Gravity Swap")]
    public float numRotations;
    public float rotationCooldown;
    public float duracion;
    bool rotationReady;


    [Header("Dash")]
    public float dashForce;
    public float numDashes;
    public float dashCooldown;
    public float dashDuration;
    public float speedDuringDash;
    bool dashReady;


    [Header("Slide")]
    public float slideForce;
    public float slideHeight;
    bool slideStart;
    
   
    [Header("CheckOnGround")]
    public float playerHeight;
    public LayerMask IsGround, IsGroundfrfr;
    bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode fowardKey = KeyCode.W;

    [Header("WallRun")]
    public float wallRunForce;
    public float maxWallRunTime;
    public float wallRunTimer;
    public float wallRunSpeed;
    public float wallRunCameraTilt;
    public float maxWallRunCameraTilt;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeigth;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    public LayerMask isWall;
    private bool wallRunState;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    Vector3 moveDirection;

    Vector3 angulo= new Vector3(0f, 0f, 0f);
    Quaternion caida;
    Quaternion inicio;
    
    float ratio;
    float tiempo;

    Camera camaraPrincipal;

    Rigidbody rb;
    ConstantForce gravedad;

    private InfiniteSoulsManager InfiniteSoulsManager = null;

    private Quaternion InitialRotation;

    [SerializeField]
    private StatsMenu StatsMenu;

    private void Start()
    {
        InfiniteSoulsManager = FindObjectOfType<InfiniteSoulsManager>();
        //StatsMenu = FindObjectOfType<StatsMenu>();
        pauseMenu = FindObjectOfType<pause_menu>();
        rb = GetComponent<Rigidbody>();
        gravedad = GetComponent<ConstantForce>();
        InitialRotation = inicio = transform.rotation;
        ratio = 1.0f / duracion;
        tiempo = 0.0f;
        rb.freezeRotation = true;
        JumpReset();
        DashReset();
        rotationReady = true;
        camaraPrincipal = Camera.main;
        startHeight = transform.localScale.y;
        slideStart = true;
        moving = false;
        originalMaxSpeed = maxSpeed;
    }

    private void Update()
    {
        if (pauseMenu.GetIsPaused() || StatsMenu.AreStatsShowing()) { 
            foreach(AudioSource audio in GetComponents<AudioSource>())
            {
                audio.Pause();
            }
            return; 
        }

        grounded = Physics.Raycast(transform.position, -transform.up , playerHeight * 0.5f + 0.2f, IsGround) 
                   || Physics.Raycast(transform.position, -transform.up, playerHeight * 0.5f + 0.2f, IsGroundfrfr);

        PlayerInput();
        SpeedLimit();
        checkForWall();
        wallRunStates();

        if (grounded)
        {
            rb.drag = groundDrag;
            doubleJumpReady = false;
            doubleJumpend = false;
        }
        else
        {
            if (!doubleJumpend && !doubleJumpReady)
            {
                Invoke(nameof(DoubleJumpReset), jumpCooldown);
            }
            rb.drag = 0;
        }

        if (tiempo <= 1.0 && !rotationReady)
        {
            tiempo += Time.deltaTime * ratio;
            transform.rotation = Quaternion.Slerp(inicio, caida, tiempo);
            if (tiempo > 1.0)
            {
                inicio = transform.rotation;
                Invoke(nameof(RotationRecover), rotationCooldown);
                Invoke(nameof(RotationReset), 0.0005f);
            }
        }


        //DEBUG
        //Debug.Log(rb.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        Move();
        if (wallRunState)
        {
            
            wallRunMovement();
        }
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey))
        {
            if (grounded && jumpReady)
            {
                jumpReady = false;
                Jump();
                //Invoke(nameof(DoubleJumpReset), jumpCooldown);
                Invoke(nameof(JumpReset), jumpCooldown);
            }
            else if (doubleJumpReady && !doubleJumpend)
            {
                doubleJumpend = true;
                doubleJumpReady = false;
                DoubleJump();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(numDashes > 0 && dashReady)
            {
                dashReady = false;
                numDashes -= 1;
                moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
                Dash(moveDirection);
                Invoke(nameof(DashRecover), dashCooldown);
                Invoke(nameof(DashReset), dashDuration);
                Invoke(nameof(DashEnd), dashDuration);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, slideHeight, transform.localScale.z);
            rb.AddForce(-transform.up * 5f, ForceMode.Impulse);
            if (slideStart)
            {
                moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
                Slide(moveDirection);
                slideStart = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
            slideStart = true;
        }

            float rotacion = camaraPrincipal.transform.localEulerAngles.y;
        //Debug.Log(rotacion);
        if (Input.GetKey(KeyCode.Q))
        {
            if (rotacion <= 325f && rotacion >= 235f) GiroPositivoX();
            if (rotacion <= 135f && rotacion >= 45f) GiroNegativoX();
            if (rotacion <= 45f || rotacion >= 325f) GiroNegativoZ();
            if (rotacion <= 235f && rotacion >= 135f) GiroPositivoZ();
        }
        if (Input.GetKey(KeyCode.E))
        {
            if (rotacion <= 325f && rotacion >= 235f) GiroNegativoX();
            if (rotacion <= 135f && rotacion >= 45f) GiroPositivoX();
            if (rotacion <= 45f || rotacion >= 325f) GiroPositivoZ();
            if (rotacion <= 235f && rotacion >= 135f) GiroNegativoZ();

        }
        if (Input.GetKey(KeyCode.F))
        {
            if (rotacion <= 325f && rotacion >= 235f) GiroNegativoZ();
            if (rotacion <= 135f && rotacion >= 45f) GiroPositivoZ();
            if (rotacion <= 45f || rotacion >= 325f) GiroNegativoX();
            if (rotacion <= 235f && rotacion >= 135f) GiroPositivoX();
        }
        if (Input.GetKey(KeyCode.C))
        {
            if (rotacion <= 325f && rotacion >= 235f) GiroPositivoZ();
            if (rotacion <= 135f && rotacion >= 45f) GiroNegativoZ();
            if (rotacion <= 45f || rotacion >= 325f) GiroPositivoX();
            if (rotacion <= 235f && rotacion >= 135f) GiroNegativoX();
        }

    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            moving = moveDirection != new Vector3();
            if (moving)
            {
                AudioSource sonido = GetComponents<AudioSource>()[0];
                sonido.enabled = true;
            }
            else{
                AudioSource sonido = GetComponents<AudioSource>()[0];
                sonido.enabled = false;
            }
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);
        }
        else
        {
            AudioSource sonido = GetComponents<AudioSource>()[0];
            sonido.enabled = false;
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedLimit()
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        AudioSource sonido = GetComponents<AudioSource>()[3];
        sonido.Play();
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        AudioSource sonido = GetComponents<AudioSource>()[4];
        sonido.Play();
        if (InfiniteSoulsManager != null)
        {
            InfiniteSoulsManager.PlayerDoubleJumped();
        }
    }

    public void Dash(Vector3 moveDirection)
    {
        maxSpeed = speedDuringDash;
        rb.AddForce(moveDirection.normalized * dashForce, ForceMode.Impulse);
        AudioSource sonido = GetComponents<AudioSource>()[2];
        sonido.Play();
        if (InfiniteSoulsManager != null)
        {
            InfiniteSoulsManager.PlayerDashed();
        }
    }

    public void Slide(Vector3 moveDirection)
    {
        rb.AddForce(moveDirection.normalized * slideForce, ForceMode.Impulse);
    }

    private void GiroPositivoX()
    {
        if (rotationReady && numRotations > 0)
        {
            rotationReady = false;
            numRotations--;
            caida = inicio * Quaternion.Euler(new Vector3(90f, 0, 0));
            tiempo = 0.0f;
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.Play();
            if (InfiniteSoulsManager != null)
            {
                InfiniteSoulsManager.PlayerChangedGravity();
            }
        }
    }

    private void GiroNegativoX()
    {
        if (rotationReady && numRotations > 0)
        {
            rotationReady = false;
            numRotations--;
            caida = inicio * Quaternion.Euler(new Vector3(-90f, 0, 0));
            tiempo = 0.0f;
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.Play();
            if (InfiniteSoulsManager != null)
            {
                InfiniteSoulsManager.PlayerChangedGravity();
            }
        }
    }

    private void GiroPositivoZ()
    {
        if (rotationReady && numRotations > 0)
        {
            rotationReady = false;
            numRotations--;
            caida = inicio * Quaternion.Euler(new Vector3(0, 0, 90f));
            tiempo = 0.0f;
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.Play();
            if (InfiniteSoulsManager != null)
            {
                InfiniteSoulsManager.PlayerChangedGravity();
            }
        }
    }
    private void GiroNegativoZ()
    {
        if (rotationReady && numRotations > 0)
        {
            rotationReady = false;
            numRotations--;
            caida = inicio * Quaternion.Euler(new Vector3(0, 0,-90f));
            tiempo = 0.0f;
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.Play();
            if (InfiniteSoulsManager != null)
            {
                InfiniteSoulsManager.PlayerChangedGravity();
            }
        }
    }


    private void JumpReset()
    {
        jumpReady = true;
    }

    private void DoubleJumpReset()
    {
        doubleJumpReady = true;
    }

    private void RotationRecover()
    {
        numRotations++;
    }

    private void RotationReset()
    {
        rotationReady = true;
    }

    private void DashRecover()
    {
        numDashes += 1;
    }
    private void DashReset()
    {
        dashReady = true;
    }

    private void DashEnd()
    {
        maxSpeed = originalMaxSpeed;
    }

    private void checkForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, isWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, isWall);

       // if (wallLeft || wallRight) doubleJumpReady = true;
    }

    private void wallRunStates()
    {
        if ( wallLeft && verticalInput > 0 && !grounded && !Input.GetKey(KeyCode.D))
        {
            wallRunState = true;
            gravedad.relativeForce = new Vector3(0, 0, 0);
        }else if (wallRight && verticalInput > 0 && !grounded && !Input.GetKey(KeyCode.A))
        {
            wallRunState = true;
            gravedad.relativeForce = new Vector3(0, 0, 0);
        }
        else
        {
            wallRunState = false;
            gravedad.relativeForce = new Vector3(0, -45, 0);
        }

    }

    private void wallRunMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        //Velocidad hacia adelante
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //Velocidad hacia el muro
        rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    public bool IsPlayerGrounded()
    {
        return grounded;
    }


    public void SetOriginalRotation()
    {
        transform.rotation = InitialRotation;
        inicio = transform.rotation;
    }
}
