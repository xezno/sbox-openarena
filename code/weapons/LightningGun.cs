namespace OpenArena;

[Library( "oa_weapon_lightning_gun" )]
public class LightningGun : BaseWeapon
{
	private TimeSince timeSinceDamaged = 0f;
	private Particles particles;

	public override bool CanPrimaryAttack()
	{
		bool isFiring = WeaponData.AutoFire ? Input.Down( InputButton.PrimaryAttack )
											: Input.Pressed( InputButton.PrimaryAttack );

		if ( TimeSinceDeployed < WeaponData.DeployTime ) return false;
		if ( !Owner.IsValid() || !isFiring ) return false;
		return true;
	}

	public override void Simulate( Client player )
	{
		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() && Ammo.Count > 0 )
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

	public override void AttackPrimary()
	{
		ShootBullet();
	}

	public override void ShootBullet()
	{
		if ( particles == null )
		{
			Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;
			particles = Particles.Create( "particles/lightning.vpcf", effectEntity, "muzzle" );
		}

		var tr = TraceBullet();
		particles.SetPosition( 1, tr.EndPosition );
		DoScorchMarks( tr );

		if ( tr.Hit && timeSinceDamaged > ( 1 / WeaponData.Rate ) )
		{
			timeSinceDamaged = 0;

			if ( !Ammo.Take() )
				return;

			if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
			{
				tr.Entity.TakeDamage( DamageInfo
					.FromBullet( tr.EndPosition, tr.Direction * 32, WeaponData.Damage )
					.WithAttacker( Owner ) );
			}
		}
	}

	private void DoScorchMarks( TraceResult tr )
	{
		//
		// No effects on resimulate
		//
		if ( !Prediction.FirstTime )
			return;

		//
		// Drop a decal
		//
		var decalPath = "data/decals/scorch.decal";

		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decal ) )
		{
			DecalSystem.PlaceUsingTrace( decal, tr );
		}
	}
}
