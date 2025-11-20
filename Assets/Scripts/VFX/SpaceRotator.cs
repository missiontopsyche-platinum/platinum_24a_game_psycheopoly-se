using System;
using UnityEngine;

/// <summary>
/// This literally is just to rotate the Space shader on a skybox. Really simple.
/// </summary>
public class SpaceRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.003f;

    private Skybox sbox;

    private void Start()
    {
        sbox = GetComponent<Skybox>();
    }

    // Update is called once per frame
    void Update()
    {
        var curRot = sbox.material.GetFloat("_Rotation");
        var newRot = curRot + (Time.deltaTime * rotationSpeed);
        sbox.material.SetFloat("_Rotation", newRot);
    }
}
