using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class PlayerController : MonoBehaviour
{
    ///////////Basic Player Movement///////////
    public Transform PlayerTransform;
    private CharacterController Controller;
    private Vector3 PlayerVelocity;
    private bool GroundedPlayer;
    private bool Crouched = false;
    private bool Running = false;
    private bool Walking = false;
    public float PlayerSpeed = 1.6f;
    public static float Stamina = 7f;
    public float StaminaTimeLimit = 7f;
    public float StaminaTimeUntilRegen = 2f;
    private float TimeSinceRun = 0;
    private float GravityValue = -9.81f;
    private float ControllerHeight = 1f;

    ///////////Variables for XRay///////////
    [Space]

    [Header("XRay Shader variables")]
    public float xrayTimeLimit = 5f;
    [HideInInspector] public static float xraytime = 5f;
    public float xRayTimeUntilRegen = 2f;
    private float timeSinceXray = 0;
    public Volume volume;
    Vignette vignette;
    ColorAdjustments colorAdj;
    FilmGrain filmGrain;

    [Space]

    ///////////Variables for Attacking///////////
    public Transform EnemyTransform;

    ///////////Variables for Kunai///////////
    public Rigidbody Kunai;
    public int AmountOfKunais = 3;

    ///////////Variables for Distractable///////////
    public GameObject Distractable;
    public static int AmountOfDistractables = 3;

    ///////////Variables for Audio///////////
    public AudioSource RunAudio;
    public AudioSource WalkAudio;


    ///////////Variables for KeyCard///////////
    public static int KeyCards = 0;

    ///////////Variables for Checking for Slopes///////////
    RaycastHit HitInfo;
    Vector3 forward;
    public float MaxGroundAngle = 120f;
    public float Height = .5f;
    public float HeightPadding = .05f;
    public LayerMask Ground;
    private float Angle;
    private float GroundAngle;
    

    


    private void Start()
    {
        Controller = GetComponent<CharacterController>();
       
        if (volume != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdj);
            volume.profile.TryGet(out filmGrain);
        }

        GameManager.Instance.keyCards = 0;
        
    }

    void Update()
    {


        

        if (GroundAngle >= MaxGroundAngle)
        {
            PlayerSpeed = 0f;
        }
        else if (Walking == true)
        {
            PlayerSpeed = 1.2f;
        }
        else if (Running == true) 
        {
            PlayerSpeed = 4.0f;
        }

        if (WalkAudio != null)
        {
           CheckAudio();
        }

        ///////////Checking for Slopes///////////
        CalculateForward();
        CalculateGroundAngle();
        CheckGrounded();
       

        ///////////Player movement (Left, Right, Forward, Bacward)///////////

        Vector3 Move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if ((Controller.velocity.x > 0f && Controller.velocity.x < 2f) || (Controller.velocity.z > 0f && Controller.velocity.z < 2f)) 
        {
            Walking = true;
        }

        ///////////Camera Movement///////////
        if (Camera.main != null)
        {
            Controller.Move(Camera.main.transform.right * Move.x * Time.deltaTime * PlayerSpeed);
            Controller.Move(Camera.main.transform.forward * Move.z * Time.deltaTime * PlayerSpeed); 
            transform.forward = Camera.main.transform.forward;

            Vector3 angles = transform.rotation.eulerAngles;
            angles.x = 0;
            angles.z = 0;

            transform.rotation = Quaternion.Euler(angles);

        }
        
        
        ///////////Player Run///////////
        if (Input.GetButton("Run"))
        {
            
            if (Stamina >= .1f)
            {
                Running = true;
                Walking = false;
                Stamina -= Time.deltaTime;
    
            }
            TimeSinceRun = Time.time + StaminaTimeUntilRegen;
        }
        else 
        {
            Walking = true;
            Running = false;
            if (Stamina <= 6.0f) {
                if (TimeSinceRun <= Time.time)
                {
                    Stamina += Time.deltaTime;

                }
            }
            
        }

        Stamina = Mathf.Clamp(Stamina, 0, StaminaTimeLimit);



        //Check for XRay
        if (Input.GetButton("XRay"))
        {
            if (xraytime >= .1f)
            {
                Shader.SetGlobalFloat("_GlobalVisibility", 1f);
                Debug.Log(vignette);
                if (vignette != null)
                    vignette.intensity.value = 0.6f;
                if (colorAdj != null)
                    colorAdj.hueShift.value = -40;
                if (filmGrain != null)
                    filmGrain.intensity.value = 0.6f;
                xraytime -= Time.deltaTime;
            }
            else
            {
                Shader.SetGlobalFloat("_GlobalVisibility", 0f);
                if (vignette != null)
                    vignette.intensity.value = 0f;
                if (colorAdj != null)
                    colorAdj.hueShift.value = 0;
                if (filmGrain != null)
                    filmGrain.intensity.value = 0f;
            }
            timeSinceXray = Time.time + xRayTimeUntilRegen;
        }
        else
        {
            Shader.SetGlobalFloat("_GlobalVisibility", 0f);
            if (vignette != null)
                vignette.intensity.value = 0f;
            if (colorAdj != null)
                colorAdj.hueShift.value = 0;
            if (filmGrain != null)
                filmGrain.intensity.value = 0f;

            if (timeSinceXray <= Time.time)
            {
                xraytime += Time.deltaTime;

            }
        }

        xraytime = Mathf.Clamp(xraytime, 0, xrayTimeLimit);
        




        ///////////Player Crouch///////////
        if (Input.GetButtonDown("Crouch"))
        {
            Crouched = !Crouched;
            ToggleCrouch();
           
        }


        ///////////Create Distractable///////////
        if (Input.GetKeyDown(KeyCode.Q))
        {

            if (AmountOfDistractables > 0)
            {
                CreateDistractable();
            }
            else
            {
                Debug.Log("Out of Distractions");
            }

        }


        ///////////Reloads Distractables///////////
        ///Will Change after reload stations are included
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadDistractables();
        }


        ///////////Checking the amount of keycards the player has///////////
        if (Input.GetKeyDown(KeyCode.F))
        {
            CheckKeyCards();
        }

        PlayerVelocity.y += GravityValue * Time.deltaTime;
        Controller.Move(PlayerVelocity * Time.deltaTime);
        //transform.position += forward * PlayerSpeed * Time.deltaTime;
    }


    ///////////Player Actions///////////


    ///Distractions
    void CreateDistractable()
    {
        Instantiate(Distractable, transform.position, transform.rotation);
        AmountOfDistractables -= 1;
        Debug.Log("Distractions left: " + AmountOfDistractables);
    }



    ///Reloading Distractables
    void ReloadDistractables()
    {
        AmountOfDistractables = 3;
        Debug.Log("Refilled Distractables");
    }


    ///Crouching
    void ToggleCrouch()
    {
        if (Crouched == true)
        {
            PlayerTransform.transform.localScale = new Vector3(1f, .5f, 1f);
            Controller.height = ControllerHeight;
            PlayerSpeed = .4f;
            Debug.Log("Player speed is now " + PlayerSpeed + " and the player is crouched");
        }
        else
        {
                PlayerTransform.transform.localScale = new Vector3(1f, ControllerHeight, 1f);
                Controller.height = 1f;
                PlayerSpeed = 1.2f;
        }
    }

    void CheckKeyCards()
    {
        Debug.Log("Keycards = " + KeyCards);
    }

    void CheckAudio() 
    {
        if (Walking == true)
{
            RunAudio.Stop();

            if (WalkAudio.isPlaying == false)
                WalkAudio.Play();
        }
        else if (Running == true)
{
            WalkAudio.Stop();

            if (RunAudio.isPlaying == false)
            {
                RunAudio.Play();
            }
        }
        else
        {
            WalkAudio.Stop();
            RunAudio.Stop();
           
        }
    }

    void CalculateForward() 
    {
        if (GroundedPlayer == false) 
        {
            forward = transform.forward;
        }
        forward = Vector3.Cross(HitInfo.normal, -transform.right);
    }

    void CalculateGroundAngle() 
    {
        if (GroundedPlayer == false) 
        {
            GroundAngle = 90;
            return;
        }

        GroundAngle = Vector3.Angle(HitInfo.normal, transform.forward);
    }

    void CheckGrounded() 
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out HitInfo, Height + HeightPadding, Ground))
        {
            GroundedPlayer = true;
        }
        else 
        {
            GroundedPlayer = false;
        }
    }

   


}
   






