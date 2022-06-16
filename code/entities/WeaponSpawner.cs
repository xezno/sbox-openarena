﻿namespace OpenArena;

[Title( "Weapon Spawner" ), Icon( "add" ), Category( "World" )]
[EditorModel( "models/world/weapon_spawn.vmdl" )]
[Library( "oa_info_weapon_spawn" )]
[HammerEntity]
public partial class WeaponSpawner : ModelEntity
{
	private SceneObject weaponSceneObject;

	private WeaponDataAsset WeaponData { get; set; }
	private float CooldownPeriod => 5f;
	private bool IsReady => TimeUntilReady <= 0;

	[Net] private TimeUntil TimeUntilReady { get; set; } = 0;
	[Property, Net] public string WeaponLibraryName { get; set; } = "oa_weapon_smg";

	private Particles progressParticles;

	public override void Spawn()
	{
		base.Spawn();

		WeaponData = WeaponDataAsset.FindByLibraryName( WeaponLibraryName );

		SetModel( "models/world/weapon_spawn.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		CollisionGroup = CollisionGroup.Trigger;

		EnableDrawing = false;
	}

	public override void ClientSpawn()
	{
		WeaponData = WeaponDataAsset.FindByLibraryName( WeaponLibraryName );

		var weaponAttachmentTransform = GetAttachment( "weapon" ) ?? default;
		weaponSceneObject = new( Map.Scene, Model.Load( WeaponData.WorldModel ), weaponAttachmentTransform );

		progressParticles = Particles.Create( "particles/pickup.vpcf" );
	}

	[Event.Frame]
	public void OnFrame()
	{
		Host.AssertClient();

		{
			float t = TimeUntilReady.Relative.LerpInverse( CooldownPeriod, 0 );

			progressParticles.SetPosition( 0, Position + Vector3.Up * 0.25f );
			progressParticles.SetPositionComponent( 1, 0, t );
			progressParticles.SetPositionComponent( 1, 1, IsReady ? 1 : 0 );
		}

		{
			Vector3 bobbingOffset = Vector3.Up * MathF.Sin( Time.Now * 2f ) * 4f;
			weaponSceneObject.Rotation = Rotation.From( 0, Time.Now * 90f, 0 );
			weaponSceneObject.Position += bobbingOffset * Time.Delta;

			weaponSceneObject.SetMaterialOverride( Material.Load( "materials/dev/reflectivity_90b.vmat" ) );
			weaponSceneObject.Flags.IsTranslucent = true;

			if ( IsReady )
				weaponSceneObject.ColorTint = Color.White;
			else
				weaponSceneObject.ColorTint = Color.Black.WithAlpha( 0.25f );
		}
	}

	private void GivePlayerWeapon( Player player )
	{
		if ( !IsServer )
			return;

		if ( player.Inventory.ContainsAny( WeaponLibraryName ) )
		{
			player.Inventory.First( WeaponLibraryName ).Ammo.Count += 100;
			return;
		}

		var weapon = TypeLibrary.Create<BaseWeapon>( WeaponLibraryName );
		player.Inventory.Add( weapon, true );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsReady )
			return;

		if ( other is Player player )
		{
			GivePlayerWeapon( player );
			TimeUntilReady = CooldownPeriod;
		}
	}
}
