namespace OpenArena;

[Title( "Base Weapon" ), Icon( "sports_martial_arts" )]
public partial class BaseWeapon : BaseCarriable
{
	protected WeaponDataAsset WeaponData { get; private set; }

	public override void Spawn()
	{
		base.Spawn();
		LoadWeaponData();

		SetModel( WeaponData.WorldModel );

		CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
		SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		LoadWeaponData();
	}

	private void LoadWeaponData()
	{
		Log.Trace( $"Loaded weapon data on {( IsServer ? "Server" : "Client" )}" );

		var typeName = this.GetLibraryName();
		WeaponData = ResourceLibrary.GetAll<WeaponDataAsset>().FirstOrDefault( x => x.LibraryName == typeName );

		if ( WeaponData == null )
		{
			throw new Exception( $"No matching weapon data asset for '{typeName}'" );
		}
	}

	public override void CreateViewModel()
	{
		Log.Trace( $"CreateViewModel start {this}" );

		Host.AssertClient();

		if ( string.IsNullOrEmpty( WeaponData.ViewModel ) )
			return;

		ViewModelEntity = new ViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( WeaponData.ViewModel );

		Log.Trace( $"CreateViewModel end {this}" );
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

		if ( !Owner.IsValid() || !isFiring ) return false;

		var rate = WeaponData.Rate;
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

		var tracerParticles = Particles.Create( WeaponData.TracerParticles, effectEntity, "muzzle" );
		tracerParticles.SetPosition( 1, tr.EndPosition );

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
