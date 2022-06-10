namespace OpenArena;

[Library( "oa_crosshair_circle" )]
public class CircleCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack, Vector2 screenSize )
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
		float t = timeSinceAttack.Relative.LerpInverse( 0, 0.5f );
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
			float startAngle = gap + ( interval * i );
			float endAngle = ( interval * ( i + 1 ) ) - gap;
			draw.CircleEx( center, radius, radius - 1f, startAngle: startAngle, endAngle: endAngle );
		}
	}
}
