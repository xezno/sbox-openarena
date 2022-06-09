namespace OpenArena;

[GameResource( "Weapon Data", "wda", "Metadata for weapons",
	Icon = "🔫", IconBgColor = "#71dcfe" )]
public class WeaponDataAsset : GameResource
{
	/// <summary>
	/// We use this because most weapons are going to be functionally different from
	/// each other. Specific functionality will get defined in a class with a Library
	/// attribute and we use the name of that attribute to give this weapon its unique
	/// function.
	/// </summary>
	public string LibraryName { get; set; } = "oa_weapon";

	/// <summary>
	/// A nice, UI-friendly name for this weapon.
	/// </summary>
	public string Name { get; set; } = "Weapon";

	/// <summary>
	/// A nice, UI-friendly, short (<255 character) description for this weapon.
	/// </summary>
	public string Description { get; set; } = "My Cool Weapon";

	/// <summary>
	/// How much damage this weapon should do to entities
	/// </summary>
	public float Damage { get; set; } = 10f;

	/// <summary>
	/// Should the player be able to hold down fire to shoot continuously?
	/// </summary>
	public bool AutoFire { get; set; } = true;

	/// <summary>
	/// How often should this weapon be ready to fire (per second)
	/// </summary>
	public float Rate { get; set; } = 5f;

	/// <summary>
	/// Which slot of the player's inventory should this go in?
	/// </summary>
	public WeaponSlot Slot { get; set; } = WeaponSlot.Melee;

	[ResourceType( "model" )]
	public string WorldModel { get; set; } = "";

	[ResourceType( "model" )]
	public string ViewModel { get; set; } = "";

	[ResourceType( "sound" )]
	public string FireSound { get; set; } = "rust_pistol.shoot.sound";

	[ResourceType( "vpcf" )]
	public string MuzzleFlashParticles { get; set; } = "particles/pistol_muzzleflash.vpcf";

	[ResourceType( "vpcf" )]
	public string TracerParticles { get; set; } = "particles/tracer.vpcf";
}
