namespace OpenArena;

partial class Player
{
	private float TargetFov { get; set; }
	private float Fov { get; set; }
	private float WalkBob { get; set; }

	private float ZoomSpeed => 25.0f;

	// TODO: conditions for zooming
	public bool IsZooming => Input.Down( InputButton.SecondaryAttack );

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		var defaultFieldOfView = setup.FieldOfView;

		//
		// Camera zoom
		//
		if ( IsZooming )
			TargetFov = 50;
		else
			TargetFov = -1;

		//
		// Apply desired FOV over time
		//
		float targetFov = TargetFov;

		// Make sure we're applying an fov... if we're not,
		// revert to the default camera FOV
		if ( targetFov <= 0 )
			targetFov = defaultFieldOfView;

		Fov = Fov.LerpTo( targetFov, ZoomSpeed * Time.Delta );
		setup.FieldOfView = Fov;

		//
		// Fire PostCameraSetup on active weapon so that it can do any custom stuff too
		//
		if ( ActiveChild != null )
		{
			ActiveChild.PostCameraSetup( ref setup );
		}

		//
		// View bobbing
		//
		var speed = Velocity.Length;
		float t = speed.LerpInverse( 0, 310 );

		if ( GroundEntity != null )
			WalkBob += Time.Delta * 20.0f * t;

		var offset = Bobbing.CalculateOffset( WalkBob, t, 2.0f ) * setup.Rotation;
		setup.Position += offset;
	}
}
