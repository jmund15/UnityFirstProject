using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFast : Enemy
{

    public float massMod;
    public float speedMod;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.mass += massMod;
        baseSpeed += speedMod; // this enemy is faster than base enemy
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
