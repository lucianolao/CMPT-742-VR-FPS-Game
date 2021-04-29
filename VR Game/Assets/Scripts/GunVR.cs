using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GunVR : MonoBehaviour {

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;
    public GameObject Enemy;

    public GameObject shotSound;
    public GameObject muzzlePrefab;
    public GameObject bulletHole;
    public Text healthText;
    public Text loser;


    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    Quaternion previousRotation;
    public float health = 100;
    public bool isDead;
 

    public Text magBullets;
    public Text remainingBullets;

    public int magBulletsVal = 30;
    public int remainingBulletsVal = 90;
    int magSize = 30;
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    // Use this for initialization
    void Start() {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
        health = 100;
    }

    // Update is called once per frame
    void Update() {
        
        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }


        OVRInput.Update();
        
        if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || Input.GetMouseButtonDown(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
        { 
            shotDetection(); // Should be completed

            addEffects(); // Should be completed

            animator.SetBool("fire", true);
            gunShotTime = 0.5f;
            
            // Instantiating the muzzle prefab and shot sound
            
            magBulletsVal = magBulletsVal - 1;
            if (magBulletsVal <= 0 && remainingBulletsVal > 0)
            {
                animator.SetBool("reloadAfterFire", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.5f);
            }
        }
        else
        {
            animator.SetBool("fire", false);
        }

        if ((OVRInput.GetDown(OVRInput.Button.Back) || OVRInput.Get(OVRInput.Button.Back) || OVRInput.GetDown(OVRInput.RawButton.Back) || OVRInput.Get(OVRInput.RawButton.Back) || Input.GetKey(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead )
        {
            animator.SetBool("reload", true);
            gunReloadTime = 2.5f;
            Invoke("reloaded", 2.0f);
        }
        else
        {
            animator.SetBool("reload", false);
        }
        updateText();
       
    }

  

    public void Being_shot(float damage) // getting hit from enemy
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            GetComponent<CharacterMovement>().isDead = true;
            GetComponent<CharacterController>().enabled = false;
            GetComponent<Animator>().SetBool("dead", true);
            headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
            loser.gameObject.SetActive(true);
            Invoke("reStartGame", 10.0f);
        }
        healthText.text = health.ToString();
    }

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if (eventNumber == 1)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        
        if (eventNumber == 2)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    void reloaded()
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString();
        remainingBullets.text = remainingBulletsVal.ToString();
    }

    void shotDetection() // Detecting the object which player shot 
    {
        RaycastHit rayHit;

        // start point, direction, range
        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position), out rayHit, 100.0f))
        {
            if (rayHit.transform.tag == "enemy")
            {
                //rayHit.GetComponent<Enemy>().Being_shot(20.0f);
                rayHit.transform.GetComponent<Enemy>().Being_shot(30.0f);
            } 
            else if (rayHit.transform.tag == "head")
            {
                rayHit.transform.GetComponent<BodyParts>().Being_shot(100.0f);
            }
            else if (rayHit.transform.tag == "arm")
            {
                rayHit.transform.GetComponent<BodyParts>().Being_shot(10.0f);
            }
            else if (rayHit.transform.tag == "leg")
            {
                rayHit.transform.GetComponent<BodyParts>().Being_shot(20.0f);
            }
            else // assume it's a wall instead
            {
                // moving bullet 1cm away from wall
                //Instantiate(bulletHole, rayHit.point + rayHit.transform.up * 0.01f, rayHit.transform.rotation);
                Destroy(Instantiate(bulletHole, rayHit.point + rayHit.transform.up * 0.01f, rayHit.transform.rotation), 10.0f);
            }
        }
    }

    void addEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // object (sound) deleted after 2 seconds
        Destroy(Instantiate(shotSound, transform.position, transform.rotation), 2.0f);

        // end = end position of our gun
        GameObject tempMuzzle = Instantiate(muzzlePrefab, end.transform.position, end.transform.rotation);
        tempMuzzle.GetComponent<ParticleSystem>().Play();
        Destroy(tempMuzzle, 2.0f);
    }

    public void reStartGame()
    {
        //Time.timeScale = 1.0f; // to resume from frozen screen
        SceneManager.LoadScene("SampleScene", 0);
    }

}
