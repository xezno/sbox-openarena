namespace OpenArena;

[Library( "oa_gamemode_instagib" )]
public class InstagibGamemode : BaseGamemode
{
	public InstagibGamemode() : base() { }

	[Event.Entity.PostSpawn]
	public void OnPostSpawn()
	{
		void DeleteAll<T>() where T : Entity
		{
			var matchingEnts = Entity.All.OfType<T>().ToList();
			matchingEnts.ForEach( x => x.Delete() );
		}

		DeleteAll<WeaponSpawner>();
		DeleteAll<VitalsSpawner>();
	}

	public override void RespawnPlayer( Player player )
	{
		base.RespawnPlayer( player );

		player.Health = 1; // One shot, one kill
	}

	protected override void SetInventory( Player player )
	{
		player.Inventory.DeleteContents();
		player.Inventory.Add( new Railgun(), true );
	}

	protected override void CheckRespawning()
	{
		foreach ( var player in Players )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				if ( player.TimeSinceDied > 3 && Host.IsServer )
				{
					RespawnPlayer( player );
				}
			}
		}
	}
}
