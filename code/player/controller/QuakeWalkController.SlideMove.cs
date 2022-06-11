namespace OpenArena;

partial class QuakeWalkController
{
	private bool SlideMove( bool gravity )
	{
		int bumpCount;
		Vector3 endVelocity = new();

		// gravity end
		if ( gravity )
		{
			endVelocity = Velocity;
			endVelocity.z -= Gravity * Time.Delta * 0.5f;
			Velocity = Velocity.WithZ( ( Velocity.z + endVelocity.z ) * 0.5f );
			LogToScreen( $"SlideMove: {Velocity} -> {endVelocity}" );

			if ( GroundPlane )
			{
				// Slide along the ground plane
				Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );
				LogToScreen( $"SlideMove GroundPlane: {endVelocity} -> {Velocity}" );
			}
		}

		float timeLeft = Time.Delta;
		float travelFraction = 0;

		using var moveplanes = new VelocityClipPlanes( Velocity );

		for ( bumpCount = 0; bumpCount < moveplanes.Max; bumpCount++ )
		{
			if ( Velocity.Length.AlmostEqual( 0.0f ) )
				break;

			var trace = TraceBBox( Position, Position + Velocity * timeLeft );
			travelFraction += trace.Fraction;

			if ( trace.StartedSolid )
			{
				Velocity = endVelocity.WithZ( 0 );
				return true;
			}

			if ( trace.Fraction > 0.03125f )
			{
				Position = trace.EndPosition + trace.Normal * 0.001f;

				if ( trace.Fraction == 1 )
					break;

				moveplanes.StartBump( Velocity );
			}

			timeLeft -= timeLeft * trace.Fraction;

			Vector3 vel = endVelocity;
			if ( !moveplanes.TryAdd( trace.Normal, ref vel, 0.0f ) )
			{
				endVelocity = vel;

				Velocity = endVelocity;
				break;
			}
			LogToScreen( $"MovePlanes: {Velocity} -> {vel}" );
			endVelocity = vel;
			Velocity = endVelocity;
		}

		if ( gravity )
			Velocity = endVelocity;

		LogToScreen( $"Bumps: {bumpCount}" );

		return bumpCount != 0;
	}
	private void StepSlideMove( bool gravity )
	{
		var startPos = Position;
		var startVel = Velocity;

		if ( !SlideMove( gravity ) )
		{
			LogToScreen( "SlideMove got exactly where we wanted to go first try" );
			return; // We got exactly where we wanted to go first try
		}

		Vector3 down = startPos;
		down.z -= StepSize;

		var trace = TraceBBox( startPos, down );
		Vector3 up = new Vector3( 0, 0, 1 );

		// never step up when you still have up velocity
		if ( Velocity.z > 0 && ( trace.Fraction == 1.0f || trace.Normal.Dot( up ) < 0.7f ) )
			return;

		up = startPos;
		up.z += StepSize;

		// test the player position if they were a stepheight higher
		trace = TraceBBox( startPos, up );

		if ( trace.StartedSolid )
		{
			// cant step up
			LogToScreen( $"Can't step up" );
			return;
		}

		float stepSize = trace.EndPosition.z - startPos.z;
		Position = trace.EndPosition;
		Velocity = startVel;

		SlideMove( gravity );

		// push down the final amount
		down = Position;
		down.z -= stepSize;
		trace = TraceBBox( Position, down );

		if ( !trace.StartedSolid )
		{
			Position = trace.EndPosition;
		}

		if ( trace.Fraction < 1.0f )
		{
			Velocity = ClipVelocity( Velocity, trace.Normal, Overclip );
		}
	}
}
