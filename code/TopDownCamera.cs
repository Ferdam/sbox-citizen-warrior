using Sandbox;
public partial class TopDownCamera : Sandbox.CameraMode
{
    public static bool thirdperson_orbit { get; set; } = false;

    public static bool thirdperson_collision { get; set; } = true;

    public Angles _orbitAngles;
    public float _orbitDistance = 150;
    public float _orbitHeight = 0;
    public float _cameraDistance = 130.0f;
    public Angles OrbitAngles;
    // public float OrbitDistance { get{ return _orbitDistance; } set { this._orbitDistance = value; } }
    public float OrbitDistance = 500;
    // public float OrbitHeight { get{ return _orbitHeight; } set { this._orbitHeight = value; }}
    public float OrbitHeight = 200;
    // public float CameraDistance { get{ return _cameraDistance; } set { this._cameraDistance = value; } }
    public float CameraDistance = 70;
    public float CameraHeight = 350;
    public float CameraInclination = 70;
    public float HorizontalOffset = 0;

    public override void Update()
    {
        if ( Local.Pawn is not AnimatedEntity pawn )
            return;

        Position = pawn.Position;
        Vector3 targetPos;

        var center = pawn.Position + Vector3.Up * 64;

        if ( thirdperson_orbit )
        {
            Position += Vector3.Up * ((pawn.CollisionBounds.Center.z * pawn.Scale) + OrbitHeight);
            Rotation = Rotation.From( OrbitAngles );

            targetPos = Position + Rotation.Backward * OrbitDistance;
        }
        else
        {
            var inputRotationNoPitch = Input.Rotation.Angles().WithPitch(0).ToRotation();
            Position = center;
            Rotation = Rotation.FromAxis( Vector3.Up, 4 ) * Input.Rotation.Angles().WithPitch(CameraInclination).ToRotation();

            float distance = CameraDistance * pawn.Scale;
            targetPos = Position + inputRotationNoPitch.Right * ((pawn.CollisionBounds.Maxs.x + HorizontalOffset) * pawn.Scale);
            targetPos += inputRotationNoPitch.Forward * -distance;
            targetPos.z += CameraHeight;
        }

        if ( thirdperson_collision )
        {
            var tr = Trace.Ray( Position, targetPos )
                .WithAnyTags( "solid" )
                .Ignore( pawn )
                .Radius( 8 )
                .Run();

            Position = tr.EndPosition;
        }
        else
        {
            Position = targetPos;
        }

        // DebugOverlay.Line(Position, targetPos)

        FieldOfView = 70;

        Viewer = null;
    }

    public override void BuildInput( InputBuilder input )
    {
        if ( thirdperson_orbit && input.Down( InputButton.Walk ) )
        {
            if ( input.Down( InputButton.PrimaryAttack ) )
            {
                OrbitDistance += input.AnalogLook.pitch;
                OrbitDistance = OrbitDistance.Clamp( 0, 1000 );
            }
            else if ( input.Down( InputButton.SecondaryAttack ) )
            {
                OrbitHeight += input.AnalogLook.pitch;
                OrbitHeight = OrbitHeight.Clamp( -1000, 1000 );
            }
            else
            {
                OrbitAngles.yaw += input.AnalogLook.yaw;
                OrbitAngles.pitch += input.AnalogLook.pitch;
                OrbitAngles = OrbitAngles.Normal;
                OrbitAngles.pitch = OrbitAngles.pitch.Clamp( -89, 89 );
            }

            input.AnalogLook = Angles.Zero;

            input.Clear();
            input.StopProcessing = true;
        }

        base.BuildInput( input );
    }
}