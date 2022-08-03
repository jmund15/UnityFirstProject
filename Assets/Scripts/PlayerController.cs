using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum powerups
{
    NOPOWERUP,
    KNOCKBACK,
    JUMP,
    POWERUP3, // missles
    POWERUP4, // earthquake attack
    POWERUP5  // spawns mini allies (bombs, turrets, etc...)
};

/*Enemy ideas!
 * Enemy that can shoot homing projectiles at you! (boss?) (stationary turret enemy?)
 * Enemy that flys up before shooting down at you (like a vulture)
 * swirly enemy?
 * pin roller type enemy?
 * mortar enemy
 * enemy that has to be jumped on?
 * enemy that runs from player when they get to close (go to a safe location)
 */


public class PlayerController : MonoBehaviour
{
    powerups playerPup;

    public Transform moveAround;
    public Vector3 moveDirection;

    public GameObject indicator;

    private Rigidbody playerRb;

    public ParticleSystem boostAnim;

    Coroutine pupCooldown = null;

    public bool gameOver;

    public float speed;
    public float airMobility; // 1 is normal mobility 
    public float knockBack;

    public float boostCooldown;
    private bool canBoost = false;

    public Vector3 velocity;

    private float horizInput;
    private float vertInput;

    public float kbStrength;
    public float kbCooldown;

    public float jumpStrength;
    public float jumpCooldown;

    private Color baseColor;
    private Color flashColor;

    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        playerPup = powerups.NOPOWERUP; // start game with no powerup

        baseColor = gameObject.GetComponent<Renderer>().material.color;
        playerRb = GetComponent<Rigidbody>();
        indicator.gameObject.SetActive(false);
        indicator.gameObject.transform.position = transform.position + new Vector3(0, -0.5f, 0);
        StartCoroutine(BoostCooldown());
        StartCoroutine(BoostReadyFlash());
    }

    void Update()
    {
        if (transform.position.y < -5)
        {
            gameOver = true;
        }
        if (playerPup != powerups.NOPOWERUP)
        {
            indicator.gameObject.transform.position = transform.position + new Vector3(0, -0.5f, 0);
            if (playerPup == powerups.JUMP && Input.GetKeyDown(KeyCode.Space) && grounded)
            {
                playerRb.AddForce((Vector3.up + (moveDirection.normalized * 0.333f)) * jumpStrength, ForceMode.Impulse); // have jump be influenced by player input as well
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        horizInput = Input.GetAxis("HorizMove");
        vertInput = Input.GetAxis("VertMove");
        Vector3 moveForward = moveAround.forward * vertInput;
        Vector3 moveRight = moveAround.right * horizInput;
        moveDirection = moveForward + moveRight;
        if (Physics.Raycast(transform.position, Vector3.down, 1.0f))
        { // make sure player is on ground
            playerRb.AddForce(speed * moveDirection);
            //Debug.Log("GROUNDED!!");
            if (!grounded) { grounded = true; }
        }
        else
        {
            playerRb.AddForce(speed * airMobility * moveDirection);
            //Debug.Log("not grounded!!");
            if (grounded) { grounded = false; }
        }

        velocity = playerRb.velocity;
    }

    IEnumerator BoostCooldown()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(boostCooldown);
            canBoost = true;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.LeftShift));
            boostAnim.Stop();
            boostAnim.Play();
            canBoost = false;
            playerRb.AddForce(new Vector3(horizInput, 0, vertInput) * speed, ForceMode.Impulse);
        }
    }
    IEnumerator BoostReadyFlash()
    {
        while (!gameOver)
        {
            yield return new WaitUntil(() => canBoost);
            gameObject.GetComponent<Renderer>().material.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            gameObject.GetComponent<Renderer>().material.color = baseColor;
            yield return new WaitForSeconds(1.0f);
        }
    }
    IEnumerator PowerupCountdownRoutine()
    {
        indicator.gameObject.SetActive(true);
        switch (playerPup)
        {
            case powerups.KNOCKBACK:
                indicator.GetComponent<Renderer>().material.color = Color.red;
                yield return new WaitForSeconds(kbCooldown);
                break;
            case powerups.JUMP:
                indicator.GetComponent<Renderer>().material.color = Color.blue;
                yield return new WaitForSeconds(jumpCooldown);
                break;
            default:
                yield return new WaitForSeconds(7);
                break;
        }
        playerPup = powerups.NOPOWERUP;
        indicator.gameObject.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            if (playerPup != powerups.NOPOWERUP)
            {
                StopCoroutine(pupCooldown); // reset cooldown of previous powerup so we can start this new one
            }
            playerPup = other.GetComponent<PowerupSpin>().powerupType; //assign which powerup the player has
            Destroy(other.gameObject);
            pupCooldown = StartCoroutine(PowerupCountdownRoutine());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody emBody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 opDirection = (collision.gameObject.transform.position - transform.position);
            if (playerPup == powerups.KNOCKBACK)
            {
                emBody.AddForce(opDirection * kbStrength, ForceMode.Impulse);
            }
            else
            {
                emBody.AddForce(opDirection * knockBack, ForceMode.Impulse); // natural knockback from player
            }
            //Debug.Log("Collided with " + collision.gameObject.name + "with powerup set to " + hasPowerup);
        }
    }
}
