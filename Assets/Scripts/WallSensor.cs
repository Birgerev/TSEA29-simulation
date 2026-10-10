using UnityEngine;

public class WallSensor : MonoBehaviour
{
	public float sensorRange = 0.8f;   // m, match the real sensor
	public LayerMask wallMask = ~0;    // everything by default; put walls on their own layer to be safe

	public float FrontDistance()
	{
		Vector3 origin = transform.position + transform.forward;

		if (Physics.Raycast(new Ray(origin, transform.forward), out var wallHit, sensorRange, wallMask))
			return wallHit.distance;    // meters from the front edge to the wall

		return sensorRange;             // nothing in range: report max, like a real sensor would
	}
}
