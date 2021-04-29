using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
	public GameObject[] target;
    public GameObject player;
    private int index;
    private bool isTargetingPlayer;
    private float waittime = 0.2f;
    private float timer = 0.0f;
    public GameObject shotSound;
    public GameObject muzzlePrefab;
    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public float health = 100;
    public bool isDead;


    void Start()
    {
        index = 0;
        isTargetingPlayer = false;
        isDead = false;
        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            return;
        }
    	Vector3 vectorToPlayer = (player.transform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Dot(transform.forward, vectorToPlayer);
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (isTargetingPlayer)
        {
            transform.LookAt(player.transform.position);
            //Quaternion desiredRotation = Quaternion.LookRotation(player.transform.position - transform.position);
            //transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime);

            if (distanceToPlayer > 10)
            {
                // start running towards player or find somewhere to hide
                GetComponent<Animator>().SetBool("fire", false);
                GetComponent<Animator>().SetBool("run", true);
            }
            else // close to player
            {
                timer += Time.deltaTime;
                GetComponent<Animator>().SetBool("run", false);
                if (timer >= waittime)
                {
                    addEffects();
                    GetComponent<Animator>().SetBool("fire", true);
                    timer -= waittime;
                    RaycastHit rayHit;

                    // start point, direction, range
                    float aimX = UnityEngine.Random.Range(-0.1f, 0.1f);
                    float aimY = UnityEngine.Random.Range(-0.1f, 0.1f);
                    Vector3 randomEnd = new Vector3(end.transform.position.x + aimX, end.transform.position.y + aimY, end.transform.position.z);
                    //if (Physics.Raycast(end.transform.position, (shootVector - start.transform.position), out rayHit, 100.0f))
                    if (Physics.Raycast(randomEnd, (randomEnd - start.transform.position), out rayHit, 100.0f))
                    { 
                        if (rayHit.transform.tag == "Player")
                        {
                            //print("SHOT");
                            player.GetComponent<GunVR>().Being_shot(20.0f);
                        }
                        else if (rayHit.transform.tag == "head")
                        {
                            player.GetComponent<GunVR>().Being_shot(100.0f);
                        }
                    }
                }
                else
                {
                    GetComponent<Animator>().SetBool("fire", false);
                }

            }

            double degreeLimit = 90;
            degreeLimit = Math.Cos(degreeLimit*Math.PI/180);
            if (player.GetComponent<GunVR>().isDead || angleToPlayer < degreeLimit || distanceToPlayer > 20)
            {
                isTargetingPlayer = false;
                GetComponent<Animator>().SetBool("fire", false);
                GetComponent<Animator>().SetBool("run", true);
            }
        }

        else // not targeting player. Walk to list of targets
        {
            GetComponent<Animator>().SetBool("run", false);
            Vector3 targetPosition = new Vector3(target[index].transform.position.x, transform.position.y, target[index].transform.position.z);
            //transform.LookAt(target.transform.position);
            Quaternion desiredRotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime);

            float distanceToTarget = Vector3.Distance(targetPosition, transform.position);
            if (distanceToTarget < 1)
            {
                // move to next target
                index = (index + 1) % target.Length;
            }

            double degreeOfView = 45;
            degreeOfView = Math.Cos(degreeOfView*Math.PI/180);
            if (angleToPlayer > degreeOfView && distanceToPlayer < 20)
            {
                if(player.GetComponent<GunVR>().isDead == false)
                {
                    isTargetingPlayer = true;
                }
                

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

    public void Being_shot(float damage) // getting hit from enemy
    {
        //print("ENEMY BEING SHOT");
        health -= damage;
        if (health <= 0)
        {
            isDead = true;
            GetComponent<CharacterController>().enabled = false;
            GetComponent<Animator>().SetBool("dead", true);
            gun.GetComponent<Rigidbody>().isKinematic = false;
            gun.GetComponent<BoxCollider>().enabled = true;
            //AddComponent<Rigidbody>();
            gun.transform.parent = null;
            //headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
        else
        {
            transform.LookAt(player.transform.position);

        }
    }
}

// 3d free ammo model



