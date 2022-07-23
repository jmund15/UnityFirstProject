using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour
{
    public Rigidbody rb;
    public PlayerController player;

    protected float baseSpeed = 30.0f;

    protected Vector3 playerPos;
    protected Vector3 lookDirection;
    protected float distFromPlayer;
    protected float distFromZEdge;
    protected float distFromXEdge;
    protected float playerDistFromZEdge;
    protected float playerDistFromXEdge;


    public int enemyCost;
    public int unlockRound;
    public bool soloOnUnlock;
    public int obsoleteRound;

    public float trackingStrength; // higher the number, stricter the tracking (base tracking = 1)
    public float trackingVar; // randomness generated when tracking (0 = no randomness)
    protected float trackPull = 0.0f;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (transform.position.y < -5)
        {
            Destroy(gameObject);
        }
        playerPos = player.transform.position;

        lookDirection = playerPos - transform.position;
        distFromPlayer = Mathf.Sqrt(Mathf.Pow(player.transform.position.x - transform.position.x, 2) + Mathf.Pow(player.transform.position.z - transform.position.z, 2));
        distFromZEdge = Mathf.Abs(Mathf.Abs(rb.position.z + rb.velocity.z) - 12.25f);
        distFromXEdge = Mathf.Abs(Mathf.Abs(rb.position.x + rb.velocity.x) + Mathf.Abs(rb.position.z + rb.velocity.z) / 1.82f - 14);

        playerDistFromZEdge = Mathf.Abs(Mathf.Abs(playerPos.z + player.GetComponent<Rigidbody>().velocity.z) - 12.25f);
        playerDistFromXEdge = Mathf.Abs(Mathf.Abs(playerPos.x + player.GetComponent<Rigidbody>().velocity.x) + Mathf.Abs(playerPos.z + player.GetComponent<Rigidbody>().velocity.z) / 1.82f - 14);
    }

    protected virtual void baseMovement() // handles all basic enemy movement
    {
        rb.AddForce(lookDirection.normalized * baseSpeed, ForceMode.Force);
    }

    protected virtual void trackingForce(float pull, ForceMode forceMode = ForceMode.Force) // higher the number, stricter the tracking (base tracking = 1)
    {
        Vector3 trackingVec = adjVector(rb.velocity, lookDirection);
        
        if (trackingVec != Vector3.zero)
        {
            rb.AddForce(trackingVec.normalized * baseSpeed * pull, forceMode); // add better player tracking

            // SUDDEN ATTACK
            //if (distFromPlayer < 3)
            //{
            //    rb.AddForce(trackingVec * baseSpeed * (1 / distFromPlayer) * sudAtkMod, forceMode); // add a bit of better player tracking (DON'T NORMALIZE, distFromPlayer in force might not be necessary)
            //}
        }
    }

    protected virtual void colKnockBack(GameObject aggressor, GameObject submisser, float power) // Enemy knocks player back a little bit
    {
        Rigidbody flyer = submisser.GetComponent<Rigidbody>();
        Vector3 opDirection = (submisser.transform.position - aggressor.transform.position);

        flyer.AddForce(opDirection * power, ForceMode.Impulse);
    }

    protected virtual void explosion(float power, float radius, float upwardsForce = 3.0f) // Enemy explodes, knocking back all objects in radius
    {
        Vector3 explosionPos = transform.position;

        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

        foreach (Collider dead in colliders)
        {
            Rigidbody exRb = dead.GetComponent<Rigidbody>();
            if (exRb)
            {
                exRb.AddExplosionForce(power, explosionPos, radius, upwardsForce);
            }
        }
    }

    protected virtual IEnumerator edgeAvoidance(float power, float dist, float cooldown)
    {
        while (gameObject)
        {
            yield return new WaitForSeconds(cooldown);
            yield return new WaitUntil(() => (distFromXEdge < dist || distFromZEdge < dist));
            rb.AddForce(baseSpeed * power * (-transform.position.normalized), ForceMode.Impulse); // go towards middle
        }
        
    }

    /* This function periodically generates an impulse on the enemy towards the player 
     * Params -
     *      power: power of impulse (mult mod)
     *      cooldown: base cooldown of impulse
     *      cdVar: random variance of cooldown (param should be positive)
     *      velTrackLow: lowest possible value for impulse tracking player velocity
     *      velTrackHigh: highest possible value for impulse tracking player velocity
     *          (A 1.0 value is the base tracking value, if velocity is unchanging, 
     *          then impulse will accurately track player)
     */
    protected virtual IEnumerator impulse(float power, float cooldown, float cdVar = 0.0f, float velTrackLow = 0.0f, float velTrackHigh = 0.0f)
    {

        while (gameObject)
        {
            yield return new WaitForSeconds(Random.Range(cooldown - cdVar, cooldown + cdVar));
            Vector3 forceMod = player.velocity * Random.Range(velTrackLow, velTrackHigh);

            /* 1. -rb.velocity: neutralize current enemy velocity to get a accurate boost
             * 2. lookDirection: boost towards player
             * 3. forceMod: boost accounting for current player velocity
             */
            rb.AddForce( (-rb.velocity) + ((lookDirection + forceMod).normalized * baseSpeed * power), ForceMode.Impulse);

            //Debug.Log((-rb.velocity) + ((lookDirection + forceMod).normalized * baseSpeed * power));
        }        
    }

    protected virtual IEnumerator adjRandomMod(float randLow, float randHigh, float timeSet, System.Action<float> setVal) //set
    {
        while (gameObject)
        {
            setVal(Random.Range(randLow, randHigh));
            yield return new WaitForSeconds(Random.Range(timeSet - 1.0f, timeSet + 1.0f));
        }
        
    }

    protected virtual IEnumerator bossSpawn(GameObject[] spawns, float spawnCooldown, int pointsPerSpawn = 0)
    {
        while (gameObject)
        {
            yield return new WaitForSeconds(spawnCooldown);
            if (pointsPerSpawn == 0)
            {
                GameObject spawn = spawns[Random.Range(0, spawns.Length)];
                Instantiate(spawn, new Vector3(Random.Range(transform.position.x - 5, transform.position.x + 5), 1, Random.Range(transform.position.z - 5, transform.position.z + 5)), spawn.transform.rotation);
            }
        }
    }

    /* This function projects the vector 'heading' onto the vector 'desiredHeading'
     * Then it uses the projection to generate a vector, that, adding a force to this new vector and towards 'desiredHeading', will track the desired object
     * Returns - A vector orthogonal to 'desiredHeading' and connecting to the projection
     */
    protected Vector3 adjVector(Vector3 heading, Vector3 desiredHeading)
    {
        Vector2 x = new Vector2(heading.x, heading.z);
        Vector2 y = new Vector2(desiredHeading.x, desiredHeading.z);

        float scalProj = Vector3.Dot(x, y) / y.magnitude;
        Vector2 vectorProj = (scalProj * (1 / y.magnitude)) * y;
        Vector2 trackingDir = vectorProj - x; // this all acounts for current velocity in relation to direction to player, adding force perpindicular to this vector to track the player better

        //Debug.Log(trackingDir);
        return new Vector3(trackingDir.x, 0, trackingDir.y);
    } 
    //protected virtual IEnumerator impulseReset()
    //{
    //    yield return new WaitForSeconds(Random.Range(impulseCooldown - 2.0f, impulseCooldown + 2.0f));
    //    impulseUsed = false;
    //}

    //protected virtual IEnumerator enemyMove()
    //{
    //    while (true)
    //    {


    //        //if (distFromXEdge < 1 || distFromZEdge < 1.5f) {
    //        //    returnToSafety = true;
    //        //    safetyCounter = 0;
    //        //}


    //        if (!impulseUsed)
    //        {
    //            rb.AddForce(((player.transform.position - transform.position) + (player.velocity * Random.Range(0.75f, 1.25f)) - enemyRb.velocity).normalized * baseSpeed * 2, ForceMode.Impulse);
    //            impulseUsed = true;
    //            StartCoroutine(impulseReset());
    //            yield return new WaitForSeconds(0.5f);
    //        }
    //        else if (returnToSafety)
    //        {
    //            safetyCounter += Time.fixedDeltaTime / safetyCooldown;
    //            rb.AddForce(((-transform.position * 2) + player.transform.position).normalized * baseSpeed * 1.5f); // go towards middle
    //            if (safetyCounter >= 1.0)
    //            {
    //                returnToSafety = false;
    //            }
    //            yield return new WaitForFixedUpdate();
    //        }
    //        //else if (Random.Range(0.0f, 100f) < 0.1f)
    //        //{

    //        //}
    //        else
    //        {
    //            safetyCounter = 0;
    //            yield return new WaitForFixedUpdate();
    //        }
    //    }

    //}
}
