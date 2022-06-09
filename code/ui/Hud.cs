namespace OpenArena;

public class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( !IsClient )
			return;

		RootPanel.SetTemplate( "ui/Hud.html" );
	}
}
