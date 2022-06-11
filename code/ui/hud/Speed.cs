namespace OpenArena;

[UseTemplate]
internal class Speed : Panel
{
	public string PlayerSpeed => $"{MathF.Abs( Local.Pawn.Velocity.Dot( Local.Pawn.EyeRotation.Forward ) ).FloorToInt()} u/s";
}
