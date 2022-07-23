using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBomb : Enemy
{
    public float massMod;
    public float speedMod;

    public float sizeScale;
    public float explosiveness;
    public float explosionRadius;
    public float upwardsForce;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.mass += massMod;
        baseSpeed += speedMod;
        transform.localScale *= sizeScale;
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
            explosion(explosiveness, explosionRadius, upwardsForce);
            Destroy(gameObject);
        }
    }
}
