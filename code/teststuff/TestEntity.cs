using Sandbox;

[Spawnable]
[Library( "ent_test", Title = "Test Entity" )]
public partial class TestEntity : Entity
{

	public override void Spawn()
	{
		base.Spawn();
        this.Position = new Vector3(0,0,200);
        this.Rotation = new Rotation(0,0,0,0);
        Log.Info("Test entity spawned");
        this.EnableDrawing = true;
        // DebugOverlay.Axis(Position, Rotation, 200, 60, false);
        // DebugOverlay.Text(Position.ToString(), Position, 5, Color.Orange, 60, 1500);
	}

    private float _counter = 0;
    private float counter {
        get { return _counter; }
        set { _counter = value > 360 ? 0 : value; }
    }

    public static bool IsEnabled = false;

    public override void Simulate(Client client) {
        if (!IsEnabled) return;
        base.Simulate(client);
        // Log.Info("runs");
        Vector3 vec = new Vector3(10,20,200);
        Rotation rot = new Angles(0,counter,counter).ToRotation();
        // Rotation rot = new Angles(0,0,0).LerpTo(new Angles(0,270,0), counter/100).ToRotation();
        Transform transf = new Transform(vec, rot, 1);
        
        // new Rotation(0,0.9f,0.5f,0);
        int vecLength = 90;
        // vec = vec * Input.Rotation;
        // rot = new Rotation(0,0,0,0);
        Vector3 transfVec = transf.TransformVector(vec);
        DebugOverlay.ScreenText(vec.ToString() + " -> " + transfVec.ToString(), new Vector2(25,25), 0);

        Vector3 vecX = transfVec.WithX(transfVec.x + vecLength);
        Vector3 vecY = transfVec.WithY(transfVec.y + vecLength);
        Vector3 vecZ = transfVec.WithZ(transfVec.z + vecLength);

        DebugOverlay.Axis(vec, rot, vecLength, 0, true);
        // DebugOverlay.Axis(transf.TransformVector(vec), Angles.Zero.ToRotation(), vecLength/2, 2, false);
        DebugOverlay.Sphere(transfVec,2,Color.Red,6.9f, false);
        DebugOverlay.Sphere(vec,2,Color.Gray,0, false);
        // DebugOverlay.Text(vec.ToString(), vec, 0, Color.Orange, 1, 1500);
        // DebugOverlay.Text(vecX.ToString(), vecX, 0, Color.Orange, 1, 1500);
        // DebugOverlay.Text(vecY.ToString(), vecY, 0, Color.Orange, 1, 1500);
        // DebugOverlay.Text(vecZ.ToString(), vecZ, 0, Color.Orange, 1, 1500);

        DebugOverlay.Line(transfVec, vecX, Color.Red, 0, true);
        DebugOverlay.Line(transfVec, vecY, Color.Green, 0, true);
        DebugOverlay.Line(transfVec, vecZ, Color.Blue, 0, true);
        DebugOverlay.Line(transfVec, vec, Color.Black, 0, false);
        counter++;
    }

    // public override void FrameSimulate(Client client) {
    //     base.FrameSimulate(client);
    //     Log.Info("runs");
    //     Vector3 vec = new Vector3(10,20,200);
    //     Rotation rot = new Rotation(0,0,0,0);
    //     DebugOverlay.Axis(vec, rot, 200, 1, false);
    //     DebugOverlay.Text(vec.ToString(), vec, 5, Color.Orange, 1, 1500);
    // }

	public override void OnKilled()
	{
		base.OnKilled();

		PlaySound( "balloon_pop_cute" );
	}
}
