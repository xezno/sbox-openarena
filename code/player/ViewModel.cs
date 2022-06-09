namespace OpenArena;

public class ViewModel : BaseViewModel
{
	private const float DefaultFov = 70f;

	private float Fov = DefaultFov;
	private Vector3 Offset;

	private float TargetFov = DefaultFov;
	private Vector3 TargetOffset;

	private Rotation TargetRotation;

	private float WalkBob;
	private float LerpRate = 10f;

	// ============================================================

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		TargetRotation = Rotation.Lerp( TargetRotation, camSetup.Rotation, 33f * Time.Delta );
		Rotation = Rotation.Lerp( camSetup.Rotation, TargetRotation, 0.1f );

		TargetFov = DefaultFov;

		BuildWalkEffects( ref camSetup );
		ApplyEffects( ref camSetup );

		DebugOverlay.ScreenText( "[VIEWMODEL]\n" +
			$"TargetOffset:                {TargetOffset}\n" +
			$"Position:                    {Position}\n" +
			$"Fov:                         {Fov}\n" +
			$"LerpRate:                    {LerpRate}\n" +
			$"Rotation:                    {Rotation}",
			new Vector2( 60, 250 ) );
	}

	private void ApplyEffects( ref CameraSetup camSetup )
	{
		Fov = Fov.LerpTo( TargetFov, LerpRate * Time.Delta );
		camSetup.ViewModel.FieldOfView = Fov;

		Offset = Offset.LerpTo( TargetOffset, LerpRate * Time.Delta );
		Position += -Offset * Rotation;
	}

	private void BuildWalkEffects( ref CameraSetup camSetup )
	{
		if ( Owner is Player player )
		{
			float factor = 2.0f;

			var speed = player.Velocity.Length;
			float t = speed.LerpInverse( 0, 310 );

			TargetOffset = new Vector3( t, 0, t / 2f ) * factor;

			var left = camSetup.Rotation.Left;
			var up = camSetup.Rotation.Up;

			if ( Owner.GroundEntity != null )
				WalkBob += Time.Delta * 20.0f * t;

			TargetOffset += up * MathF.Sin( WalkBob ) * t * -0.5f * factor;
			TargetOffset += left * MathF.Sin( WalkBob * 0.5f ) * t * -0.25f * factor;
		}
	}
}
