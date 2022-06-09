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

	public override void RenderHud( Vector2 screenSize )
	{
		var draw = Render.Draw2D;
		var center = screenSize / 2.0f;

		//
		// Properties
		//
		draw.Color = Color.White;

		float t = TimeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		draw.Color = draw.Color.WithAlpha( t );

		// Dot
		draw.Circle( center, 1f );
	}
}
