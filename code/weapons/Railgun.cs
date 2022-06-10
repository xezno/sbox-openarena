namespace OpenArena;

[Library( "oa_weapon_railgun" )]
public class Railgun : BaseWeapon
{
	protected override void CreateShootEffects( Vector3 direction, Vector3 endPosition )
	{
		Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;

		var muzzleTransform = ( effectEntity as ModelEntity )?.GetAttachment( "muzzle" ) ?? default;
		var startPosition = muzzleTransform.Position;

		var tracerParticles = Particles.Create( WeaponData.TracerParticles, startPosition );
		tracerParticles.SetForward( 0, direction );
		tracerParticles.SetPosition( 1, endPosition );

		_ = Particles.Create( WeaponData.MuzzleFlashParticles, effectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		PlaySound( WeaponData.FireSound );
	}

	protected override DamageInfo CreateDamageInfo( TraceResult tr )
	{
		var damageInfo = base.CreateDamageInfo( tr );

		Log.Trace( tr.Bone );

		if ( tr.IsHeadshot() )
			damageInfo.Damage *= 2.0f;

		return damageInfo;
	}
}
