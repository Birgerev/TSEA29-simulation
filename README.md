# TSEA29 Robot Simulation

A Unity sandbox for prototyping the TSEA29 robot before touching real hardware.
Each simulated module maps 1-to-1 to a real module on the robot, so logic we
tune here (PID gains, state machines, thresholds) ports over with little
change.

## Structure mirrors the real robot

| Script | Role on the real robot |
| --- | --- |
| `Kommunikationsmodul.cs` | High-level decisions. Reads sensors, picks a target heading / speed, decides when to turn. |
| `Styrmodul.cs` | Takes the target vs. the current state and drives the two motors (speed-match or in-place turn). |
| `PID.cs` / `TargetAnglePID.cs` | The control loop. Plain C# with no Unity types, so it ports line-by-line to C. |
| `DCMotor.cs` | The physical motor + wheel. One instance per side, driven by `DIR` + PWM `duty`. |
| `WallSensor.cs` | The IR / ToF front sensor. Returns a distance in meters, capped at its range. |

The data flow is the same shape as the hardware:

```
WallSensor --> Kommunikationsmodul --> Styrmodul --> PID --> (DIR, duty) --> DCMotor x2
```

## Realistic motor model (`DCMotor.cs`)

Instead of just setting a velocity, each motor runs a small physics model that
matches a real brushed DC motor driving a wheel:

- **PWM duty** (0-1) sets the average voltage. `DIR` picks direction, same as
  the H-bridge inputs.
- **Deadband**: below some duty the motor can't overcome friction and nothing
  moves. The usable range above it is stretched back over 0-1.
- **Stall force + back-EMF**: push force is `stallForce * (duty - speed/maxSpeed)`.
  Full force at standstill, fading to zero as the wheel reaches the speed the
  duty is asking for -- just like a real motor loading up against its EMF.
- **Lateral grip**: a sideways force proportional to sideways slip, so the
  robot doesn't skate like it's on ice.

Forces are applied with `Rigidbody.AddForceAtPosition` at each wheel, so the
chassis actually rotates from an imbalance between the two sides -- the same
way the real robot turns.

## PID algorithm (`PID.cs`)

Written as plain C# on purpose -- no `UnityEngine` dependencies -- so it can
be dropped onto the microcontroller unchanged:

- Standard P + I + D, with per-term clamping (`iTermLimit`) and output
  saturation (`outputMin`/`outputMax`).
- Integral accumulates `Ki * error * dt`, not the raw error. Changing `Ki`
  live doesn't cause a jump in the output.
- D works on the **measurement rate**, not the error. Moving the setpoint
  doesn't kick the derivative term.
- `TargetAnglePID` wraps angles into `(-180, 180]` and low-pass filters the
  rate, which is what the heading loop needs on top of the generic PID.

`PIDVisualizer.cs` draws P / I / D / output and the error over time in the
scene view, so tuning is visual rather than guesswork.

## Loop timing

Physics (motors, forces) runs in `FixedUpdate` at Unity's fixed timestep, so
the simulation steps at a steady rate the way the real control loop does on
the MCU. The PID receives the actual `dt` each step.
