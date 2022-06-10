namespace OpenArena;

[Library( "oa_weapon_rpg" )]
public class RocketLauncher : BaseWeapon
{
	private float Speed => 1000f;

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
			rocket.Predictable = true;
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}
}
