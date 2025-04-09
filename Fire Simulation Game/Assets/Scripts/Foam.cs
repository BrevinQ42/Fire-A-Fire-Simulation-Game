using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foam : FireFightingObject
{
    public string type;
    [SerializeField] private float affectFireMult;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionStay(Collision collision)
    {
        Fire fire = collision.collider.GetComponent<Fire>(); 
        if (fire)
        {
            if (fire.EffectivityTable[fire.type].Equals(type))
                fire.AffectFire(-fireFightingValue * affectFireMult);
            else
                fire.AffectFire(fireFightingValue * affectFireMult);
        }
    }
}
