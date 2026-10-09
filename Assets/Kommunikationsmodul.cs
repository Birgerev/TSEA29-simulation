using UnityEngine;

public class Kommunikationsmodul : MonoBehaviour
{
	public float targetAngle;
	public float targetSpeed;

    // Update is called once per frame
    void Update()
    {
        var styrmodul = GetComponent<Styrmodul>();
		
		styrmodul.UpdateValues(targetAngle, transform.eulerAngles.y, targetSpeed);
	}
}
