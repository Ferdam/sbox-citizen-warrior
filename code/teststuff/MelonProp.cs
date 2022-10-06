using Sandbox;

[Spawnable]
[Library( "prop_melon", Title = "Melon prop" )]
public partial class MelonProp : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();
        SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
        Position = new Vector3(0,0,200);
        Rotation = new Rotation(0,0,0,0);
        EnableDrawing = true;
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic, false);
        PhysicsEnabled = true;
	}

	public override void OnKilled()
	{
		base.OnKilled();
		PlaySound( "balloon_pop_cute" );
	}
}
