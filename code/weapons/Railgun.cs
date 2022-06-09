namespace OpenArena;

public class Railgun : BaseWeapon
{
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float Rate => 1;
	public override string WorldModel => "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl";
	public override string FireSound => "rust_pumpshotgun.shoot";
	public override string TracerParticles => "particles/beam.vpcf";
	public override float Damage => 100;

	protected override void CreateShootEffects( TraceResult tr )
	{
		Entity effectEntity = IsLocalPawn ? ViewModelEntity : this;

		var muzzleTransform = ( effectEntity as ModelEntity )?.GetAttachment( "muzzle" ) ?? default;
		var startPosition = muzzleTransform.Position;

		var tracerParticles = Particles.Create( TracerParticles, startPosition );
		tracerParticles.SetForward( 0, tr.Direction );
		tracerParticles.SetPosition( 1, tr.EndPosition );

		_ = Particles.Create( MuzzleFlashParticles, effectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		PlaySound( FireSound );
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
