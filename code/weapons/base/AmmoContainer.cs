namespace OpenArena;

public partial class AmmoContainer : BaseNetworkable
{
	[Net] public int Count { get; set; }

	public AmmoContainer()
	{
		Count = 100;
	}

	public bool Take()
	{
		if ( Count <= 0 )
			return false;

		Count--;
		return true;
	}
}
