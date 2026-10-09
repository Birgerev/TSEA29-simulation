using UnityEngine;

public class Styrmodul : MonoBehaviour
{
	public float targetAngle = 0;
	public float currentAngle = 0;
	
	[Space]

	public float targetSpeed;

	[Space]
	public DCMotor leftMotor;
	public DCMotor rightMotor;

    // Update is called once per frame
    void Update()
    {
        //TODO PID

		// Just testing forward
		SetMotorPower(targetSpeed, targetSpeed);
    }

	private void SetMotorPower(float leftPower, float rightPower)
	{
		leftMotor.DIR = leftPower > 0;
		rightMotor.DIR = rightPower > 0;

		leftMotor.duty = Mathf.Clamp01(Mathf.Abs(leftPower));
		rightMotor.duty = Mathf.Clamp01(Mathf.Abs(rightPower));
	}
}
