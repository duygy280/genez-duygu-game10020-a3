using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundObject : MonoBehaviour
{

    public UnityEvent<SoundObject> OnSoundTriggered = new UnityEvent<SoundObject>();
    AudioSource source;
    public virtual void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    public virtual void OnTriggerEnter(Collider other)
    {
       if (other.GetComponent<Character>() != null)
        {
            source.Play();
            OnSoundTriggered.Invoke(this);
        }
    }
}
