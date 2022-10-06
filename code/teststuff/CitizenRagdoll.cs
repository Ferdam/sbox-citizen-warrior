// "models/sbox_sword_03.vmdl";

using System.Collections.Generic;
using Sandbox;

public partial class CitizenRagdoll : ModelEntity {
    public bool UpdateParent = true;
    public List<ModelEntity> AttachList => new List<ModelEntity>();
    public override void Spawn() {
        base.Spawn();
        SetModel( "models/citizen/citizen.vmdl" );
        
        // Position = new Vector3(0,0,200);
        // Rotation = Vector3.Up.EulerAngles.;
        var owner = Owner;
        // Position = new Vector3(owner.Position.x+50, owner.Position.y, owner.Position.z+500);
        SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
        if (this.AttachList.Count == 0) {
            ModelEntity attach = new ModelEntity();
            attach.SetModel("models/sbox_sword_03.vmdl");
            this.AttachList.Add(attach);
            var handBone = GetBoneTransform("hold_R", false);
            attach.SetParent(this, "hold_R", new Transform(new Vector3(0,0,-6), Rotation.Forward.EulerAngles.ToRotation()));
        }
    }

    public override void Simulate(Client cl)
    {
        base.Simulate(cl);
        // if (UpdateParent) {
        //     AttachList[0].SetParent(this, "hold_R", new Transform(new Vector3(0,0,-3), Rotation.Forward.EulerAngles.ToRotation()));
        //     UpdateParent = false;
        //     Log.Info("Updated parent!");
        // }
        // AttachList[0].Position = AttachList[0].Position.WithZ(-6);
    }
}