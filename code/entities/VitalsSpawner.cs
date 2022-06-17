namespace OpenArena;

[Title( "Vitals Spawner" ), Icon( "add" ), Category( "World" )]
[EditorModel( "models/world/weapon_spawn.vmdl" )]
[Library( "oa_info_vitals_spawn" )]
[HammerEntity]
public partial class VitalsSpawner : BasePickup
{
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		SetGhostModel( "models/world/health_pill.vmdl" );
	}

	private void GivePlayerHealth( Player player )
	{
		player.Health += 15;
		PlayDeploySound( To.Single( player ) );
	}

	[ClientRpc]
	public void PlayDeploySound()
	{
		Sound.FromScreen( "heal" );
	}

	public override void OnPickup( Player player )
	{
		base.OnPickup( player );
		GivePlayerHealth( player );
	}
}

