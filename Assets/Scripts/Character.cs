using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class Character : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;


    Dictionary<string, GameObject> enemies;

    public Animator characterAnimator;
    PlayerSound playerSound;

    private Camera cam;
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 45;

    [HideInInspector]
    public bool canMove = true;

    private float lastAttack = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerSound = GetComponent<PlayerSound>();
        cam = Camera.main;
        enemies = new Dictionary<string, GameObject>();
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && Input.GetMouseButton(1))
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // animation
        if (curSpeedX > 0)
        {
            if (isRunning)
            {
                characterAnimator.SetBool("IsWalking", true);
                characterAnimator.SetBool("IsRunning", true);
            }
            else
            {
                characterAnimator.SetBool("IsWalking", true);
                characterAnimator.SetBool("IsRunning", false);
            }
        }
        else
        {
            characterAnimator.SetBool("IsWalking", false);
            characterAnimator.SetBool("IsRunning", false);
        }

        // attack
        if (Input.GetButtonDown("Fire1"))
        {
            if (lastAttack < Time.time - 1f)
            {
                characterAnimator.SetTrigger("Attack");
                playerSound.playAttackSound();
                Attack();

                lastAttack = Time.time;
            }
        }
    }


    // attack range trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            enemies.Add(other.gameObject.name, other.gameObject);

            Debug.Log("Enter" + other.gameObject.name);

        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            enemies.Remove(other.gameObject.name);

            Debug.Log("Exit" + other.gameObject.name);
        }
    }

    void Attack()
    {
        foreach (KeyValuePair<string, GameObject> each in enemies)
        {
            string K = each.Key;
            GameObject g = each.Value;

            Debug.Log(K, g);
        }
    }

}