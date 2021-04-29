using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour {
    public GameObject CameraObject;
    public GameObject CameraPlace;
    public GameObject CameraParent;
    public GameObject cameraFinalPos;
    public GameObject cameraFinalPos2;
    public GameObject neck;
    Animator animator;
    public GameObject spine;
    Quaternion spineInitialLocalRotation;
    public static bool leftHanded { get; private set; }

    public bool isDead;
    public GameObject Door;
    public GameObject Bullet1;
    public GameObject headMesh;
    public Text winner;

    // Use this for initialization
    void Start () {
        
        animator = GetComponent<Animator>();
        // Initializing animator values
        animator.SetFloat("walk_forward", 0.0f);
        animator.SetFloat("walk_backward", 0.0f);
        animator.SetFloat("walk_right", 0.0f);
        animator.SetFloat("walk_left", 0.0f);

        // Setting Initial rotation of spine to make it 
        spineInitialLocalRotation = Quaternion.Euler(new Vector3(0.0f, 40.0f, 0.0f));

    }

    // Update is called once per frame
    void Update () {
        OVRInput.Update();
        // Getting touch-pad touch position
        Vector2 touchPos = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, OVRInput.Controller.RTrackedRemote);

        if (!isDead)
        {
            // The player should not move
            if (touchPos.magnitude < 0.1f)
            {
                animator.SetFloat("walk_forward", -1f);
                animator.SetFloat("walk_backward", -1f);
                animator.SetFloat("walk_right", -1f);
                animator.SetFloat("walk_left", -1f);
                animator.SetFloat("animation_speed", 0.0f);

                // Unless moving through keyboard input
                float speed = 2.5f;
                if (Input.GetKey(KeyCode.W))
                {
                    speed -= 0.5f;
                    animator.SetFloat("walk_forward", 2.0f);
                    animator.SetFloat("animation_speed", speed+0.5f);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    speed -= 0.5f;
                    animator.SetFloat("walk_backward", 1.0f);
                    animator.SetFloat("animation_speed", speed);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    speed -= 0.5f;
                    animator.SetFloat("walk_right", 1.0f);
                    animator.SetFloat("animation_speed", speed);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    speed -= 0.5f;
                    animator.SetFloat("walk_left", 1.0f);
                    animator.SetFloat("animation_speed", speed);
                }
            }
            else // The player should move
            {
                float forwardSpeed = touchPos.y;
                if (forwardSpeed > 0) // making forward walking speed faster
                {
                    forwardSpeed = forwardSpeed * 2;
                }

                // Running the correct animation
                animator.SetFloat("walk_forward", forwardSpeed);
                animator.SetFloat("walk_backward", -touchPos.y);
                animator.SetFloat("walk_right", touchPos.x);
                animator.SetFloat("walk_left", -touchPos.x);

                // Setting animation running speed
                animator.SetFloat("animation_speed", Mathf.Sqrt(Mathf.Pow(touchPos.x, 2f) + Mathf.Pow(forwardSpeed, 2f)));
            }

            if (Bullet1.activeSelf == true)
            {
                float distanceToBullet1 = Vector3.Distance(transform.position, Bullet1.transform.position);
                if (distanceToBullet1 < 1)
                {
                    Bullet1.SetActive(false);
                    GetComponent<GunVR>().remainingBulletsVal += 30;
                }
            }
            //cameraFinalPos.transform.position
        }
    }



    private void LateUpdate()
    {

        // If the handset is held with right hand or left hand
        leftHanded = OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch);
        OVRInput.Controller c = leftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

        if (OVRInput.GetControllerPositionTracked(c) && !isDead)
        {
            // Rotating the player spine according to handset rotation
            spine.transform.rotation = OVRInput.GetLocalControllerRotation(c);
            spine.transform.localRotation = spineInitialLocalRotation * spine.transform.localRotation;
           

            if (Quaternion.Angle(Quaternion.identity, spine.transform.localRotation) > 30.0f) // Limiting the spine rotation  
            {
                Quaternion currentLocalRotation = spine.transform.localRotation;
                spine.transform.localRotation = Quaternion.Slerp(Quaternion.identity, currentLocalRotation, 30.0f / Quaternion.Angle(Quaternion.identity, spine.transform.localRotation)); //Spherical interpolation of the spine rotation 
            }
        }
        

        if (!isDead) // Rotating the character according to headset rotation.
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(CameraObject.transform.forward.x, 0.0f, CameraObject.transform.forward.z).normalized, Vector3.up);
        }


        if (!isDead) // Moving the camera to a predifined position according to spine position and rotation
        {
            CameraParent.transform.position = CameraPlace.transform.position;
        }
        else // Moving camera to a fixed point when the player is killed
        {
            float distanceToCamera1 = Vector3.Distance(transform.position, cameraFinalPos.transform.position);
            if (distanceToCamera1 < 29)
            {
                CameraParent.transform.position = cameraFinalPos.transform.position;
            }
            else
            {
                CameraParent.transform.position = cameraFinalPos2.transform.position;
            }
        }

    }


    public void OnTriggerEnter(Collider Door)
    {
        //print("YAY");
        GetComponent<CharacterController>().enabled = false;
        GetComponent<GunVR>().enabled = false;
        GetComponent<CharacterMovement>().enabled = false;
        Destroy(Door.gameObject);
        //Time.timeScale = 0f; // freezing screen
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
        winner.gameObject.SetActive(true);
        Invoke("reStartGame", 10.0f);
        //Time.timeScale = 1.0f; // to resume from frozen screen
   
    }

    public void reStartGame()
    {
        //Time.timeScale = 1.0f; // to resume from frozen screen
        SceneManager.LoadScene("SampleScene", 0);
    }

}
