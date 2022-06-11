namespace OpenArena;

partial class QuakeWalkController
{
	public bool SlideMove( bool gravity )
	{
		int bumpCount;
		Vector3 primalVelocity = Velocity;
		Vector3 endVelocity = new();

		if ( gravity )
		{
			endVelocity = Velocity;
			endVelocity.z -= Gravity * Time.Delta;
			Velocity = Velocity.WithZ( ( Velocity.z + endVelocity.z ) * 0.5f );
			primalVelocity.z = endVelocity.z;

			if ( GroundPlane )
			{
				// Slide along the ground plane
				Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );
			}
		}

		float timeLeft = Time.Delta;
		float travelFraction = 0;
		bool HitWall = false;

		using var moveplanes = new VelocityClipPlanes( Velocity );

		for ( bumpCount = 0; bumpCount < moveplanes.Max; bumpCount++ )
		{
			if ( Velocity.Length.AlmostEqual( 0.0f ) )
				break;

			var trace = TraceBBox( Position, Position + Velocity * timeLeft );
			travelFraction += trace.Fraction;

			if ( trace.Fraction > 0.03125f )
			{
				Position = trace.EndPosition + trace.Normal * 0.001f;

				if ( trace.Fraction == 1 )
					break;

				moveplanes.StartBump( Velocity );
			}

			if ( bumpCount == 0 && trace.Hit && trace.Normal.Angle( Vector3.Up ) >= MinWalkNormal )
			{
				HitWall = true;
			}

			timeLeft -= timeLeft * trace.Fraction;

			Vector3 vel = endVelocity;
			if ( !moveplanes.TryAdd( trace.Normal, ref vel, ( HitWall ) ? 0.0f : 0.0f ) )
			{
				LogToScreen( $"MovePlanes: {Velocity} -> {vel}" );
				endVelocity = vel;

				Velocity = endVelocity;
				break;
			}
			endVelocity = vel;
			Velocity = endVelocity;
		}

		if ( travelFraction == 0 )
			Velocity = 0;

		if ( gravity )
			Velocity = endVelocity;

		LogToScreen( $"Bumps: {bumpCount}" );

		return bumpCount != 0;
	}
	public void StepSlideMove( bool gravity )
	{
		Vector3 start_o = Position;
		Vector3 start_v = Velocity;

		if ( !SlideMove( gravity ) )
		{
			LogToScreen( "SlideMove got exactly where we wanted to go first try" );
			return; // We got exactly where we wanted to go first try
		}

		Vector3 down = start_o;
		down.z -= StepSize;

		var trace = TraceBBox( start_o, down );
		Vector3 up = new Vector3( 0, 0, 1 );

		// never step up when you still have up velocity
		if ( Velocity.z > 0 && ( trace.Fraction == 1.0f || trace.Normal.Dot( up ) < 0.7f ) )
			return;

		up = start_o;
		up.z += StepSize;

		trace = TraceBBox( start_o, down );
		if ( trace.StartedSolid )
		{
			// cant step up
			LogToScreen( $"Can't step up" );
			return;
		}

		// try slidemove from this position
		Position = trace.EndPosition;
		Velocity = start_v;

		SlideMove( gravity );
		Velocity = ClipVelocity( Velocity, trace.Normal, Overclip );
	}

}
