using Sandbox;

namespace Fortress;

[Library( "ft_rocket_launcher" )]
public class RocketLauncher : BaseWeapon
{
	private float Speed => 1000f;
	public override float Rate => 1.0f;
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Pressed( InputButton.PrimaryAttack ) ) return false;

		var rate = Rate;
		if ( rate <= 0 ) return true;

		return TimeSinceAttack > (1 / rate);
	}

	public override void AttackPrimary()
	{
		base.AttackPrimary();

		if ( IsServer )
		{
			var player = Owner;

			var rocket = new Rocket();
			rocket.Position = player.EyePosition + player.EyeRotation.Forward * 32f;
			rocket.Rotation = player.EyeRotation;
			rocket.Velocity = (player.Velocity.Length + Speed) * player.EyeRotation.Forward;

			rocket.Owner = player;
			rocket.Predictable = true;
		}

		(ViewModelEntity as AnimatedEntity)?.SetAnimParameter( "fire", true );
	}

	public override void RenderHud( Vector2 screenSize )
	{
		var draw = Render.Draw2D;
		var center = screenSize / 2.0f;

		//
		// Properties
		//
		float radius = 16f;
		int count = 3;
		float gap = 20;

		//
		// Animation / easing
		//
		float t = TimeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		draw.Color = Color.White.WithAlpha( t );
		radius *= 2.0f.LerpTo( 1.0f, t );
		gap *= 2.0f.LerpTo( 1.0f, t );

		//
		// Circle crosshair
		//
		float interval = 360 / count;
		for ( int i = 0; i < count; ++i )
		{
			float startAngle = gap + (interval * i);
			float endAngle = (interval * (i + 1)) - gap;
			draw.CircleEx( center, radius, radius - 1f, startAngle: startAngle, endAngle: endAngle );
		}

		//
		// Cross crosshair
		//
		//// N
		//draw.Line( thickness, center - new Vector2( 0, gap + size ), center - new Vector2( 0, gap ) );

		//// S
		//draw.Line( thickness, center + new Vector2( 0, gap + size ), center + new Vector2( 0, gap ) );

		//// E
		//draw.Line( thickness, center - new Vector2( gap + size, 0 ), center - new Vector2( gap, 0 ) );

		//// W
		//draw.Line( thickness, center + new Vector2( gap + size, 0 ), center + new Vector2( gap, 0 ) );
	}
}
