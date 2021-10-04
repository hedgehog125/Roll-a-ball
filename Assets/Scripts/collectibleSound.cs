using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectibleSound : MonoBehaviour
{
    public AudioSource collectSound;

    void OnDisable()
    {
        collectSound.Play();
    }
}
