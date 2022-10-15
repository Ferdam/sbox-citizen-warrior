using System.Collections.Generic;
using System.Linq;
using Sandbox;

partial class SwordLong : CitizenWarriorGame.BaseWeapon
{	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 2.0f;
	public float PrimaryAttackDamage = 25.0f;
	public float SecondaryAttackDamage = 40.0f;
	public float MeleeAttackRange = 40.0f;

    public List<SwordPoseStance> ComboSequence = new List<SwordPoseStance>
    {
        SwordPoseStance.RightStance,
        SwordPoseStance.LeftStance,
	};

	public int _comboSequenceIndex = 0;
	public int ComboSequenceIndex
	{
		get { return _comboSequenceIndex; } 
		set { 
			_comboSequenceIndex = value % ComboSequence.Count;
			Log.Info(ComboSequence[_comboSequenceIndex]);
		}
	}
	
	public override void Spawn()
	{
		base.Spawn();
		(Owner as AnimatedEntity)?.SetAnimParameter( "stance_type", 0 );
		SetModel( "models/sbox_sword_03.vmdl" );
	}

	private void Attack( SwordPoseStance poseStance )
	{
		// if (MeleeIsSwinging) return;
		
		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );
		bool hitSuccess = MeleeAttack();
		// hitSuccess ? OnMeleeHit(poseStance) : OnMeleeMiss(poseStance);
		(Owner as AnimatedEntity)?.SetAnimParameter( "stance_type", (int)poseStance );
		ComboSequenceIndex++;
	}

	public override void AttackPrimary()
	{
		Attack( ComboSequence[ComboSequenceIndex] );
	}

	public override void AttackSecondary()
	{
		Attack( ComboSequence[ComboSequenceIndex] );
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

	[ClientRpc]
	private void OnMeleeMiss( SwordPoseStance poseStance )
	{
		Host.AssertClient();

		(Owner as AnimatedEntity)?.SetAnimParameter( "attack_has_hit", false );
	}

	[ClientRpc]
	private void OnMeleeHit( SwordPoseStance poseStance )
	{
		Host.AssertClient();

		(Owner as AnimatedEntity)?.SetAnimParameter( "attack_has_hit", true );
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	public override void SimulateAnimator( CWAnimationHelper anim )
	{
		AnimatedEntity playerModelAnim = Owner as AnimatedEntity;		
		var player = (CitizenWarriorPlayer) Owner;
		var playerController = (TopDownController) player.Controller;

		if (playerModelAnim != null) {
			if (playerModelAnim?.RootMotion != new Vector3(0,0,0)) {
				Log.Info("RootMotion: " + playerModelAnim?.RootMotion);
				
				var initialPos = playerController.Position;
				player.Position += playerModelAnim.RootMotion.Normal * player.Rotation;
			}
		}
		anim.HoldType = CWAnimationHelper.HoldTypes.Swing;
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
		new Rotation();
		this.SetParent(Owner, "hold_R", new Transform(new Vector3(-3.4f,0.1f,-8), new Rotation().Forward.EulerAngles.WithYaw(150).ToRotation()));
		
		AnimatedEntity playerModelAnim = Owner as AnimatedEntity;		
		
		var playerController = (TopDownController) player.Controller;
		if (playerModelAnim != null) {
			if (playerModelAnim?.RootMotion != new Vector3(0,0,0)) {
				Log.Info("RootMotion: " + playerModelAnim?.RootMotion);
				
				var initialPos = playerController.Position;
				// player.Position += playerModelAnim.RootMotion.Normal * player.Rotation;
				// playerController.Velocity = (playerModelAnim.RootMotion.Normal * player.Rotation).Distance(initialPos) * -Time.Delta;
				// player.Position += Vector3.Forward * player.Rotation * 1.0f;
				// player.Accelerate(Vector3.Forward * player.Rotation, 25, 55, 1.1f);
				// player.WalkMove();
				// player.Move();
			}
				// Owner.Velocity = 0;
			if (playerModelAnim.CurrentSequence.Name == "CW_Sword_Attack_a_move" || playerModelAnim.CurrentSequence.Name == "CW_Sword_Attack_b_move") {
			}
		}
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

	

	private void Activate()
	{
		// nothing yet
	}

	private void Deactivate()
	{
		// nothing yet
	}

	public override void ActiveStart( Entity ent )
	{
		EnableDrawing = true;
		base.ActiveStart( ent );

		if ( IsServer )
		{
			Activate();
		}
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		Log.Info("Called ActiveEnd()");
		EnableDrawing = false;
		base.ActiveEnd( ent, dropped );

		if ( IsServer )
		{
			if ( dropped )
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}
	}

	public enum SwordPoseStance
	{
		LeftStance,
		RightStance
	}
}