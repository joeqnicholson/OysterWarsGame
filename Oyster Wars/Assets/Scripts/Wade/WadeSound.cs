using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WadeSound : MonoBehaviour
{

    public AudioSource SoundSource;
    public AudioClip pearlPickup;
    public AudioClip rifleShot;
    public AudioClip footStep;

    private void Start()
    {

    }

    public void PlayPearlPickup()
    {
        SoundSource.PlayOneShot(pearlPickup, 0.5f);
    }

    public void PlayRifleShot()
    {
        SoundSource.PlayOneShot(rifleShot, 0.5f);
    }

    public void PlayFootSteps()
    {
        SoundSource.PlayOneShot(footStep);
    }
}
