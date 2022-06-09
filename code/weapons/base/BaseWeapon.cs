namespace OpenArena;

[Title( "Base Weapon" ), Icon( "sports_martial_arts" )]
public partial class BaseWeapon : BaseCarriable
{
	protected WeaponDataAsset WeaponData { get; private set; }
	protected TimeSince TimeSinceDeployed { get; private set; }

	[Net, Predicted] public AmmoContainer Ammo { get; private set; }

	public override void Spawn()
	{
		base.Spawn();
		LoadWeaponData();

		SetModel( WeaponData.WorldModel );

		CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
		SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it

		Ammo = new();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		LoadWeaponData();
	}

	private void LoadWeaponData()
	{
		var typeName = this.GetLibraryName();
		WeaponData = ResourceLibrary.GetAll<WeaponDataAsset>().FirstOrDefault( x => x.LibraryName == typeName );

		if ( WeaponData == null )
		{
			throw new Exception( $"No matching weapon data asset for '{typeName}'" );
		}
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( WeaponData.ViewModel ) )
			return;

		ViewModelEntity = new ViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( WeaponData.ViewModel );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		( ViewModelEntity as AnimatedEntity )?.SetAnimParameter( "deploy", true );
		TimeSinceDeployed = 0;
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
		bool isFiring = WeaponData.AutoFire ? Input.Down( InputButton.PrimaryAttack )
											: Input.Pressed( InputButton.PrimaryAttack );

		if ( TimeSinceDeployed < WeaponData.DeployTime ) return false;
		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = WeaponData.Rate;
		if ( rate <= 0 ) return true;

		return TimeSinceAttack > ( 1 / rate );
	}

	public virtual void AttackPrimary()
	{
		//
		// Try to take ammo
		//
		if ( !Ammo.Take() )
			return;

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
					.FromBullet( tr.EndPosition, tr.Direction * 32, WeaponData.Damage )
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

		var start = ( effectEntity as ModelEntity ).GetAttachment( "muzzle" ) ?? default;
		var startPosition = start.Position + ( tr.Direction * 300 );
		var endPosition = tr.EndPosition;

		var tracerParticles = Particles.Create( WeaponData.TracerParticles, startPosition );
		tracerParticles.SetPosition( 1, endPosition );

		_ = Particles.Create( WeaponData.MuzzleFlashParticles, effectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		PlaySound( WeaponData.FireSound );
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
