using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float sensitivity;
    public InputActionAsset playerInputs;
    public InputAction moveAction;
    public InputAction moveUpAction;
    public InputAction moveDownAction;

    Vector2 moveVector2;

    void Start()
    {
        moveAction = playerInputs.FindAction("Move", false);
        moveDownAction = playerInputs.FindAction("MoveDown", false);
        moveUpAction = playerInputs.FindAction("MoveUp", false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if(moveAction.IsPressed())
        {
            transform.position += new Vector3(moveVector2.x*moveSpeed,0,moveVector2.y*moveSpeed);
        }
        if(moveUpAction.IsPressed())
        {
            if(transform.position.y > 25)
            {
                transform.position += new Vector3(0,-1*moveSpeed,0);
            }
        }
        if(moveDownAction.IsPressed())
        {
            if(transform.position.y < 500)
            {
                transform.position += new Vector3(0,1*moveSpeed,0);
            }
        }
    }

    void OnMove(InputValue movementValue)
    {
        moveVector2 = movementValue.Get<Vector2>();
    }

    void OnQuit(InputValue input)
    {
        //Debug line to test quit function in editor
        //UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Quit pressed");
        Application.Quit();
    }
}
