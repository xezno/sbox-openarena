namespace OpenArena;

public class InventoryUI : Panel
{
	private List<InventoryPanel> WeaponPanels { get; set; } = new();

	public InventoryUI()
	{
		StyleSheet.Load( "/ui/hud/Inventory.scss" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
			return;

		var playerInventory = player.Inventory;
		var existingPanels = WeaponPanels.ToList();

		foreach ( var panel in existingPanels )
		{
			if ( !playerInventory.Contains( panel.Weapon ) )
			{
				Log.Trace( $"{panel.Weapon} isn't in player inv anymore, deleting.." );
				panel.Delete();
				WeaponPanels.Remove( panel );
			}
		}

		foreach ( var ent in playerInventory.List )
		{
			if ( ent is not BaseWeapon weapon )
				continue;

			if ( !WeaponPanels.Any( x => x.Weapon == weapon ) )
			{
				Log.Trace( $"{weapon} had no panel, creating.." );
				var panel = new InventoryPanel( weapon, this );
				WeaponPanels.Add( panel );
			}
		}

		for ( int i = 0; i < WeaponPanels.Count; i++ )
		{
			InventoryPanel panel = WeaponPanels[i];
			panel.SetClass( "active", playerInventory.GetActiveSlot() == panel.Weapon.WeaponData.InventorySlot );
		}

		SortChildren( x =>
		{
			if ( x is InventoryPanel invPanel )
				return (int)invPanel.Weapon.WeaponData.InventorySlot;

			return 0;
		} );
	}

	class InventoryPanel : Panel
	{
		public BaseWeapon Weapon { get; set; }

		private Label NameLabel { get; set; }

		public InventoryPanel( BaseWeapon weapon, Panel parent )
		{
			Add.Label( $"{(int)weapon.WeaponData.InventorySlot}" );
			NameLabel = Add.Label( weapon.WeaponData.UIName );

			Weapon = weapon;
			Parent = parent;
		}
	}
}
