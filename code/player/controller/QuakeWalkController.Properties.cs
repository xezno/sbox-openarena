namespace OpenArena;

partial class QuakeWalkController
{
	public float BodyGirth => 32f;
	public float BodyHeight => 72f;
	public float EyeHeight => 64f;

	//
	// Movement parameters
	//
	[ConVar.Replicated( "oa_stopspeed" )]
	public static float StopSpeed { get; set; } = 100.0f;

	[ConVar.Replicated( "oa_grounddistance" )]
	public static float GroundDistance { get; set; } = 0.25f;

	[ConVar.Replicated( "oa_acceleration" )]
	public static float Acceleration { get; set; } = 10.0f;

	[ConVar.Replicated( "oa_airacceleration" )]
	public static float AirAcceleration { get; set; } = 10.0f;

	[ConVar.Replicated( "oa_friction" )]
	public static float Friction { get; set; } = 6.0f;

	[ConVar.Replicated( "oa_speed" )]
	public static float Speed { get; set; } = 320.0f;

	[ConVar.Replicated( "oa_airspeed" )]
	public static float AirSpeed { get; set; } = 180.0f;

	[ConVar.Replicated( "oa_aircontrol" )]
	public static float AirControl { get; set; } = 50.0f;

	[ConVar.Replicated( "oa_gravity" )]
	public static float Gravity { get; set; } = 800f;

	[ConVar.Replicated( "oa_maxwalkangle" )]
	public static float MaxWalkAngle { get; set; } = 45f;

	[ConVar.Replicated( "oa_stepsize" )]
	public static float StepSize { get; set; } = 18;

	[ConVar.Replicated( "oa_jumpvelocity" )]
	public static float JumpVelocity { get; set; } = 270;

	[ConVar.Replicated( "oa_overclip" )]
	public static float Overclip { get; set; } = 1.001f;

	public enum JumpModes
	{
		AutoBhop,
		QueueJump,
		Vanilla
	}

	[ConVar.Replicated( "oa_jumpmode" )]
	public static JumpModes JumpMode { get; set; } = JumpModes.AutoBhop;

	public enum AccelModes
	{
		Quake2,
		NoStrafeJump
	}

	[ConVar.Replicated( "oa_accelmode" )]
	public static AccelModes AccelMode { get; set; } = AccelModes.Quake2;
}
