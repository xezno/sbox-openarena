namespace OpenArena;

[Library( "oa_weapon_lightning_gun" )]
public class LightningGun : BaseWeapon
{
	private TimeSince timeSinceDamaged = 0f;
	private Particles particles;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void Simulate( Client player )
	{
		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSinceAttack = 0;
				AttackPrimary();
			}
		}
		else
		{
			particles?.Destroy( true );
			particles = null;
		}
	}

	public override void ShootBullet()
	{
		if ( particles == null )
		{
			Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;
			particles = Particles.Create( "particles/physgun_beam.vpcf", effectEntity, "muzzle" );
		}

		var tr = TraceBullet();
		particles.SetPosition( 1, tr.EndPosition );

		if ( tr.Hit && timeSinceDamaged > 0.1f )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
			{
				tr.Entity.TakeDamage( DamageInfo
					.FromBullet( tr.EndPosition, tr.Direction * 32, 15f )
					.WithAttacker( Owner ) );
			}

			timeSinceDamaged = 0;
		}
	}

	public override void RenderHud( Vector2 screenSize )
	{
		var draw = Render.Draw2D;
		var center = screenSize / 2.0f;

		//
		// Properties
		//
		float size = 16f;
		float thickness = 1f;
		float gap = 8f;

		draw.Color = Color.White;

		float t = TimeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		gap *= 2.0f.LerpTo( 1.0f, t );

		// top left
		draw.Line( thickness, center - new Vector2( gap + size, gap + size ), center - new Vector2( gap, gap ) );
		draw.Line( thickness, center - new Vector2( -gap - size, gap + size ), center - new Vector2( -gap, gap ) );

		// S
		gap *= 1.5f;
		draw.Line( thickness, center + new Vector2( 0, gap + size ), center + new Vector2( 0, gap ) );

		// dot
		draw.Circle( center, 1f );
	}
}
