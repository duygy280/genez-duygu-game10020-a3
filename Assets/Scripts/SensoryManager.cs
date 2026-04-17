using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensoryManager : MonoBehaviour
{
    public StateMachineSimple npc;
    public GameObject soundObjects;
    private void Awake()
    {
        foreach (Transform child in soundObjects.transform)
        {
            SoundObject soundObject = child.GetComponent<SoundObject>();
            if (soundObject != null)
            {
                soundObject.OnSoundTriggered.AddListener(npc.SoundTrigger);
            }
        }
    }
}
