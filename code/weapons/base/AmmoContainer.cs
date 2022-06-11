namespace OpenArena;

public partial class AmmoContainer : BaseNetworkable
{
	[ConVar.Replicated( "oa_infinite_ammo" )] public static bool InfiniteAmmo { get; set; }
	[Net] public int Count { get; set; }

	public AmmoContainer()
	{
		Count = 100;
	}

	public bool Take()
	{
		if ( InfiniteAmmo )
			return true;

		if ( Count <= 0 )
			return false;

		Count--;
		return true;
	}
}
