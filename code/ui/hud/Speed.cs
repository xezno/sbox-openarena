namespace OpenArena;

[UseTemplate]
internal class Speed : Panel
{
	public string PlayerSpeed => $"{MathF.Abs( Local.Pawn.Velocity.WithZ( 0 ).Length ).FloorToInt()}";
}
