namespace OpenArena;

public static class Bobbing
{
	public static Vector3 CalculateOffset( float walkBob, float t, float factor = 1.0f )
	{
		Vector3 TargetOffset = new();

		TargetOffset += Vector3.Up * MathF.Sin( walkBob ) * t * -0.5f * factor;
		TargetOffset += Vector3.Left * MathF.Sin( walkBob * 0.5f ) * t * -0.25f * factor;

		return TargetOffset;
	}
}
