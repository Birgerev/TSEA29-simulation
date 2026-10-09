using Unity.Collections;
using UnityEngine;

public class Styrmodul : MonoBehaviour
{
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
        //TODO PID

		// Just testing forward
		SetMotorPower(_targetSpeed, _targetSpeed);
    }

	private void SetMotorPower(float leftPower, float rightPower)
	{
		leftMotor.DIR = leftPower > 0;
		rightMotor.DIR = rightPower > 0;

		leftMotor.duty = Mathf.Clamp01(Mathf.Abs(leftPower));
		rightMotor.duty = Mathf.Clamp01(Mathf.Abs(rightPower));
	}
}
