using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : Enemy
{
    public float massMod;
    public float speedMod;

    public float sizeScale;
    public float power;

    public float edgeAvoidPwr;
    public float edgeAvoidDist;
    public float edgeAvoidCooldown;

    public GameObject[] spawn;
    public float spawnCooldown;

    public float weakness;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.mass += massMod;
        baseSpeed += speedMod;
        transform.localScale *= sizeScale;
        StartCoroutine(bossSpawn(spawn, spawnCooldown));
        StartCoroutine(edgeAvoidance(edgeAvoidPwr, edgeAvoidDist, edgeAvoidCooldown));
        if (trackingVar != 0)
        {
            StartCoroutine(adjRandomMod(trackingStrength - trackingVar, trackingStrength + trackingVar, 0.5f, result => trackPull = result));
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected void FixedUpdate()
    {
        baseMovement();
        trackingForce(trackPull);
    }

    private void OnCollisionEnter(Collision collision) // Enemy knocks player back a little bit
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            colKnockBack(gameObject, collision.gameObject, power);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            colKnockBack(collision.gameObject, gameObject, weakness);
        }
    }
}
