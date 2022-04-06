using UnityEngine;
using HLRiptide;

public class PlayerController : NetworkedBehaviour
{
    public new Rigidbody rigidbody;
    public CapsuleCollider capsuleCollider;
    public LocalPlayerInputInfo localPlayerInputInfo;

    [Tooltip("Maximum slope the character can walk on")]
    [Range(5f, 60f)]
    public float slopeLimit = 45f;
    [Tooltip("Walk speed")]
    public float walkSpeed;
    [Tooltip("Sprint speed")]
    public float sprintSpeed;
    [Tooltip("Upward speed to apply when jumping")]
    public float jumpSpeed;
    [Tooltip("How much control the player has of their movement while in the air strafing")]
    public float strafeControl;

    private IState defaultState = new WalkingState();

    private IState playerState;

    public override void OnAwake()
    {
        walkSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;

        playerState = defaultState;
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayerWithAuthority) playerState = playerState.Update(this);
    }

    public void MoveForward(float speed)
    {
        rigidbody.AddForce(speed * transform.forward, ForceMode.Force);
    }

    public void MoveRight(float speed)
    {
        rigidbody.AddForce(speed * transform.right, ForceMode.Force);
    }

    public void MoveLeft(float speed)
    {
        rigidbody.AddForce(speed * -transform.right, ForceMode.Force);
    }

    public void MoveBackward(float speed)
    {
        rigidbody.AddForce(speed * -transform.forward, ForceMode.Force);
    }

    public void Jump(float speed)
    {
        rigidbody.AddForce(speed * transform.up, ForceMode.Impulse);
    }

    public override void OnRegisterCommands()
    {
    }
}