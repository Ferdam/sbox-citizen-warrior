
using System;
using Sandbox;

public partial class TopDownController : WalkController
{
    public bool IsFirstPerson => ((CitizenWarriorPlayer) Pawn).CameraMode is FirstPersonCamera;
    public override void Simulate()
    {
        base.Simulate();
        
        var noPitchRot = Input.Rotation.Angles().WithPitch(0).ToRotation();
		Rotation = noPitchRot;
		EyeRotation = noPitchRot;
        if (IsFirstPerson) {
            Rotation = Input.Rotation;
            EyeRotation = Input.Rotation;
        }
    }

    public override void FrameSimulate()
    {
        base.FrameSimulate();
        var noPitchRot = Input.Rotation.Angles().WithPitch(0).ToRotation();
        Rotation = noPitchRot;
		EyeRotation = noPitchRot;
        if (IsFirstPerson) {
            Rotation = Input.Rotation;
            EyeRotation = Input.Rotation;
        }
    }
}
