using UnityEngine;
using FishNet;
using FishNet.Object;

public class GlueTrap : NetworkBehaviour
{
    [SerializeField] float speedModifier = 0.25f;
    [SerializeField] float jumpModifier = 0.05f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Frog"))
        {
            FrogMotor motor = other.GetComponent<FrogMotor>();
            if (motor != null)
            {
                motor.Tuning.moveSpeed = motor.Tuning.moveSpeed * speedModifier;
                motor.Tuning.minJumpHeight = motor.Tuning.minJumpHeight * jumpModifier;
                motor.Tuning.maxJumpHeight = motor.Tuning.maxJumpHeight * jumpModifier;
                motor.Tuning.minJumpDistance = motor.Tuning.minJumpDistance * jumpModifier;
                motor.Tuning.maxJumpDistance = motor.Tuning.maxJumpDistance * jumpModifier;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Frog"))
        {
            FrogMotor motor = other.GetComponent<FrogMotor>();
            if (motor != null)
            {
                motor.Tuning.moveSpeed = motor.Tuning.moveSpeed / speedModifier;
                motor.Tuning.minJumpHeight = motor.Tuning.minJumpHeight / jumpModifier;
                motor.Tuning.maxJumpHeight = motor.Tuning.maxJumpHeight / jumpModifier;
                motor.Tuning.minJumpDistance = motor.Tuning.minJumpDistance / jumpModifier;
                motor.Tuning.maxJumpDistance = motor.Tuning.maxJumpDistance / jumpModifier;
            }
        }
    }
}
