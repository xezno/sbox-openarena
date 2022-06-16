namespace OpenArena;

[UseTemplate]
public class Vitals : Panel
{
	//
	// @ref
	//
	public Label AmmoLabel { get; set; }
	public Label HealthLabel { get; set; }
	public Label ShieldsLabel { get; set; }
	public Panel HealthPanel { get; set; }

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		HealthPanel.BindClass( "low", () => ( Local.Pawn as Player )?.Health <= 30 );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
		{
			AmmoLabel.Text = "-";
			HealthLabel.Text = "-";
			ShieldsLabel.Text = "-";
			return;
		}

		if ( player.ActiveChild is BaseWeapon weapon && weapon.Ammo != null )
			AmmoLabel.Text = $"{weapon.Ammo.Count}";
		else
			AmmoLabel.Text = "";

		HealthLabel.Text = $"{player.Health.CeilToInt()}";
		ShieldsLabel.Text = $"{player.Shields.CeilToInt()}";
	}
}
