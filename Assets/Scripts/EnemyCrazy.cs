using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCrazy : Enemy
{

    public float massMod;
    public float speedMod;
    public float speedVar;

    public float sizeScale;

    public float boostPwr;
    public float boostCooldown;
    public float cdVariance;
    public float boostCraziness;

    public float power;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.mass += massMod;
        transform.localScale *= sizeScale;
        StartCoroutine(impulse(boostPwr, boostCooldown, cdVariance, -boostCraziness, boostCraziness));
        if (speedVar != 0)
        {
            StartCoroutine(adjRandomMod(speedMod, speedMod + speedVar, 1.0f, result => baseSpeed = result));
        }
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
        if (!collision.gameObject.CompareTag("Ground"))
         colKnockBack(gameObject, collision.gameObject, power); //knockback EVERYTHING
    }
}
