namespace OpenArena;

public class PredictedTrigger : BaseTrigger
{
	[ConVar.Client( "oa_draw_trigger_bounds" )] public static bool DrawBounds { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouch = true;
		EnableTouchPersists = true;
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( !DrawBounds )
			return;

		DebugOverlay.Box( WorldSpaceBounds.Mins, WorldSpaceBounds.Maxs );
	}

	public virtual void PredictedStartTouch( Player player )
	{

	}

	public virtual void PredictedEndTouch( Player player )
	{

	}

	public virtual void PredictedTouch( Player player )
	{

	}
}
