using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class t800_soul : MonoBehaviour
{

    [Header("Pause Menu")]
    pause_menu pauseMenu;

    public NavMeshAgent t800;
    Animator t800Anim;

    public Transform player;

    public GameObject altar;

    public LayerMask isGround, isPlayer, isGroundnotfrfr;
    public RaycastHit rayHit;

    private bool WasStopped;

    //Idle-Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public float accuracyOffsetGrounded, accuracyOffsetAirborne;
    public GameObject muzzleFlash;

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
        t800 = GetComponent<NavMeshAgent>();
        t800Anim = GetComponent<Animator>();
        InfiniteSoulsManager = FindObjectOfType<InfiniteSoulsManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu = FindObjectOfType<pause_menu>();
        health = maxHealth;
        originalHealth = maxHealth;
        stunned = false;
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

        if (!playerInSightRange && !playerInAttackRange && health>0)
        { 
            Patroling(); 
            t800Anim.SetBool("isWalkin", true);
            t800Anim.SetBool("isStunned", false);
            AudioSource sonido = GetComponents<AudioSource>()[3];
            sonido.enabled = true;
        }
        else if (!playerInAttackRange && health > 0) 
        { 
            ChasePlayer(); 
            t800Anim.SetBool("isWalkin", true);
            t800Anim.SetBool("isStunned", false);
            AudioSource sonido = GetComponents<AudioSource>()[3];
            sonido.enabled = true;
        }
        else if (health > 0)
        { 
            AttackPlayer(); 
            //t800Anim.SetBool("isWalkin", true);
            t800Anim.SetBool("isStunned", false);
            AudioSource sonido = GetComponents<AudioSource>()[3];
            sonido.enabled = !t800.isStopped;
        }
        else if (!stunned)
        { 
            Stun(); 
            t800Anim.SetBool("isStunned", true);
            t800Anim.SetBool("isWalkin", false);
            AudioSource sonido = GetComponents<AudioSource>()[3];
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
            t800.isStopped = false;
            t800.SetDestination(walkPoint); 
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
        t800.isStopped = false;
        t800.SetDestination(new Vector3(player.position.x, transform.position.y, player.position.z));
    }
    private void AttackPlayer()
    {
        Vector3 playerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        t800.isStopped = Vector3.Distance(transform.position, playerPos) < 4;
        t800Anim.SetBool("isWalkin", !t800.isStopped);
        t800.SetDestination(new Vector3(player.position.x, transform.position.y, player.position.z));
        walkPointSet = false;
        transform.LookAt(playerPos);

        if (!alreadyAttacked)
        {
            //Coigo para el ataque (sera similar al de disparar)
            GetComponents<AudioSource>()[0].Play();
            muzzleFlash.SetActive(true);
            Invoke(nameof(MuzzleFlashReset), 0.5f);

            //Vector3 offsetPlayerPos = new Vector3(player.position.x + Random.Range(0f, accuracyOffset), player.position.y + Random.Range(0f, accuracyOffset), player.position.z + Random.Range(0f, accuracyOffset));

            // Obtener la velocidad del jugador
            float playerSpeed = player.GetComponent<Rigidbody>().velocity.magnitude;

            // Interpolar el offset de 0 (cuando la velocidad es 15 o menor) a un máximo (cuando la velocidad es 100 o mayor)
            float accuracyFactor = Mathf.InverseLerp(5f, 35f, playerSpeed); // 0 cuando velocidad ≤ 15, 1 cuando velocidad ≥ 100
            float currentOffset = accuracyFactor * (player.GetComponent<player_movement>().IsPlayerGrounded() ? accuracyOffsetGrounded : accuracyOffsetAirborne); // Escalamos el offset

            // Calcular la posición con el offset dependiente de la velocidad del jugador
            Vector3 offsetPlayerPos = new Vector3(
                player.position.x + Random.Range(-currentOffset, currentOffset),
                player.position.y + Random.Range(-currentOffset, currentOffset),
                player.position.z + Random.Range(-currentOffset, currentOffset)
            );

            if (Physics.Raycast(transform.position, offsetPlayerPos - transform.position, out rayHit, attackRange, isPlayer + isGround + isGroundnotfrfr))
            {
                if (rayHit.collider.CompareTag("Player"))
                {
                    rayHit.collider.GetComponent<player_combat>().DamagePlayer();
                }
            }

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
    }
    private void MuzzleFlashReset()
    {
        muzzleFlash.SetActive(false);
    }

    public void Stun()
    {
        //Debug.Log("T800 stunned");
        stunned = true;
        t800.isStopped = true;
        stunMoment = Time.realtimeSinceStartup;
        Invoke(nameof(Revive), reviveTime);
        AudioSource sonido = GetComponents<AudioSource>()[1];
        sonido.enabled = true;
    }

    private void Revive()
    {
        stunned = false;
        t800Anim.SetBool("isStunned", false);
        AudioSource sonido = GetComponents<AudioSource>()[1];
        sonido.enabled = false;
        maxHealth += 15;
        health = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee" && stunned)
        {
            //Debug.Log("MORIDO");
            if (Time.realtimeSinceStartup - stunMoment <= reviveTime/4)
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
        else if(other.tag == "Projectile" && !stunned)
        {
            health -= 10;
        }
    }

    public void Respawn()
    {
        t800Anim.SetBool("isStunned", false);
        t800Anim.SetBool("isWalkin", false);
        maxHealth = originalHealth;
        health = maxHealth;
        stunned = false;
    }

    public void WalkPointReset()
    {
        walkPointSet = false;
    }

    public void OutOfRange()
    {
        WasStopped = t800.isStopped;
        t800.isStopped = true;
    }

    public void InOfRange()
    {
        t800.isStopped = WasStopped;
    }
}
