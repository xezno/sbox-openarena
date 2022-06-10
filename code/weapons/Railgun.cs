namespace OpenArena;

[Library( "oa_weapon_railgun" )]
public class Railgun : BaseWeapon
{
	protected override void CreateShootEffects( TraceResult tr )
	{
		Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;

		var muzzleTransform = ( effectEntity as ModelEntity )?.GetAttachment( "muzzle" ) ?? default;
		var startPosition = muzzleTransform.Position;

		var tracerParticles = Particles.Create( WeaponData.TracerParticles, startPosition );
		tracerParticles.SetForward( 0, tr.Direction );
		tracerParticles.SetPosition( 1, tr.EndPosition );

		_ = Particles.Create( WeaponData.MuzzleFlashParticles, effectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		PlaySound( WeaponData.FireSound );
	}

	protected override DamageInfo CreateDamageInfo( TraceResult tr )
	{
		var damageInfo = base.CreateDamageInfo( tr );

		Log.Trace( tr.Bone );
		if ( tr.Bone == 5 ) // Head bone index: 5
			damageInfo.Damage *= 2.0f;

		return damageInfo;
	}
}
