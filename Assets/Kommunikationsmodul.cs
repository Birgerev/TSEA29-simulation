using UnityEngine;

public class Kommunikationsmodul : MonoBehaviour
{
	public WallSensor wallSensor;
	public float wallStopDistance = 0.3f;
	public float targetAngle;
	public float targetSpeed;
	public float turnDuration = 1;

	private float _startTurnTime;
    // Update is called once per frame
    void Update()
    {
		// Turn for turnDuration seconds
		bool turning = Time.time - _startTurnTime < turnDuration;

		// Start a turn when we are close to a wall
		if(!turning && wallSensor.FrontDistance() < wallStopDistance)
		{
			print(wallSensor.FrontDistance() );
			_startTurnTime = Time.time;

			targetAngle -= 90;
			if (targetAngle < -180)
				targetAngle += 360;
		}

		// Stand still while turning
		targetSpeed = turning ? 0 : 1;

		// Send target & current values to styrmodul
        var styrmodul = GetComponent<Styrmodul>();
		styrmodul.UpdateValues(targetAngle, transform.eulerAngles.y, targetSpeed);
	}
}
