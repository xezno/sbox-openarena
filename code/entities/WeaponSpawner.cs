namespace OpenArena;

[Title( "Weapon Spawner" ), Icon( "add" ), Category( "World" )]
[EditorModel( "models/world/weapon_spawn.vmdl" )]
[Library( "oa_weapon_spawner" )]
[HammerEntity]
public class WeaponSpawner : ModelEntity
{
	private SceneObject weaponSceneObject;
	private TimeUntil timeUntilReady = 0;

	private float CooldownPeriod => 5f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/world/weapon_spawn.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		CollisionGroup = CollisionGroup.Trigger;
	}

	public override void ClientSpawn()
	{
		var weaponAttachmentTransform = GetAttachment( "weapon" ) ?? default;
		weaponSceneObject = new( Map.Scene, Model.Load( "weapons/rust_smg/rust_smg.vmdl" ), weaponAttachmentTransform );
	}

	[Event.Frame]
	public void OnFrame()
	{
		Host.AssertClient();

		DebugOverlay.Text( $"{timeUntilReady}", Position );

		Vector3 bobbingOffset = Vector3.Up * MathF.Sin( Time.Now * 2f ) * 4f;
		weaponSceneObject.Rotation = Rotation.From( 0, Time.Now * 90f, 0 );
		weaponSceneObject.Position += bobbingOffset * Time.Delta;

		weaponSceneObject.SetMaterialOverride( Material.Load( "materials/dev/reflectivity_90b.vmat" ) );

		if ( timeUntilReady <= 0 )
			weaponSceneObject.ColorTint = Color.White;
		else
			weaponSceneObject.ColorTint = Color.Black.WithAlpha( 0.25f );
	}

	private void GivePlayerWeapon( Player player )
	{
		if ( player.Inventory.ContainsAny<SMG>() ) // TODO: Collect ammo?
			return;

		player.Inventory.Add( new SMG(), true );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is Player player )
		{
			GivePlayerWeapon( player );
			timeUntilReady = CooldownPeriod;
		}
	}
}
