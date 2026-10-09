using UnityEngine;

[System.Serializable]
public class TargetAnglePID : PID
{
	[Tooltip("How much of each new rate sample to take")]
	public float rateFilter = 0.3f;
	
	private float _prevAngle;
	private float _rate;
	bool _hasPrev;

	public TargetAnglePID(float kp, float ki, float kd, float rateFilter) : base(kp, ki, kd) {
		this.rateFilter = rateFilter;
	}

    public float UpdateAngle(float targetAngle, float currentAngle, float dt)
    {
		float error = WrapAngle(targetAngle - currentAngle);

		// Rate from the change in the measured angle (not the error), so moving the target doesn't kick D
		float rawRate = _hasPrev ? WrapAngle(currentAngle - _prevAngle) / dt : 0f;
		_prevAngle = currentAngle;
		_hasPrev = true;

		// Low-pass: move part of the way toward the new sample each step
		_rate += rateFilter * (rawRate - _rate);

		return Step(error, _rate, dt);
	}

	static float WrapAngle(float a)
	{
		while (a > 180f) a -= 360f;
		while (a < -180f) a += 360f;
		return a;
	}
}
