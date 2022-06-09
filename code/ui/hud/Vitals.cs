namespace OpenArena;

[UseTemplate]
public class Vitals : Panel
{
	public string Ammo => $"{( ( Local.Pawn as Player ).ActiveChild as BaseWeapon )?.Ammo}";
	public string Health => $"{( Local.Pawn as Player )?.Health}";
	public string Shields => $"{0}";

	public Panel HealthPanel { get; set; }
	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		HealthPanel.BindClass( "low", () => ( Local.Pawn as Player )?.Health <= 30 );
	}
}
