using Unity.Collections;
using UnityEngine;

public class Styrmodul : MonoBehaviour
{
	public TargetAnglePID targetAnglePID;
	public PIDVisualizer targetAnglePIDVisualizer;
	
	private float _targetAngle;
	private float _currentAngle;
	private float _targetSpeed;

	[Space]
	public DCMotor leftMotor;
	public DCMotor rightMotor;

	public void UpdateValues(float targetAngle, float currentAngle, float targetSpeed) 
	{
		_targetAngle = targetAngle;
		_currentAngle = currentAngle;
		_targetSpeed = targetSpeed;
	}

    // Update is called once per frame
    void Update()
    {
		// If there is any target speed, move, otherwise turn.
		if(Mathf.Abs(_targetSpeed) > 0.1f)
			MotorsMoving();
		else
			MotorsTurn();
    }

	private void MotorsMoving()
	{
		SetMotorPower(_targetSpeed, _targetSpeed);
	}

	private void MotorsTurn()
	{
		// How quick should we turn? Value between -1 & 1, decided by PID-agorithm
		float turnSpeed = targetAnglePID.StepAngle(_targetAngle, _currentAngle, Time.deltaTime);

		// Update visualizer
		targetAnglePIDVisualizer.Update(targetAnglePID);

		SetMotorPower(-turnSpeed, turnSpeed);
	}

	private void SetMotorPower(float leftPower, float rightPower)
	{
		leftMotor.DIR = leftPower > 0;
		rightMotor.DIR = rightPower > 0;

		leftMotor.duty = Mathf.Clamp01(Mathf.Abs(leftPower));
		rightMotor.duty = Mathf.Clamp01(Mathf.Abs(rightPower));
	}

	private void OnDrawGizmos()
	{
		targetAnglePIDVisualizer.DrawGizmos(transform.position + Vector3.up * .5f);
	}
}
