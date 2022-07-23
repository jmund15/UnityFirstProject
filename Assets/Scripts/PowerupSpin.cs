using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpin : MonoBehaviour
{
    public float spinSpeed;

    public powerups powerupType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * spinSpeed);
    }
}
