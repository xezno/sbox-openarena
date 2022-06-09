namespace OpenArena;

[GameResource( "Weapon Data", "wda", "Metadata for weapons" )]
public class WeaponDataAsset : GameResource
{
	public string LibraryName { get; set; } = "oa_weapon";
	public string Name { get; set; } = "Weapon";
	public string Description { get; set; } = "My Cool Weapon";
	public float Damage { get; set; } = 10f;
	public bool AutoFire { get; set; } = true;
	public float Rate { get; set; } = 5f;

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
