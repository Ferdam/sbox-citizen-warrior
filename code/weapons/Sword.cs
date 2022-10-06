using System.Linq;
using Sandbox;
using Weapon = CitizenWarriorGame.Weapon;

[Spawnable]
[Library( "weapon_sword", Title = "Sword" )]
partial class Sword : Weapon
{
	public override string ViewModelPath => "models/sbox_sword_03.vmdl";
	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 2.0f;
	public float PrimaryAttackDamage = 25.0f;
	public float SecondaryAttackDamage = 40.0f;
	public float MeleeAttackRange = 40.0f;
	
	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/sbox_sword_03.vmdl" );
	}

	public override bool CanReload()
	{
		return false;
	}

	private void Attack( bool leftHand )
	{
		if ( MeleeAttack() )
		{
			OnMeleeHit( leftHand );
		}
		else
		{
			OnMeleeMiss( leftHand );
		}

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );
	}

	public override void AttackPrimary()
	{
		Attack( true );
	}

	public override void AttackSecondary()
	{
		Attack( false );
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	public override void SimulateAnimator( CWAnimationHelper anim )
	{
		anim.HoldType = CWAnimationHelper.HoldTypes.Swing;
		// anim.HoldTypePose = CWAnimationHelper.HoldTypes.Swing;
		anim.Handedness = CWAnimationHelper.Hand.Right;
		anim.AimBodyWeight = 1.0f;
	}

	private float _counterMaxValue = 180;
	private float _counter = 0;
	private float swingCounter = 0;
	private float meleeSwingOffset = -120;

	public bool MeleeIsSwinging = false;
    private bool _debugEnabled => false;

    public override void Simulate(Client client) {
		base.Simulate(client);
		if (MeleeIsSwinging) {
			SwingTracer(-90, 150, 15, 60.0f);
		}
		EnableDrawing = !IsFirstPersonMode;
		var player = (CitizenWarriorPlayer) client.Pawn;
		var playerModel = (ModelEntity) client.Pawn;
		var holdBone = playerModel.GetBoneTransform("hold_R", true);
		// playerModel.SetBoneTransform("hold_R", Transform.ToWorld(new Transform(holdBone.Position, Rotation.Random)));
		// Rotation = Rotation.Random;
		new Rotation();
		this.SetParent(Owner, "hold_R", new Transform(new Vector3(-3.4f,0.1f,-8), new Rotation().Forward.EulerAngles.WithYaw(150).ToRotation()));
	}

	private void SwingTracer() {
		if (swingCounter > _counterMaxValue) {
			MeleeIsSwinging = false;
			swingCounter = 0;
			return;
		}
		Rotation inputRotNoPitch = Owner.EyeRotation.Angles().WithPitch(0).WithYaw(Owner.EyeRotation.Angles().yaw + meleeSwingOffset + (swingCounter += 15)).ToRotation();
		DebugOverlay.Line(Owner.EyePosition, Owner.EyePosition + inputRotNoPitch.Forward.Normal * MeleeAttackRange, Color.Yellow, 0.125f, false);
		DebugOverlay.Sphere(Owner.EyePosition + inputRotNoPitch.Forward.Normal * MeleeAttackRange, 2, Color.Orange, 0.0f, false);
	}
	private void SwingTracer(float meleeSwingOffset, float swingMaxAngle, float swingCounterStep, float meleeAttackRange) {
		var playerModel = (ModelEntity) Owner;

		if (swingCounter > swingMaxAngle) {
			MeleeIsSwinging = false;
			swingCounter = 0;
			return;
		}
		Rotation inputRotNoPitch = Owner.EyeRotation.Angles().WithPitch(0).WithYaw(Owner.EyeRotation.Angles().yaw + meleeSwingOffset + (swingCounter += swingCounterStep)).ToRotation();
		Vector3 swingStepRayEndPosition = Owner.EyePosition + inputRotNoPitch.Forward.Normal * meleeAttackRange;

        if (_debugEnabled)
        {
            DebugOverlay.Line(Owner.EyePosition, swingStepRayEndPosition, Color.Yellow, 0.125f, false);
			DebugOverlay.Sphere(swingStepRayEndPosition, 2, Color.Orange, 0.0f, false);
		}

		TraceResult swingStepTraceResult = Trace.Ray(Owner.EyePosition, swingStepRayEndPosition)
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this )
				.Run();
		if (swingStepTraceResult.Hit) {
			Log.Info("Attack Swing hit!");
		}
	}

	private bool MeleeAttack() {
		Log.Info("swingCounter " + swingCounter);
		if (MeleeIsSwinging) {
			return false;
		}
		_counterMaxValue = 150;
		meleeSwingOffset = -120;
		swingCounter = 0;
		MeleeIsSwinging = true;
		return true;
	}
	private bool MeleeAttackNew() {
		var forwardRotation = Owner.EyeRotation.Forward.Normal;

		var traceHitResult = Trace.Ray(Owner.EyePosition, Owner.EyePosition + forwardRotation * MeleeAttackRange)
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this )
				.Run();

		if (!traceHitResult.Entity.IsValid()) return false;

		traceHitResult.Surface.DoBulletImpact( traceHitResult );
		
		if (IsServer) {
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( traceHitResult.EndPosition, forwardRotation * 100, PrimaryAttackDamage )
					.UsingTraceResult( traceHitResult )
					.WithAttacker( Owner )
					.WithWeapon( this );

				traceHitResult.Entity.TakeDamage( damageInfo );
			}
		}

		return true;
	}

	private bool MeleeAttackOld()
	{
		var forward = Owner.EyeRotation.Forward;
		forward = forward.Normal;

		bool hit = false;

		MeleeAttackRange = 40;
		Log.Info(MeleeAttackRange);
				
		var traceMeleeResults = TraceMelee( Owner.EyePosition, Owner.EyePosition + forward * MeleeAttackRange, 50.0f );
		
		// while (traceMeleeResults || trRes < 10) { Log.Info("Tr: " + trRes++); }
		
		foreach ( var tr in traceMeleeResults )
		{
			if ( !tr.Entity.IsValid() ) continue;

			tr.Surface.DoBulletImpact( tr );
			
			DebugOverlay.Sphere(tr.HitPosition, 2, Color.Red, 10, false);

			hit = true;

			if ( !IsServer ) continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100, PrimaryAttackDamage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}

		return hit;
	}

	[ClientRpc]
	private void OnMeleeMiss( bool leftHand )
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "attack_has_hit", false );
		ViewModelEntity?.SetAnimParameter( "attack", true );
		ViewModelEntity?.SetAnimParameter( "holdtype_attack", 3 );
	}

	[ClientRpc]
	private void OnMeleeHit( bool leftHand )
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
		ViewModelEntity?.SetAnimParameter( "attack", true );
		ViewModelEntity?.SetAnimParameter( "holdtype_attack", 3 );
	}
}
