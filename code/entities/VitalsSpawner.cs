namespace OpenArena;

[Title( "Vitals Spawner" ), Icon( "add" ), Category( "World" )]
[EditorModel( "models/world/weapon_spawn.vmdl" )]
[Library( "oa_info_vitals_spawn" )]
[HammerEntity]
public partial class VitalsSpawner : ModelEntity
{
	private SceneObject weaponSceneObject;

	private float CooldownPeriod => 5f;
	private bool IsReady => TimeUntilReady <= 0;
	[Net] private TimeUntil TimeUntilReady { get; set; } = 0;

	private float baseOffset;
	private Particles progressParticles;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/world/weapon_spawn.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		CollisionGroup = CollisionGroup.Trigger;

		EnableDrawing = false;
	}

	public override void ClientSpawn()
	{
		var weaponAttachmentTransform = GetAttachment( "weapon" ) ?? default;
		weaponSceneObject = new( Map.Scene, Model.Load( "models/world/health_pill" ), weaponAttachmentTransform );

		progressParticles = Particles.Create( "particles/pickup.vpcf" );

		baseOffset = Rand.Float( 0, MathF.PI * 2 ); // 0 to 1 sine cycles
	}

	[Event.Frame]
	public void OnFrame()
	{
		Host.AssertClient();

		float t = TimeUntilReady.Relative.LerpInverse( CooldownPeriod, 0 );

		{
			progressParticles.SetPosition( 0, Position + Vector3.Up * 0.25f );
			progressParticles.SetPositionComponent( 1, 0, t );
			progressParticles.SetPositionComponent( 1, 1, IsReady ? 1 : 0 );
		}

		{
			float sinX = Time.Now + baseOffset;
			Vector3 bobbingOffset = Vector3.Up * MathF.Sin( sinX * 2f ) * 4f;
			weaponSceneObject.Rotation = Rotation.From( 0, sinX * 90f, 0 );
			weaponSceneObject.Position += bobbingOffset * Time.Delta;

			weaponSceneObject.Flags.IsTranslucent = true;
			weaponSceneObject.Flags.IsOpaque = false;

			float alpha = t < 1.0f ? 0.5f : 1.0f;
			weaponSceneObject.ColorTint = Color.White * alpha;
		}
	}

	private void GivePlayerHealth( Player player )
	{
		if ( !IsServer )
			return;

		player.Health = player.Health + 15;
		PlayDeploySound( To.Single( player ) );
	}

	[ClientRpc]
	public void PlayDeploySound()
	{
		Sound.FromScreen( "heal" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsReady )
			return;

		if ( other is Player player )
		{
			GivePlayerHealth( player );
			TimeUntilReady = CooldownPeriod;
		}
	}
}

