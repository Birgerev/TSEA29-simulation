// Plain C# on purpose: no UnityEngine types, so it ports line-by-line to C on the robot.
[System.Serializable]
public class PID
{
    public float Kp, Ki, Kd;
    public float outputMin = -1f, outputMax = 1f;
    public float iTermLimit = 0.3f;     // max contribution from the I-term, in output units

    float iTerm;

    public PID(float kp, float ki, float kd)
    {
        Kp = kp; Ki = ki; Kd = kd;
    }
    // error: setpoint - measurement
    // rate:  derivative of the measurement (e.g. gyro rate), so D needs no differentiation
    public float Step(float error, float rate, float dt)
    {
        // Accumulating Ki * error (instead of error alone) means changing Ki live doesn't cause a jump
        iTerm += Ki * error * dt;
        iTerm = Clamp(iTerm, -iTermLimit, iTermLimit);

        float output = Kp * error + iTerm - Kd * rate;
        return Clamp(output, outputMin, outputMax);
    }

    public void Reset()
    {
        iTerm = 0f;
    }

    static float Clamp(float x, float lo, float hi)
    {
        return x < lo ? lo : (x > hi ? hi : x);
    }
}
