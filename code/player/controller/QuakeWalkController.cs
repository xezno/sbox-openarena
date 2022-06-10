namespace OpenArena;

//
// This isn't exactly like the Quake movement controller
// ( some quirks have been tweaked or removed ) but it's much
// closer to arena shooter movement than the default WalkController
//
//
// https://github.com/id-Software/Quake-III-Arena/blob/master/code/game/bg_pmove.c
//
public class QuakeWalkController : BasePlayerController
{
	public float BodyGirth => 32f;
	public float BodyHeight => 72f;
	public float EyeHeight => 64f;

	// movement parameters
	float pm_stopspeed => 100.0f;
	float pm_duckScale => 0.25f;
	float pm_swimScale => 0.50f;
	float pm_wadeScale => 0.70f;

	float pm_accelerate => 10.0f;
	float pm_airaccelerate => 1.0f;
	float pm_wateraccelerate => 4.0f;
	float pm_flyaccelerate => 8.0f;

	float pm_friction => 6.0f;
	float pm_waterfriction => 1.0f;
	float pm_flightfriction => 3.0f;
	float pm_spectatorfriction => 5.0f;

	float pm_speed => 320.0f;
	float pm_airspeed => 180.0f;
	float pm_gravity => 800f;

	float MIN_WALK_NORMAL => 0.7f;     // can't walk on very steep slopes
	float STEPSIZE => 18;
	float JUMP_VELOCITY => 270;
	float TIMER_LAND => 130;
	float TIMER_GESTURE => ( 34 * 66 + 50 );
	float OVERCLIP => 1.001f;

	int c_pmove => Pawn.NetworkIdent;

	private Vector3 mins;
	private Vector3 maxs;

	struct pml_t
	{
		public Vector3 forward, right, up;

		public bool walking;
		public bool groundPlane;
		public TraceResult groundTrace;

		public float impactSpeed;

		public Vector3 previousOrigin;
		public Vector3 previousVelocity;
		public int previousWaterLevel;
	}

	pml_t pml;

	public override void FrameSimulate()
	{
		EyeRotation = Input.Rotation;
	}

	public virtual void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	public virtual void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;

		var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
		var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

