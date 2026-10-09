using UnityEngine;

public class Kommunikationsmodul : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var styrmodul = GetComponent<Styrmodul>();
		
		// Update current state
		styrmodul.currentAngle = transform.eulerAngles.y;
		
		// Update target values
		styrmodul.targetAngle = 90; // TODO try alternating
		// styrmodul.targetSpeed = 0; //TODO testing
    }
}
