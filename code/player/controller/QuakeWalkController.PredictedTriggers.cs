namespace OpenArena;

partial class QuakeWalkController
{
	private List<PredictedTrigger> TouchingTriggers = new();

	private void UpdatePredictedTriggers()
	{
		if ( Pawn is not Player player )
			return;

		var currentlyTouching = Entity.All.OfType<PredictedTrigger>()
			.Where( x => x.WorldSpaceBounds.Overlaps( player.WorldSpaceBounds ) )
			.ToList();

		foreach ( var trigger in currentlyTouching )
		{
			if ( !TouchingTriggers.Contains( trigger ) )
				trigger.PredictedStartTouch( player );
			else
				trigger.PredictedTouch( player );
		}

		foreach ( var trigger in TouchingTriggers )
		{
			if ( !currentlyTouching.Contains( trigger ) )
				trigger.PredictedEndTouch( player );
		}

		TouchingTriggers = currentlyTouching.ToList();
	}
}
