using System.Linq;
using Sandbox;

public partial class TopDownCameraOld : Sandbox.CameraMode {

    public (float x, float y, float z) CameraOffset = (200,0,400);
    public float CameraFOV = 65;

    private TopDownCameraOld _cameraInstance = null;

    public TopDownCameraOld() {
        this._cameraInstance = this;
    }
    public override void Update() {
        FieldOfView = CameraFOV;
        // SetupAsFixed();
        SetupAsLookAtPlayer();
    }

    public void SetupAsFixed() {
        Vector3 playerPos = Local.Client.Pawn.Position;
        Vector3 cameraPos = new Vector3(playerPos.x + CameraOffset.x, playerPos.y + CameraOffset.y, playerPos.z + CameraOffset.z);
        Position = cameraPos;
        // Rotation = Local.Client.Pawn.EyeRotation.Angles().WithPitch(50).ToRotation();
        // Rotation = Rotation.LookAt(playerPos) * -1
        // Input.Rotation.Angles().WithPitch(0).ToRotation()
        var trResult = Trace.Ray(Position, playerPos).Run();
        Rotation = trResult.Direction.EulerAngles.ToRotation();
        FieldOfView = CameraFOV;
    }

    float _xoff = 0;
    float xOffset { get { _xoff = _xoff > 400 ? -1 : _xoff++; return _xoff++; } }
    float _yoff = 0;
    float yOffset { get { _yoff = _yoff > 400 ? -1 : _yoff++; return _yoff++; } }
    public void SetupAsLookAtPlayer() {
        
        Rotation playerRot = Input.Rotation.Angles().WithPitch(70).ToRotation();
        Rotation playerRotNoPitch = Input.Rotation.Angles().WithPitch(0).ToRotation();
        Vector3 playerPos = ((CitizenWarriorPlayer)Entity).EyePosition;
        Vector3 cameraPos = new Vector3(playerPos.x + 100, playerPos.y, playerPos.z+500);
        var trResult = Trace.Ray(playerPos*playerRotNoPitch, playerPos.Normal*playerRot + 200).Run();
        Position = playerPos.WithZ(500);
        Rotation = playerRot;
        DebugOverlay.Line(playerPos*playerRotNoPitch, playerPos*playerRotNoPitch + 30, Color.Black, 3, false);
        DebugOverlay.Text("start", playerPos*playerRotNoPitch, 0, Color.Orange, 1, 2000);
        DebugOverlay.Text("end", playerPos*playerRotNoPitch + 30, 0, Color.Red, 1, 2000);
        
        // Rotation = trResult.Direction.EulerAngles.WithYaw(30).ToRotation();

        // Rotation = playerRot.Angles().WithPitch(70).ToRotation();
        // DebugOverlay.Line(Position, playerPos, Color.Black, 1, false);
        DebugOverlay.Axis(playerPos, playerRot, 20, 0, false);
        // Rotation = playerRot;
    }
    public void SetupAsLookAtPlayerOld() {
        Vector3 playerPos = Local.Client.Pawn.Position;
        Vector3 cameraPos = new Vector3(playerPos.x + CameraOffset.x, playerPos.y + CameraOffset.y, playerPos.z + CameraOffset.z);
        Position = cameraPos;
        // playerPos.
        var trResult = Trace.Ray(Position, playerPos).Run();
        Rotation = trResult.Direction.EulerAngles.ToRotation();
        FieldOfView = CameraFOV;
    }

    internal class TopDownCameraSetup
    {
        public TopDownCamera CameraInstance = null;

        public TopDownCameraSetup(TopDownCamera instance) {
            this.CameraInstance = instance;
        }

        public void CameraMethod() {

        }
    }

    public enum TopDownCameraPreset
    {
        Fixed,
        LockToPlayer
    }
}