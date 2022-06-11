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

	float pm_groundDistance => 1;

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

		// stick to ground
		CategorizePosition( GroundEntity != null );

		// set groundentity
		GroundTrace();

		// check if we're stuck and fix that
		Unstuck();

		// stay on ground

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

		outVec = inVec - ( normal * backoff );
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
		int bumpCount;
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

			if ( bumpCount == 0 && trace.Hit && trace.Normal.Angle( Vector3.Up ) >= MIN_WALK_NORMAL )
			{
				HitWall = true;
			}

			timeLeft -= timeLeft * trace.Fraction;

			Vector3 vel = endVelocity;
			if ( !moveplanes.TryAdd( trace.Normal, ref vel, ( HitWall ) ? 0.0f : 0.0f ) )
			{
				Log( $"MovePlanes: {Velocity} -> {vel}" );
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

		Log( $"Bumps: {bumpCount}" );

		return bumpCount != 0;
	}

	public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0 )
	{
		return base.TraceBBox( start, end, mins, maxs, liftFeet );
	}

	public void StepSlideMove( bool gravity )
	{
		Vector3 start_o = Position;
		Vector3 start_v = Velocity;

		if ( !SlideMove( gravity ) )
		{
			Log( "SlideMove got exactly where we wanted to go first try" );
			return; // We got exactly where we wanted to go first try
		}

		Vector3 down = start_o;
		down.z -= STEPSIZE;

		var trace = TraceBBox( start_o, down );
		Vector3 up = new Vector3( 0, 0, 1 );

		// never step up when you still have up velocity
		if ( Velocity.z > 0 && ( trace.Fraction == 1.0f || trace.Normal.Dot( up ) < 0.7f ) )
			return;

		up = start_o;
		up.z += STEPSIZE;

		trace = TraceBBox( start_o, down );
		if ( trace.StartedSolid )
		{
			// cant step up
			Log( $"Can't step up" );
			return;
		}

		// try slidemove from this position
		Position = trace.EndPosition;
		Velocity = start_v;

		SlideMove( gravity );
		Velocity = ClipVelocity( Velocity, trace.Normal, OVERCLIP );
	}

	private bool CorrectAllSolid()
	{
		Vector3 point;

		Log( "CorrectAllSolid" );

		for ( int i = -1; i <= 1; i++ )
		{
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int k = -1; k <= 1; k++ )
				{
					point = Position;
					point.x += i;
					point.y += j;
					point.z += k;

					var trace = TraceBBox( point, point );
					DebugOverlay.Line( Position, point );

					if ( !trace.StartedSolid )
					{
						Log( "Found space for unstuck" );
						DebugOverlay.Sphere( point, 2f, Color.White );

						point = Position.WithZ( Position.z - pm_groundDistance );
						trace = TraceBBox( Position, point );
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

	public bool Unstuck()
	{
		var tr = TraceBBox( Position, Position );
		if ( !tr.StartedSolid )
			return true;

		return CorrectAllSolid();
	}

	private void GroundTrace()
	{
		//
		// todo (AG): there's some latency here caused by some weird
		// bouncing stuff... we could probably do with changing this out
		// completely so that it's instantaneous
		//
		Vector3 point;
		TraceResult trace;

		point = new Vector3( Position ).WithZ( Position.z - pm_groundDistance );
		trace = TraceBBox( Position, point );

		pml.groundTrace = trace;

		// do something corrective if the trace starts in a solid...
		if ( trace.StartedSolid )
		{
			Log( "do something corrective if the trace starts in a solid..." );
			if ( CorrectAllSolid() )
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

	private void CategorizePosition( bool bStayOnGround )
	{
		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;

		bool bMoveToEndPos = false;

		if ( GroundEntity != null )
		{
			bMoveToEndPos = true;
			point.z -= STEPSIZE;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= STEPSIZE;
		}

		var trace = TraceBBox( vBumpOrigin, point );

		if ( trace.Entity == null || Vector3.GetAngle( Vector3.Up, trace.Normal ) > MIN_WALK_NORMAL )
		{
			GroundTraceMissed();
			bMoveToEndPos = false;
		}
		else
		{
			GroundEntity = trace.Entity;
			pml.groundPlane = true;
			pml.walking = false;
		}

		if ( bMoveToEndPos && !trace.StartedSolid && trace.Fraction > 0.0f && trace.Fraction < 1.0f )
		{
			Position = trace.EndPosition;
		}
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
