using Sandbox;
using Weapon = CitizenWarriorGame.Weapon;
public partial class MeleeSwingTracer {
    
    public Entity Owner;
    public Weapon Weapon;
	private float _counterMaxValue = 180;
	private float swingCounter = 0;
	private float meleeSwingOffset = -120;
    private float meleeAttackRange = 40.0f;

	public bool MeleeIsSwinging = false;

	public void SwingTracer() {
		if (swingCounter > _counterMaxValue) {
			MeleeIsSwinging = false;
			swingCounter = 0;
			return;
		}
		Rotation inputRotNoPitch = Owner.EyeRotation.Angles().WithPitch(0).WithYaw(Owner.EyeRotation.Angles().yaw + meleeSwingOffset + (swingCounter += 15)).ToRotation();
		DebugOverlay.Line(Owner.EyePosition, Owner.EyePosition + inputRotNoPitch.Forward.Normal * meleeAttackRange, Color.Yellow, 0.125f, false);
		DebugOverlay.Sphere(Owner.EyePosition + inputRotNoPitch.Forward.Normal * meleeAttackRange, 2, Color.Orange, 0.0f, false);
	}
	public void SwingTracer(float meleeSwingOffset, float swingMaxAngle, float swingCounterStep, float meleeAttackRange) {
		if (swingCounter > swingMaxAngle) {
			MeleeIsSwinging = false;
			swingCounter = 0;
			return;
		}
		Rotation inputRotNoPitch = Owner.EyeRotation.Angles().WithPitch(0).WithYaw(Owner.EyeRotation.Angles().yaw + meleeSwingOffset + (swingCounter += swingCounterStep)).ToRotation();
		Vector3 swingStepRayEndPosition = Owner.EyePosition + inputRotNoPitch.Forward.Normal * meleeAttackRange;
		DebugOverlay.Line(Owner.EyePosition, swingStepRayEndPosition, Color.Yellow, 0.125f, false);
		DebugOverlay.Sphere(swingStepRayEndPosition, 2, Color.Orange, 0.0f, false);

		TraceResult swingStepTraceResult = Trace.Ray(Owner.EyePosition, swingStepRayEndPosition)
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this.Weapon )
				.Run();
		if (swingStepTraceResult.Hit) {
			Log.Info("Something was hit!");
		}
	}
}