		SetBBox( mins, maxs );
	}

	public override void Simulate()
	{
		EyeRotation = Input.Rotation;
		EyeLocalPosition = Vector3.Up * ( EyeHeight * Pawn.Scale );

		// debug
		line = 0;

		// pml
		pml.forward = EyeRotation.Forward;
		pml.right = EyeRotation.Left;

		UpdateBBox();

		// set groundentity
		GroundTrace();

		if ( GroundEntity != null )
		{
			// walking on ground
			WalkMove();
		}
		else
		{
			// in air
			AirMove();
		}

		// set groundentity
		GroundTrace();

		// if ( Debug )
		{
			DebugOverlay.ScreenText( $"[QUAKE WALK CONTROLLER]\n" +
				$"Velocity:                    {Velocity}\n" +
				$"Position:                    {Position}\n" +
				$"GroundEntity:                {GroundEntity}",
				new Vector2( 360, 150 ) );
		}
	}

	private Vector3 ClipVelocity( Vector3 inVec, Vector3 normal, float overbounce )
	{
		Vector3 outVec = new();

		float backoff = inVec.Dot( normal );

		if ( backoff < 0 )
		{
			backoff *= overbounce;
		}
		else
		{
			backoff /= overbounce;
		}

		for ( int i = 0; i < 3; i++ )
		{
			float change = normal[i] * backoff;
			outVec[i] = inVec[i] - change;
		}

		return outVec;
	}

	public void Friction()
	{
		if ( GroundEntity == null )
			return;

		Vector3 vec;
		float speed, newspeed, control;

		float drop;

		vec = Velocity;

		if ( pml.walking )
		{
			vec.z = 0;
		}

		speed = vec.Length;

		if ( speed < 1 )
		{
			Vector3 vel = new( Velocity );
			vel.x = 0;
			vel.y = 0;
			Velocity = vel;
			return;
		}

		drop = 0;

		if ( pml.walking )
		{
			control = speed < pm_stopspeed ? pm_stopspeed : speed;
			drop += control * pm_friction * Time.Delta;
		}

		newspeed = speed - drop;

		if ( newspeed < 0 )
		{
			newspeed = 0;
		}

		newspeed /= speed;

		Log( newspeed.ToString() );
		Velocity *= newspeed;
	}

	public void Accelerate( Vector3 wishDir, float wishSpeed, float accel )
	{
		float addspeed, accelspeed, currentspeed;

		currentspeed = Velocity.Dot( wishDir );
		addspeed = wishSpeed - currentspeed;

		if ( addspeed <= 0 )
		{
			return;
		}

		accelspeed = accel * Time.Delta * wishSpeed;

		if ( accelspeed > addspeed )
		{
			accelspeed = addspeed;
		}

		Velocity += accelspeed * wishDir;
	}

	public bool CheckJump()
	{
		if ( GroundEntity == null )
			return false;

		if ( !Input.Down( InputButton.Jump ) )
			return false;

		Velocity = Velocity.WithZ( 270 );

		pml.groundPlane = false;
		pml.walking = false;

		return true;
	}

	public void AirMove()
	{
		Log( "AirMove" );
		Vector3 wishVel;

		Friction();

		float fMove = Input.Forward;
		float sMove = Input.Left;

		pml.forward.z = 0;
		pml.right.z = 0;

		pml.forward = pml.forward.Normal;
		pml.right = pml.right.Normal;

		wishVel = pml.forward * fMove + pml.right * sMove;
		wishVel.z = 0;

		Vector3 wishDir = wishVel.Normal;
		float wishSpeed = wishDir.Length;
		wishSpeed *= pm_airspeed;

		Accelerate( wishDir, wishSpeed, pm_airaccelerate );

		if ( pml.groundPlane )
		{
			Velocity = ClipVelocity( Velocity, pml.groundTrace.Normal, OVERCLIP );
		}

		StepSlideMove( true );
	}

	private void WalkMove()
	{
		if ( CheckJump() )
		{
			// Jumped away
			AirMove();
			return;
		}

		Log( "WalkMove" );

		Friction();

		float fMove = Input.Forward;
		float sMove = Input.Left;

		pml.forward.z = 0;
		pml.right.z = 0;

		pml.forward = ClipVelocity( pml.forward, pml.groundTrace.Normal, OVERCLIP );
		pml.right = ClipVelocity( pml.right, pml.groundTrace.Normal, OVERCLIP );

		pml.forward = pml.forward.Normal;
		pml.right = pml.right.Normal;

		Vector3 wishVel = fMove * pml.forward + sMove * pml.right;

		Vector3 wishDir = wishVel.Normal;
		float wishSpeed = wishDir.Length;
		wishSpeed *= pm_speed;

		Accelerate( wishDir, wishSpeed, pm_accelerate );

		// Slide along the ground plane
		float vel = Velocity.Length;
		Velocity = ClipVelocity( Velocity, pml.groundTrace.Normal, OVERCLIP );

		// Don't decreate velocity when going up or down a slope
		Velocity = Velocity.Normal;
		Velocity = Velocity * vel;

		// Don't do anything if standing still
		if ( Velocity.Length.AlmostEqual( 0 ) )
		{
			return;
		}

		StepSlideMove( false );
	}

	public bool SlideMove( bool gravity )
	{
		int bumpCount = 0;
		int numplanes = 0;
		int numbumps = 4;
		Vector3[] planes = new Vector3[6];
		Vector3 primalVelocity = Velocity;
		Vector3 endVelocity = new();

		if ( gravity )
		{
			endVelocity = Velocity;
			endVelocity.z -= pm_gravity * Time.Delta;
			Velocity = Velocity.WithZ( ( Velocity.z + endVelocity.z ) * 0.5f );
			primalVelocity.z = endVelocity.z;

			if ( pml.groundPlane )
			{
				// Slide along the ground plane
				Velocity = ClipVelocity( Velocity, pml.groundTrace.Normal, OVERCLIP );
			}
		}

		float timeLeft = Time.Delta;

		// Never turn against the ground plane
		if ( pml.groundPlane )
		{
			numplanes = 1;
			planes[0] = pml.groundTrace.Normal;
		}
		else
		{
			numplanes = 0;
		}

		// Never turn against original velocity
		planes[numplanes] = Velocity.Normal;
		numplanes++;

		for ( bumpCount = 0; bumpCount < numbumps; bumpCount++ )
		{
			// Calculate position we are trying to move to
			Vector3 end = VectorMA( Position, timeLeft, Velocity );

			// See if we can make it there
			var trace = Trace( Position, mins, maxs, end );

			DebugOverlay.Line( Position, end );

			if ( trace.StartedSolid )
			{
				Log( "entity is completely trapped in another solid" );
				// entity is completely trapped in another solid
				Velocity = Velocity.WithZ( 0 );
				return true;
			}

			if ( trace.Fraction > 0 )
			{
				// actually covered some distance
				Log( "actually covered some distance" );
				Position = trace.EndPosition;
			}

			if ( trace.Fraction == 1 )
			{
				Log( "moved the entire distance" );
				break; // moved the entire distance
			}

			timeLeft -= timeLeft * trace.Fraction;

			//
			// if this is the same plane we hit before, nudge velocity
			// out along it, which fixes some epsilon issues with
			// non-axial planes
			//

			int i = 0;
			for ( i = 0; i < numplanes; i++ )
			{
				if ( trace.Normal.Dot( planes[i] ) > 0.99f )
				{
					Velocity = trace.Normal + Velocity;
				}
			}

			if ( i < numplanes )
				continue;

			planes[numplanes] = trace.Normal;
			numplanes++;

			//
			// modify velocity so it parallels all of the clip planes
			//

			// find a plane that it enters
			for ( i = 0; i < numplanes; i++ )
			{
				float into = Velocity.Dot( planes[i] );
				if ( into >= 0.1f )
				{
					continue; // move doesn't interact with the plane
				}

				// see how hard we are hitting things
				if ( -into > pml.impactSpeed )
				{
					pml.impactSpeed = -into;
				}

				// slide along the plane
				Vector3 clipVelocity = ClipVelocity( Velocity, planes[i], OVERCLIP );

				// slide along the plane
				Vector3 endClipVelocity = ClipVelocity( endVelocity, planes[i], OVERCLIP );

				// see if there is a second plane that the new move enters
				for ( int j = 0; j < numplanes; j++ )
				{
					if ( j == i )
						continue;

					if ( clipVelocity.Dot( planes[j] ) >= 0.1f )
						continue;// move doesn't interact with the plane

					// try clipping the move to the plane
					clipVelocity = ClipVelocity( clipVelocity, planes[i], OVERCLIP );
					endClipVelocity = ClipVelocity( endClipVelocity, planes[i], OVERCLIP );

					// see if it goes back into the first clip plane
					if ( clipVelocity.Dot( planes[i] ) >= 0 )
						continue;

					// slide the original velocity along the crease
					Vector3 dir = planes[i].Cross( planes[j] ).Normal;
					float d = dir.Dot( Velocity );
					clipVelocity = dir * d;


					dir = planes[i].Cross( planes[j] ).Normal;
					d = dir.Dot( endVelocity );
					endClipVelocity = dir * d;

					// see if there is a third plane that the new move enters
					for ( int k = 0; k < numplanes; k++ )
					{
						if ( k == i || k == j )
							continue;

						if ( clipVelocity.Dot( planes[k] ) >= 0.1f )
							continue; // move dont interact with plane

						// stop dead at triple plane interaction
						Velocity = 0;
						return true;
					}
				}

				// if we have fixed all interactions, try another move
				Velocity = clipVelocity;
				endVelocity = endClipVelocity;

				break;
			}
		}

		if ( gravity )
			Velocity = endVelocity;

		Log( $"Bumps: {bumpCount}" );

		return bumpCount != 0;
	}

	private TraceResult Trace( Vector3 position, Vector3 mins, Vector3 maxs, Vector3 end )
	{
		var tr = Sandbox.Trace.Ray( position, end )
			.Ignore( Pawn )
			.Size( mins, maxs )
			.Run();

		return tr;
	}

	private Vector3 VectorMA( Vector3 veca, float scale, Vector3 vecb )
	{
		Vector3 vecc = new();

		vecc.x = veca.x + scale * vecb.x;
		vecc.y = veca.y + scale * vecb.y;
		vecc.z = veca.z + scale * vecb.z;

		return vecc;
	}

	public void StepSlideMove( bool gravity )
	{
		Vector3 start_o = Position;
		Vector3 start_v = Velocity;

		if ( !SlideMove( gravity ) )
		{
			Log( "SlideMove Success" );
			return; // We got exactly where we wanted to go first try
		}

		Vector3 down = start_o;

		down.z -= STEPSIZE;

		var trace = Trace( start_o, mins, maxs, down );
		Vector3 up = new Vector3( 0, 0, 1 );

		// never step up when you still have up velocity
		if ( Velocity.z > 0 && ( trace.Fraction == 1.0f || trace.Normal.Dot( up ) < 0.7f ) )
			return;

		Vector3 down_o = Position;
		Vector3 down_v = Velocity;

		up = start_o;

		up.z += STEPSIZE;

		trace = Trace( start_o, mins, maxs, down );
		if ( trace.StartedSolid )
		{
			Log( $"{c_pmove}: bend can't step" );

			return;// cant step up
		}

		float stepSize = trace.EndPosition.z - start_o.z;
		// try slidemove from this position
		Position = trace.EndPosition;
		Velocity = start_v;

		SlideMove( gravity );

		// push down the final amount
		down = Position;
		down.z -= stepSize;

		trace = Trace( Position, mins, maxs, down );

		if ( !trace.StartedSolid )
		{
			Position = trace.EndPosition;
		}

		if ( trace.Fraction < 1.0f )
		{
			Velocity = ClipVelocity( Velocity, trace.Normal, OVERCLIP );
		}
	}

	private bool CorrectAllSolid( TraceResult trace )
	{
		Vector3 point;

		Log( $"{c_pmove}: allsolid" );

		for ( int i = -1; i <= 1; i++ )
		{
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int k = -1; k <= 1; k++ )
				{
					point = Position;
					point.x += i * 8f;
					point.y += j * 8f;
					point.z += k * 8f;

					trace = Trace( point, mins, maxs, point );
					DebugOverlay.Line( Position, point );

					if ( !trace.StartedSolid )
					{
						Log( "Found space for unstuck" );
						DebugOverlay.Sphere( point, 2f, Color.White );

						//point = Position.WithZ( Position.z - 0.25f );
						//trace = Trace( Position, mins, maxs, point );
						Position = point;
						pml.groundTrace = trace;

						return true;
					}
				}
			}
		}

		GroundEntity = null;
		pml.groundPlane = false;
		pml.walking = false;

		return false;
	}

	private void GroundTraceMissed()
	{
		GroundEntity = null;
		pml.groundPlane = false;
		pml.walking = false;
	}

	private void GroundTrace()
	{
		Vector3 point;
		TraceResult trace;

		point = new Vector3( Position ).WithZ( Position.z - 0.25f );
		trace = Trace( Position, mins, maxs, point );

		pml.groundTrace = trace;

		// do something corrective if the trace starts in a solid...
		if ( trace.StartedSolid )
		{
			if ( CorrectAllSolid( trace ) )
				return;
		}

		// if the trace didn't hit anything, we are in free fall
		if ( trace.Fraction == 1.0f )
		{
			GroundTraceMissed();
			pml.groundPlane = false;
			pml.walking = false;
		}

		// check if getting thrown off the ground
		if ( Velocity.z > 0 && Velocity.Dot( trace.Normal ) > 10.0f )
		{
			Log( $"{c_pmove}: kickoff" );

			GroundEntity = null;
			pml.groundPlane = false;
			pml.walking = false;

			return;
		}

		// slopes that are too steep will not be considered onground
		if ( trace.Entity != null && trace.Normal.z < MIN_WALK_NORMAL )
		{
			Log( $"{c_pmove}: steep" );

			// ID FIXME: if they can't slide down the slope, let them
			// walk ( sharp crevices )

			GroundEntity = null;
			pml.groundPlane = true;
			pml.walking = false;
			return;
		}

		pml.groundPlane = true;
		pml.walking = true;
		GroundEntity = trace.Entity;
	}

	int line = 0;
	private void Log( string text )
	{
		string realm = Pawn.IsClient ? "CL" : "SV";
		float starty = Pawn.IsClient ? 150 : 250;

		var pos = new Vector2( 760, starty + ( line++ * 16 ) );
		DebugOverlay.ScreenText( $"{realm}: {text}", pos );
	}
}
