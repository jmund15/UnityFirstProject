using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyImpulse : Enemy
{
    public float massMod;
    public float speedMod;
    public float sizeScale;

    public float impulsePower = 10.0f;
    public float impulseCooldown = 6.0f;
    public float cooldownVar = 2.0f;
    public float boostAccuracyLow = 0.0f;
    public float boostAccuracyHigh = 0.0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.mass += massMod;
        baseSpeed += speedMod;
        StartCoroutine(impulse(impulsePower, impulseCooldown, cooldownVar, boostAccuracyLow, boostAccuracyHigh));
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

}
