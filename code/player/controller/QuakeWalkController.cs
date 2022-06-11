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

	//
	// Movement parameters
	//
	float StopSpeed => 100.0f;
	float DuckScale => 0.25f;
	float GroundDistance => 1;
	float Acceleration => 10.0f;
	float AirAcceleration => 2.0f;
	float Friction => 6.0f;
	float Speed => 320.0f;
	float AirSpeed => 180.0f;
	float Gravity => 800f;

	// Can't walk on very steep slopes
	float MinWalkNormal => 0.7f;
	float StepSize => 18;
	float JumpVelocity => 270;
	float Overclip => 1.001f;

	private Vector3 mins;
	private Vector3 maxs;

	private bool Walking { get; set; }
	private bool GroundPlane { get; set; }
	private TraceResult GroundTrace { get; set; }
	private Unstuck Unstuck { get; set; }
	private Duck Duck { get; set; }

	public QuakeWalkController()
	{
		Duck = new( this );
		Unstuck = new Unstuck( this );
	}

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

		Duck.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

		SetBBox( mins, maxs );
	}

	public override void Simulate()
	{
		EyeRotation = Input.Rotation;
		EyeLocalPosition = Vector3.Up * ( EyeHeight * Pawn.Scale );

		// debug
		line = 0;

		// ducking
		Duck.PreTick();

		UpdateBBox();

		// set groundentity
		TraceToGround();

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
		TraceToGround();

		// check if we're stuck and fix that
		Unstuck.TestAndFix();

		// stay on ground

		// if ( Debug )
		{
			DebugOverlay.ScreenText( $"[QUAKE WALK CONTROLLER]\n" +
				$"Velocity:                    {Velocity}\n" +
				$"Vel length:                  {Velocity.Length}\n" +
				$"Position:                    {Position}\n" +
				$"GroundEntity:                {GroundEntity}",
				new Vector2( 360, 150 ) );
		}
	}

	private Vector3 ClipVelocity( Vector3 inVec, Vector3 normal, float overbounce )
	{
		float backoff = inVec.Dot( normal );

		if ( backoff < 0 )
			backoff *= overbounce;
		else
			backoff /= overbounce;

		Vector3 outVec = inVec - ( normal * backoff );
		return outVec;
	}

	public void ApplyFriction()
	{
		if ( GroundEntity == null )
			return;

		Vector3 vec = Velocity;

		if ( Walking )
			vec.z = 0;

		float speed = vec.Length;
		float control;

		if ( speed < 1 )
		{
			Vector3 vel = new( Velocity );
			vel.x = 0;
			vel.y = 0;
			Velocity = vel;
			return;
		}

		float drop = 0;

		if ( Walking )
		{
			control = speed < StopSpeed ? StopSpeed : speed;
			drop += control * Friction * Time.Delta;
		}

		float newspeed = speed - drop;

		if ( newspeed < 0 )
			newspeed = 0;

		newspeed /= speed;

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

		Velocity = Velocity.WithZ( JumpVelocity );

		SetGroundEntity( null );

		return true;
	}

	public void AirMove()
	{
		ApplyFriction();

		Vector3 wishDir = GetWishDirection();
		float wishSpeed = GetWishSpeed();

		Accelerate( wishDir, wishSpeed, AirAcceleration );

		if ( GroundPlane )
		{
			Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );
		}

		StepSlideMove( true );
	}

	private Vector3 GetWishDirection()
	{
		float fMove = Input.Forward;
		float sMove = Input.Left;

		Vector3 forward = EyeRotation.Forward.WithZ( 0 );
		Vector3 left = EyeRotation.Left.WithZ( 0 );

		forward = ClipVelocity( forward, GroundTrace.Normal, Overclip );
		left = ClipVelocity( left, GroundTrace.Normal, Overclip );

		forward = forward.Normal;
		left = left.Normal;

		Vector3 wishVel = fMove * forward + sMove * left;
		wishVel.z = 0;

		return wishVel.Normal;
	}

	private float GetWishSpeed()
	{
		float duckSpeed = Duck.GetWishSpeed();

		if ( duckSpeed > 0 )
			return duckSpeed;

		if ( GroundEntity == null )
			return AirSpeed;

		return Speed;
	}

	private void WalkMove()
	{
		if ( CheckJump() )
		{
			// Jumped away
			AirMove();
			return;
		}

		ApplyFriction();

		Vector3 wishDir = GetWishDirection();
		float wishSpeed = GetWishSpeed();

		Accelerate( wishDir, wishSpeed, Acceleration );

		// Slide along the ground plane
		float vel = Velocity.Length;
		Velocity = ClipVelocity( Velocity, GroundTrace.Normal, Overclip );

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
			Log( $"Can't step up" );
			return;
		}

		// try slidemove from this position
		Position = trace.EndPosition;
		Velocity = start_v;

		SlideMove( gravity );
		Velocity = ClipVelocity( Velocity, trace.Normal, Overclip );
	}

	private bool CorrectAllSolid()
	{
		Vector3 point;

		Log( "CorrectAllSolid" );

		{
			for ( int i = -1; i <= 1; i++ )
			{
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int k = -1; k <= 1; k++ )
					{
						point = Position;
						point.x += i;
						point.y += j;
						point.z += k - 0.25f;

						var trace = TraceBBox( point, point );
						DebugOverlay.Line( Position, point );

						if ( !trace.StartedSolid )
						{
							Log( "Found space for correctallsolid" );
							DebugOverlay.Sphere( point, 2f, Color.White );

							point = Position.WithZ( Position.z - GroundDistance );
							trace = TraceBBox( Position, point );
							Position = point;
							GroundTrace = trace;

							return true;
						}
					}
				}
			}
		}

		SetGroundEntity( null );
		return false;
	}

	private void TraceToGround()
	{
		//
		// todo (AG): there's some latency here caused by some weird
		// bouncing stuff... we could probably do with changing this out
		// completely so that it's instantaneous
		//
		Vector3 point;
		TraceResult trace;

		point = new Vector3( Position ).WithZ( Position.z - GroundDistance );
		trace = TraceBBox( Position, point );

		GroundTrace = trace;

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
			SetGroundEntity( null );
		}

		// check if getting thrown off the ground
		if ( Velocity.z > 0 && Velocity.Dot( trace.Normal ) > 10.0f )
		{
			Log( $"Kickoff" );

			SetGroundEntity( null );
			return;
		}

		// slopes that are too steep will not be considered onground
		if ( trace.Entity != null && trace.Normal.z < MinWalkNormal )
		{
			Log( $"Too steep" );

			// ID FIXME: if they can't slide down the slope, let them
			// walk ( sharp crevices )

			SetGroundEntity( null );
			return;
		}

		SetGroundEntity( trace );
	}

	private void CategorizePosition( bool bStayOnGround )
	{
		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;

		bool bMoveToEndPos = false;

		if ( GroundEntity != null )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		var trace = TraceBBox( vBumpOrigin, point );

		if ( trace.Entity == null || Vector3.GetAngle( Vector3.Up, trace.Normal ) > MinWalkNormal )
		{
			SetGroundEntity( null );
			bMoveToEndPos = false;
		}
		else
		{
			SetGroundEntity( trace );
		}

		if ( bMoveToEndPos && !trace.StartedSolid && trace.Fraction > 0.0f && trace.Fraction < 1.0f )
		{
			Position = trace.EndPosition;
		}
	}

	private void SetGroundEntity( TraceResult tr ) => SetGroundEntity( tr.Entity );

	private void SetGroundEntity( Entity ent )
	{
		if ( ent == null )
		{
			GroundPlane = false;
			Walking = false;
		}
		else
		{
			GroundPlane = true;
			Walking = true;
		}

		GroundEntity = ent;
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
