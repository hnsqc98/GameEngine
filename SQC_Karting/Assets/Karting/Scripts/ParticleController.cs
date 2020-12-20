using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem PA1,PB1,PC1,PD1,PE1,PF1,PA2,PB2,PC2,PD2,PE2,PF2,SMOKE;
    // Start is called before the first frame update
    void Start()
    {
        PA1.Play();
        PB1.Play();
        PC1.Play();
        PD1.Play();
        PE1.Play();
        PF1.Play();
        PA2.Play();
        PB2.Play();
        PC2.Play();
        PD2.Play();
        PE2.Play();
        PF2.Play();
        SMOKE.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
