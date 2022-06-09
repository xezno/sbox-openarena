using Sandbox.UI.Tests;

namespace OpenArena;

[Title( "Base Weapon" ), Icon( "sports_martial_arts" )]
public partial class BaseWeapon : BaseCarriable
{
	public virtual string WorldModel => "";
	public virtual bool AutoFire => true;
	public virtual float Rate => 5.0f;
	public virtual string FireSound => "rust_pistol.shoot";
	public virtual float Damage => 10f;
	public virtual string MuzzleFlashParticles => "particles/pistol_muzzleflash.vpcf";
	public virtual string TracerParticles => "particles/tracer.vpcf";

	public override void Spawn()
	{
		base.Spawn();
		SetModel( WorldModel );

		CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
		SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it
	}

	[Net, Predicted]
	public TimeSince TimeSinceAttack { get; set; }

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
	}

	public virtual bool CanPrimaryAttack()
	{
		bool isFiring = AutoFire ? Input.Down( InputButton.PrimaryAttack )
								 : Input.Pressed( InputButton.PrimaryAttack );

		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = Rate;
		if ( rate <= 0 ) return true;

		return TimeSinceAttack > ( 1 / rate );
	}

	public virtual void AttackPrimary()
	{
		ShootBullet();
	}

	public virtual void ShootBullet()
	{
		var tr = TraceBullet();

		//
		// Bullet damage etc.
		//
		if ( tr.Hit )
		{
			tr.Surface.DoBulletImpact( tr );
			if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
			{
				tr.Entity.TakeDamage( DamageInfo
					.FromBullet( tr.EndPosition, tr.Direction * 32, Damage )
					.WithAttacker( Owner ) );
			}
		}

		//
		// Shoot effects
		//
		// using ( Prediction.Off() )
		{
			CreateShootEffects( tr );
		}
	}

	protected virtual void CreateShootEffects( TraceResult tr )
	{
		Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;

		var tracerParticles = Particles.Create( TracerParticles, effectEntity, "muzzle" );
		tracerParticles.SetPosition( 1, tr.EndPosition );

		_ = Particles.Create( MuzzleFlashParticles, effectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		PlaySound( FireSound );
	}

	public virtual TraceResult TraceBullet()
	{
		var start = Owner.EyePosition;
		var end = Owner.EyePosition + Owner.EyeRotation.Forward * 8192f;
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

	public override Sound PlaySound( string soundName, string attachment )
	{
		if ( Owner.IsValid() )
			return Owner.PlaySound( soundName, attachment );

		return base.PlaySound( soundName, attachment );
	}

	public virtual void RenderHud( Vector2 screenSize )
	{

	}
}
