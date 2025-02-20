using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class t200_soul : MonoBehaviour
{

    [Header("Pause Menu")]
    pause_menu pauseMenu;

    public NavMeshAgent t200;
    public GameObject skull;
    Animator t200Anim;

    public Transform player;

    public GameObject altar;

    public LayerMask isGround, isPlayer;
    public RaycastHit rayHit;

    private bool WasStopped;

    //Idle-Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    public float meleeColliderStart;
    public float meleeColliderDuration;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    bool playerInSightRange, playerInAttackRange, stunned;

    //Health and damage values
    public float health;
    public float maxHealth;
    private float originalHealth;
    public float reviveTime;
    float stunMoment;

    //Infinite Souls Manager
    private InfiniteSoulsManager InfiniteSoulsManager;

    private void Awake()
    {
        player = GameObject.Find("Capsule").transform;
        t200 = GetComponent<NavMeshAgent>();
        t200Anim = GetComponent<Animator>();
        InfiniteSoulsManager = FindObjectOfType<InfiniteSoulsManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        t200.enabled = false;
        t200.enabled = true;
        //t200.updateUpAxis = false;
        pauseMenu = FindObjectOfType<pause_menu>();
        health = maxHealth;
        originalHealth = maxHealth;
        stunned = false;
        //t200.transform.up = -skull.transform.up;
    }

    // Update is called once per frame
    void Update()
    {

        if (pauseMenu.GetIsPaused())
        {
            AudioSource sonido1 = GetComponents<AudioSource>()[1];
            sonido1.enabled = false;
            AudioSource sonido2 = GetComponents<AudioSource>()[3];
            sonido2.enabled = false;
            return;
        }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, isPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, isPlayer);

        if (!playerInSightRange && !playerInAttackRange && health > 0)
        {
            Patroling();
            t200Anim.SetBool("walkin", true);
            t200Anim.SetBool("stunned", false);
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.enabled = true;
        }
        else if (!playerInAttackRange && health > 0)
        {
            ChasePlayer();
            t200Anim.SetBool("walkin", true);
            t200Anim.SetBool("stunned", false);
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.enabled = true;
        }
        else if (health > 0)
        {
            t200Anim.SetBool("stunned", false);
            AttackPlayer();
            //AudioSource sonido = GetComponents<AudioSource>()[3];
            //sonido.enabled = true;
        }
        else if (!stunned)
        {
            Stun();
            t200Anim.SetBool("stunned", true);
           // t200Anim.SetBool("walkin", false);
            AudioSource sonido = GetComponents<AudioSource>()[1];
            sonido.enabled = false;
            AudioSource sonido2 = GetComponents<AudioSource>()[2];
            sonido2.Play();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet) FindWalkPoint();
        else
        {
            t200.isStopped = false;
            t200.SetDestination(walkPoint);
            Vector3 walkLook = new Vector3(walkPoint.x, transform.position.y, walkPoint.z);
            transform.LookAt(walkLook);
        }

        Vector3 distanceToWalk = transform.position - walkPoint;

        if (distanceToWalk.magnitude < 1f) walkPointSet = false;
    }

    private void FindWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, isGround)) walkPointSet = true;
        CancelInvoke("WalkPointReset");
        Invoke(nameof(WalkPointReset), 5f);
    }
    private void ChasePlayer()
    {
        t200.isStopped = false;
        Vector3 playerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        t200.SetDestination(playerPos);
        transform.LookAt(playerPos, transform.up);
    }
    private void AttackPlayer()
    {
        t200.isStopped = true;
        walkPointSet = false;

        if (!alreadyAttacked)
        {
            Vector3 playerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(playerPos);
            GetComponents<AudioSource>()[0].Play();
            t200Anim.SetBool("golpe", false);
            t200Anim.SetBool("golpe", true);
            Invoke(nameof(ColliderEnable), meleeColliderStart);
            alreadyAttacked = true;
            Invoke(nameof(AttackReset), timeBetweenAttacks);
        }
    }

    public void RecibeDamage(int damage)
    {
        health -= damage;
    }
    private void AttackReset()
    {
        alreadyAttacked = false;
        t200Anim.SetBool("golpe", false);
    }

    public void Stun()
    {
        //Debug.Log("t200 stunned");
        stunned = true;
        t200.isStopped = true;
        stunMoment = Time.realtimeSinceStartup;
        Invoke(nameof(Revive), reviveTime);
        AudioSource sonido = GetComponents<AudioSource>()[3];
        sonido.enabled = true;
    }

    private void Revive()
    {
        stunned = false;
        t200Anim.SetBool("stunned", false);
        AudioSource sonido = GetComponents<AudioSource>()[3];
        sonido.enabled = false;
        maxHealth += 15;
        health = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee" && stunned)
        {
            //Debug.Log("MORIDO");
            if (Time.realtimeSinceStartup - stunMoment <= reviveTime / 4)
            {
                player.gameObject.GetComponent<player_combat>().healPlayer(2);
            }
            else if (Time.realtimeSinceStartup - stunMoment <= reviveTime / 2)
            {
                player.gameObject.GetComponent<player_combat>().healPlayer(1);
            }

            if (altar != null)
            {
                altar.GetComponent<altar_nv2>().enemyKilled();
            }
            else
            {
                InfiniteSoulsManager.EnemyDead();
            }
            this.gameObject.SetActive(false);
        }
        else if (other.tag == "Projectile" && !stunned)
        {
            health -= 10;
        }
    }

    public void Respawn()
    {
        maxHealth = originalHealth;
        health = maxHealth;
        stunned = false;
    }

    public void ColliderEnable()
    {
        GetComponents<BoxCollider>()[1].enabled = true;
        Invoke(nameof(ColliderReset), meleeColliderDuration);
    }
    public void ColliderReset()
    {
        GetComponents<BoxCollider>()[1].enabled = false;
    }

    public void WalkPointReset()
    {
        walkPointSet = false;
    }

    public void OutOfRange()
    {
        WasStopped = t200.isStopped;
        t200.isStopped = true;
    }

    public void InOfRange()
    {
        t200.isStopped = WasStopped;
    }
}