namespace OpenArena;

[Library( "oa_weapon_rpg" )]
public class RocketLauncher : BaseWeapon
{
	private float Speed => 1000f;

	public override void Simulate( Client cl )
	{
		Entity.All.OfType<Rocket>().ToList().ForEach( x => x.Simulate( cl ) );

		base.Simulate( cl );
	}

	public override void ShootBullet()
	{
		if ( IsServer )
		{
			var player = Owner;

			var rocket = new Rocket();
			rocket.Position = player.EyePosition + player.EyeRotation.Forward * 32f;
			rocket.Rotation = player.EyeRotation;
			rocket.Velocity = ( player.Velocity.Length + Speed ) * player.EyeRotation.Forward;

			rocket.Owner = player;
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );

		using ( Prediction.Off() )
		{
			var fireSound = Path.GetFileNameWithoutExtension( WeaponData.FireSound );
			PlaySound( fireSound );
		}
	}
}
