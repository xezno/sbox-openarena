namespace OpenArena;

[Library( "oa_crosshair_dot" )]
public class DotCrosshair : ICrosshair
{
	void ICrosshair.RenderHud( TimeSince timeSinceAttack, Vector2 screenSize )
	{
		var draw = Render.Draw2D;
		var center = screenSize / 2.0f;

		//
		// Properties
		//
		draw.Color = Color.White;

		float t = timeSinceAttack.Relative.LerpInverse( 0, 0.5f );
		t = Easing.EaseOut( t );

		draw.Color = draw.Color.WithAlpha( t );

		// Dot
		draw.Circle( center, 1f );
	}
}
