namespace OpenArena;

partial class QuakeWalkController
{
	private int line = 0;

	private void LogToScreen( string text )
	{
		if ( !Debug )
			return;

		string realm = Pawn.IsClient ? "CL" : "SV";
		float starty = Pawn.IsClient ? 150 : 250;

		var pos = new Vector2( 760, starty + ( line++ * 16 ) );
		DebugOverlay.ScreenText( $"{realm}: {text}", pos );
	}
}
