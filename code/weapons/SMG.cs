namespace Fortress;

public class SMG : BaseWeapon
{
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
	public override float Rate => 10;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "weapons/rust_smg/rust_smg.vmdl" );
	}

	public override void AttackPrimary()
	{
		base.AttackPrimary();

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 8192f ) )
		{
			Entity effectEntity = (IsLocalPawn) ? ViewModelEntity : this;

			var tracerParticles = Particles.Create( "particles/tracer.vpcf", effectEntity, "muzzle" );
			tracerParticles.SetPosition( 1, tr.EndPosition );

			_ = Particles.Create( "particles/pistol_muzzleflash.vpcf", effectEntity, "muzzle" );
			(ViewModelEntity as AnimatedEntity)?.SetAnimParameter( "fire", true );
			PlaySound( "rust_smg.shoot" );

			if ( tr.Hit )
			{
				tr.Surface.DoBulletImpact( tr );
				if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
				{
					tr.Entity.TakeDamage( DamageInfo
						.FromBullet( tr.EndPosition, tr.Direction * 32, 10 )
						.WithAttacker( Owner ) );
				}
			}
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

		// N
		draw.Line( thickness, center - new Vector2( 0, gap + size ), center - new Vector2( 0, gap ) );

		// S
		draw.Line( thickness, center + new Vector2( 0, gap + size ), center + new Vector2( 0, gap ) );

		// E
		draw.Line( thickness, center - new Vector2( gap + size, 0 ), center - new Vector2( gap, 0 ) );

		// W
		draw.Line( thickness, center + new Vector2( gap + size, 0 ), center + new Vector2( gap, 0 ) );
	}
}
