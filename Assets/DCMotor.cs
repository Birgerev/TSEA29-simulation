using UnityEngine;

public class DCMotor : MonoBehaviour
{
    [Header("Inputs (what the microcontroller drives)")]
    [Tooltip("Rotation direction. True drives forward along this transform's Z axis, false drives backward.")]
    public bool DIR;
    
	[Tooltip("PWM duty cycle: the fraction of each period the signal is high (0 = off, 1 = full). The motor responds to the average voltage, 7.2 V * duty.")]
    [Range(0, 1)] public float duty;

    [Header("Motor model")]
    [Tooltip("Force in N this side pushes with at standstill and 100% duty. Fades as the wheel approaches its target speed (back-EMF). Tune so the sim spins up as fast as the real robot.")]
    public float stallForce = 3f;          // N per side at standstill, 100% duty -- tune
    
	[Tooltip("Wheel speed in m/s at 100% duty with no load. 291 RPM on a 65 mm wheel is about 1 m/s. Steady-state speed is roughly duty * maxSpeed.")]
    public float maxSpeed = 1f;            // m/s at 100% duty (291 RPM, 65 mm wheel)
    
	[Tooltip("Duty below which the motor can't overcome friction and nothing moves. The duty range above it is stretched back over 0-1. Measure on the real robot by ramping duty up until it starts moving.")]
    [Range(0, 1)] public float deadband = 0.2f;

    [Header("Ground contact")]
    [Tooltip("Sideways force in N per m/s of sideways slip. Stops the robot sliding like it's on ice. Keep 2 * lateralGrip * fixedDeltaTime / mass well below 1, or the sideways motion oscillates.")]
    public float lateralGrip = 10f;        // N per m/s of sideways slip -- tune

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float d = Mathf.Clamp01(duty);

        // Below the deadband the motor can't overcome friction
        float effective = d < deadband ? 0f : (d - deadband) / (1f - deadband);
        float u = effective * (DIR ? 1f : -1f);

        Vector3 v = rb.GetPointVelocity(transform.position);
        float forwardSpeed = Vector3.Dot(v, transform.forward);
        float sidewaysSpeed = Vector3.Dot(v, transform.right);

        // Back-EMF: force fades as the wheel approaches the speed the duty asks for
        float drive = stallForce * (u - forwardSpeed / maxSpeed);
        float grip = -lateralGrip * sidewaysSpeed;

        rb.AddForceAtPosition(transform.forward * drive + transform.right * grip, transform.position);
    }
}
