using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weapon = CitizenWarriorGame.Weapon;

partial class SandboxPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	private DamageInfo lastDamage;

	public TopDownGame GameInstance = null;

	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public ClothingContainer Clothing = new();

	/// <summary>
	/// Default init
	/// </summary>
	public SandboxPlayer()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	/// Initialize using this client
	/// </summary>
	public SandboxPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public TestEntity TestEntityInst;

	public List<Weapon> meleeList = new List<Weapon>();

	public override void Respawn()
	{
		// foreach(var item in meleeList) {
		// 	item.Delete();
		// }
		// meleeList = new List<Weapon>();
		base.Respawn();
		// Inventory.DeleteContents();
		// Inventory.DeleteContents();
		SetModel( "models/citizenwarrior/citizenwarrior.vmdl" );

		Controller = new TopDownController();

		// if ( DevController is NoclipController )
		// {
		// 	DevController = null;
		// }

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableShadowCasting = true;

		Clothing.DressEntity( this );
		
		// Log.Info($"deleted {garbage.GetType()}: {garbage.GetHashCode()}");
		// Inventory = new Inventory(this);
		// Inventory.SetActive(null);
		// Inventory = new Inventory(this);
		Sword sw = new Sword();
		Inventory.Add( sw, true );
		// Inventory.Add( new Fists() );
		// Inventory.SetActive(sw);
		
		
		TopDownCamera.thirdperson_orbit = false;
		var topDownCamera = new TopDownCamera();
		// topDownCamera.CameraDistance = 5550;
		// topDownCamera.OrbitHeight = 2500.0f;
		CameraMode = topDownCamera;
		// topDownCamera.Activated();

		if (TestEntityInst == null) TestEntityInst = new TestEntity();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		CameraMode = new SpectateRagdollCamera();

		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		var controller = GetActiveController();
		if ( controller != null )
		{
			EnableSolidCollisions = !controller.HasTag( "noclip" );

			SimulateAnimation( controller );
		}

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			if (CameraMode is TopDownCamera) {
				CameraMode = new FirstPersonCamera();
				Controller = new WalkController();
				EnableHideInFirstPerson = true;
			}
			else {
				CameraMode = new TopDownCamera();
				Controller = new TopDownController();
				EnableHideInFirstPerson = false;
			}
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				// Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}

	}

	Entity lastWeapon;

	void SimulateAnimation( PawnController controller )
	{
		if ( controller == null )
			return;

		// where should we be rotated to
		var inputRotNoPitch = Input.Rotation.Angles().WithPitch(0).ToRotation();
		var turnSpeed = 0.02f;
		var idealRotation = Rotation.LookAt( inputRotNoPitch.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = inputRotNoPitch;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = ( Host.IsClient && Client.IsValid() ) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsNoclipping = controller.HasTag( "noclip" );
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = WaterLevel >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
		if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

		if ( ActiveChild is BaseCarriable carry )
		{
			carry.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
			animHelper.AimBodyWeight = 0.5f;
		}

		lastWeapon = ActiveChild;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	public override float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}

	[ConCmd.Server( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( slot.ClassName != entName )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	[ConCmd.Server( "spawnrag" )]
	public static void SpawnCitizenRagdoll()
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		CitizenRagdoll citi = new CitizenRagdoll() { Owner = target };
	}
}
