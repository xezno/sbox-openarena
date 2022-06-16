namespace OpenArena;

[GameResource( "Weapon Data", "wda", "Metadata for weapons",
	Icon = "🔫", IconBgColor = "#71dcfe" )]
public class WeaponDataAsset : GameResource
{

	#region Meta
	/// <summary>
	/// A nice, UI-friendly name for this weapon.
	/// </summary>
	[Category( "Meta" ), Title( "UI Name" )]
	public string UIName { get; set; } = "Weapon";

	/// <summary>
	/// A nice, UI-friendly, short (<255 character) description for this weapon.
	/// </summary>
	[Category( "Meta" ), Title( "UI Description" )]
	public string UIDescription { get; set; } = "My Cool Weapon";

	/// <summary>
	/// Which slot of the player's inventory should this go in?
	/// </summary>
	[Category( "Meta" )]
	public WeaponSlot InventorySlot { get; set; } = WeaponSlot.Melee;

	/// <summary>
	/// We use this because most weapons are going to be functionally different from
	/// each other. Specific functionality will get defined in a class with a Library
	/// attribute and we use the name of that attribute to give this weapon its unique
	/// function.
	/// </summary>
	[Category( "Meta" )]
	public string LibraryName { get; set; } = "oa_weapon";

	/// <summary>
	/// Which crosshair should we use? (prefixed with oa_crosshair_)
	/// </summary>
	[Category( "Meta" )]
	public string CrosshairLibraryName { get; set; } = "oa_crosshair_cross";
	#endregion

	#region Combat
	[Category( "Combat" )]
	public bool AutoFire { get; set; } = true;

	[Category( "Combat" )]
	public float Damage { get; set; } = 10f;

	[Category( "Combat" )]
	public float FireRate { get; set; } = 5f;

	[Category( "Combat" )]
	public float DeployTime { get; internal set; } = 1.0f;

	[HideInEditor]
	private int STK => ( 100f / Damage ).CeilToInt();

	[Category( "Combat" ), Title( "Calculated TTK" ), Editable( false )]
	public string TTK => $"{( ( STK / FireRate ) * 1000 ).CeilToInt()}ms (+ {DeployTime * 1000}ms deploy time)";
	#endregion

	#region Weapon-specific
	//
	// Shotgun
	//
	[Category( "Weapon-Specific" ), ShowIf( "InventorySlot", WeaponSlot.Shotgun )]
	public float Spread { get; set; } = 0.1f;

	[Category( "Weapon-Specific" ), ShowIf( "InventorySlot", WeaponSlot.Shotgun )]
	public float SpreadRandomness { get; set; } = 0.1f;

	[Category( "Weapon-Specific" ), ShowIf( "InventorySlot", WeaponSlot.Shotgun )]
	public Vector2 ShotCount { get; set; } = new( 3, 3 );
	#endregion

	#region Visuals
	[Category( "Visuals" ), ResourceType( "model" )]
	public string WorldModel { get; set; } = "";

	[Category( "Visuals" ), ResourceType( "model" )]
	public string ViewModel { get; set; } = "";

	[Category( "Visuals" ), ResourceType( "sound" )]
	public string FireSound { get; set; } = "rust_pistol.shoot.sound";

	[Category( "Visuals" ), ResourceType( "vpcf" )]
	public string MuzzleFlashParticles { get; set; } = "particles/pistol_muzzleflash.vpcf";

	[Category( "Visuals" ), ResourceType( "vpcf" )]
	public string TracerParticles { get; set; } = "particles/tracer.vpcf";
	#endregion

	#region Functions
	public static WeaponDataAsset FindByLibraryName( string libraryName ) =>
		ResourceLibrary.GetAll<WeaponDataAsset>().FirstOrDefault( x => x.LibraryName == libraryName );
	#endregion
}
