namespace OpenArena;

[Library( "oa_crosshair_triangle" )]
public class TriangleCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack, Vector2 screenSize )
	{
		var draw = Render.Draw2D;
		var center = screenSize / 2.0f;

		//
		// Properties
		//
		float size = 16f;
		float thickness = 1f;
		float gap = 8f;

		draw.Color = Color.White;

		float t = timeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		gap *= 2.0f.LerpTo( 1.0f, t );

		// top left
		draw.Line( thickness, center - new Vector2( gap + size, gap + size ), center - new Vector2( gap, gap ) );
		draw.Line( thickness, center - new Vector2( -gap - size, gap + size ), center - new Vector2( -gap, gap ) );

		// S
		gap *= 1.5f;
		draw.Line( thickness, center + new Vector2( 0, gap + size ), center + new Vector2( 0, gap ) );

		// dot
		draw.Circle( center, 1f );
	}
}
