namespace OpenArena;

partial class QuakeWalkController
{
	public float BodyGirth => 32f;
	public float BodyHeight => 72f;
	public float EyeHeight => 64f;

	//
	// Movement parameters
	//
	float StopSpeed => 100.0f;
	float DuckScale => 0.25f;
	float GroundDistance => 0.25f;
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
}
