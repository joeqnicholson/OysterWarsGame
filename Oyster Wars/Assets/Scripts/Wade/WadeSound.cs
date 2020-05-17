using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WadeSound : MonoBehaviour
{

    public AudioSource SoundSource;
    public AudioClip pearlPickup;
    public AudioClip rifleShot;
    public AudioClip footStep;
    public AudioClip swoosh;
    public AudioClip jump;
    public AudioClip hit;


    private void Start()
    {

    }

    public void PlayPearlPickup()
    {
        SoundSource.PlayOneShot(pearlPickup, 0.5f);
    }

    public void PlaySwoosh()
    {
        SoundSource.PlayOneShot(swoosh, 0.4f);
    }

    public void PlayRifleShot()
    {
        SoundSource.PlayOneShot(rifleShot, 0.2f);
    }

    public void PlayFootSteps()
    {
        SoundSource.PlayOneShot(footStep, 0.2f);
    }

    public void PlayJump()
    {
        SoundSource.PlayOneShot(jump, 0.2f);
    }
    public void PlayHit()
    {
        SoundSource.PlayOneShot(hit, 0.2f);
    }
}
