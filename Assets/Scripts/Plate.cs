using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Plate : SoundObject
{
    public Material PlateMaterial;
    public Material PlateMaterialPressed;

    MeshRenderer meshRenderer;

    public override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        meshRenderer.material = PlateMaterialPressed;
    }
    public void OnTriggerExit(Collider other)
    {
        meshRenderer.material = PlateMaterial;
    }
}
