using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

partial class TopDownGame : Game
{
	public TopDownGame()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}
	}

	public TestEntity TestEntityInstance;

	public TopDownGame GameInstance => this;

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new CitizenWarriorPlayer( cl );
		player.GameInstance = this;
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void DoPlayerNoclip( Client player )
	{
		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}

	[ConCmd.Server("respawn")]
    public static void RespawnPlayer() {
		Log.Info("Respawning...");
        var callingClient = ConsoleSystem.Caller;
        Sandbox.Player callingClientPawn = (Sandbox.Player) callingClient.Pawn;
        callingClientPawn.Respawn();
        // callingClientPawn.Spawn();
    }

	[ConCmd.Server("entlist")]
    public static void EntityList() {
		Log.Info("entity list: " + Game.All.Count);
		foreach( var ent in Game.All) {
			Log.Info(ent.GetHashCode());
		}
    }

	[ConCmd.Server("spawn_melon")]
    public static void SpawnMelon() {
		MelonProp melonProp = new MelonProp();
    }

	[ConCmd.Admin( "respawn_entities" )]
	public static void RespawnEntities()
	{
		Map.Reset( DefaultCleanupFilter );
	}

	[ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		// KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
		Log.Info("OnKilledMessage ");
	}

}
