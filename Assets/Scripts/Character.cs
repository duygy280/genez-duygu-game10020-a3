using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    CharacterController controller;
    PlayerInput playerInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        PlayerMotion();
    }

    void PlayerMotion()
    {
        Vector2 moveDirection = playerInput.currentActionMap["Move"].ReadValue<Vector2>();

        Vector3 move = new Vector3(moveDirection.x, 0, moveDirection.y);

        controller.Move(move * moveSpeed * Time.deltaTime);
        controller.Move(Vector3.down * 2f * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.deltaTime
            );
        }
    }

}
