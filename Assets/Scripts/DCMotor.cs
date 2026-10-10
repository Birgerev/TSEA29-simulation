using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Header("Gizmos")]
    [Tooltip("Arrow length in meters per newton of force. At 0.05 a 3 N force draws as a 15 cm arrow.")]
    public float gizmoScale = 0.05f;

    Rigidbody rb;
    Vector3 lastDriveForce;
    Vector3 lastGripForce;

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

        lastDriveForce = transform.forward * drive;
        lastGripForce = transform.right * grip;

        rb.AddForceAtPosition(lastDriveForce + lastGripForce, transform.position);
    }

    void OnDrawGizmos()
    {
        Vector3 p = transform.position;

        DrawArrow(p, lastDriveForce * gizmoScale, Color.green);                    // drive
        DrawArrow(p, lastGripForce * gizmoScale, Color.red);                       // sideways grip
        DrawArrow(p, (lastDriveForce + lastGripForce) * gizmoScale, Color.yellow); // total

#if UNITY_EDITOR
        Handles.Label(p + Vector3.up * 0.1f, $"DIR: {(DIR ? 1 : -1)}\nduty: {duty:F2}");
#endif
    }

    static void DrawArrow(Vector3 from, Vector3 vec, Color color)
    {
        if (vec.sqrMagnitude < 1e-6f) return;

        Gizmos.color = color;
        Vector3 tip = from + vec;
        Gizmos.DrawLine(from, tip);

        float head = Mathf.Min(0.03f, vec.magnitude * 0.3f);
        Quaternion look = Quaternion.LookRotation(vec);
        Gizmos.DrawLine(tip, tip + look * Quaternion.Euler(0, 155, 0) * Vector3.forward * head);
        Gizmos.DrawLine(tip, tip + look * Quaternion.Euler(0, -155, 0) * Vector3.forward * head);
    }
}
