namespace OpenArena;

public static class TraceResultExtensions
{
	public static bool IsHeadshot( this TraceResult traceResult )
	{
		return traceResult.Bone == 11;
	}
}
