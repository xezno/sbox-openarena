namespace OpenArena;

[Library( "oa_weapon_shotgun" )]
public class Shotgun : BaseWeapon
{
	public override void ShootBullet()
	{
		Rand.SetSeed( Time.Tick );
		for ( int x = -1; x <= 1; x++ )
		{
			for ( int y = -1; y <= 1; y++ )
			{
				float rand = Rand.Float( -1, 1 ) * WeaponData.SpreadRandomness;
				var offset = new Vector3( 0, x, y ) * WeaponData.Spread;
				offset += rand;

				var tr = TraceBullet( offset );

				//
				// Bullet damage etc.
				//
				if ( tr.Hit )
				{
					tr.Surface.DoBulletImpact( tr );
					if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
					{
						tr.Entity.TakeDamage( CreateDamageInfo( tr ) );
					}
				}

				//
				// Shoot effects
				//
				CreateShootEffects( tr.Direction, tr.EndPosition );
			}
		}

		using ( Prediction.Off() )
		{
			var fireSound = Path.GetFileNameWithoutExtension( WeaponData.FireSound );
			PlaySound( fireSound );
		}
	}

	private TraceResult TraceBullet( Vector3 offset )
	{
		using var _ = LagCompensation();

		var forward = Owner.EyeRotation.Forward + ( offset * Owner.EyeRotation );

		Vector3 start = Owner.EyePosition;
		Vector3 end = Owner.EyePosition + forward * 8192f;

		float radius = 2.0f;

		bool inWater = Map.Physics.IsPointWater( start );

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Water, !inWater )
				.HitLayer( CollisionLayer.Debris )
				.Ignore( Owner )
				.Ignore( this )
				.Size( radius )
				.Run();

		return tr;
	}
}
