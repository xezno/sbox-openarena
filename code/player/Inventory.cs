namespace OpenArena;

public partial class Inventory : BaseNetworkable
{
	[Net] public IList<BaseWeapon> List { get; set; }

	[Net] public Entity Owner { get; set; }

	public virtual BaseWeapon Active
	{
		get
		{
			return ( Owner as Player )?.ActiveChild as BaseWeapon;
		}

		set
		{
			if ( Owner is Player player )
			{
				player.ActiveChild = value;
			}
		}
	}

	public virtual bool CanAdd( BaseWeapon weapon )
	{
		if ( weapon.CanCarry( Owner ) )
			return true;

		return false;
	}

	/// <summary>
	/// Delete every entity we're carrying. Useful to call on death.
	/// </summary>
	public virtual void DeleteContents()
	{
		Host.AssertServer();

		foreach ( var item in List.ToArray() )
		{
			item.Delete();
		}

		List.Clear();
	}

	/// <summary>
	/// Get the item in this slot
	/// </summary>
	public virtual BaseWeapon GetSlot( WeaponSlot slot )
	{
		return List.FirstOrDefault( x => x.WeaponData.InventorySlot == slot );
	}

	/// <summary>
	/// Returns the number of items in the inventory
	/// </summary>
	public virtual int Count() => List.Count;

	/// <summary>
	/// Returns the index of the currently active child
	/// </summary>
	public virtual WeaponSlot GetActiveSlot()
	{
		return Active?.WeaponData.InventorySlot ?? (WeaponSlot)( -1 );
	}

	public bool ContainsAny( string weaponLibraryName )
	{
		return List.Any( x => x.GetLibraryName() == weaponLibraryName );
	}

	public BaseWeapon First( string weaponLibraryName )
	{
		return List.OfType<BaseWeapon>().FirstOrDefault( x => x.GetLibraryName() == weaponLibraryName );
	}

	/// <summary>
	/// Set our active entity to the entity on this slot
	/// </summary>
	public virtual bool SetActiveSlot( WeaponSlot slot, bool evenIfEmpty = false )
	{
		var ent = GetSlot( slot );
		if ( Active == ent )
			return false;

		if ( !evenIfEmpty && ent == null )
			return false;

		Active = ent;

		return ent.IsValid();
	}

	/// <summary>
	/// Drop the active entity. If we can't drop it, will return null
	/// </summary>
	public virtual BaseWeapon DropActive()
	{
		if ( !Host.IsServer ) return null;

		var ac = Active;
		if ( ac == null ) return null;

		if ( Drop( ac ) )
		{
			Active = null;
			return ac;
		}

		return null;
	}

	/// <summary>
	/// Drop this entity. Will return true if successfully dropped.
	/// </summary>
	public virtual bool Drop( BaseWeapon ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		ent.Parent = null;
		ent.OnCarryDrop( Owner );

		return true;
	}

	/// <summary>
	/// Returns true if this inventory contains this entity
	/// </summary>
	public virtual bool Contains( Entity ent )
	{
		return List.Any( x => x == ent );
	}

	/// <summary>
	/// Returns true if this inventory contains any of this type
	/// </summary>
	public virtual bool ContainsAny<T>() where T : Entity
	{
		return List.OfType<T>().Any();
	}

	/// <summary>
	/// Returns first weapon of this type
	/// </summary>
	public virtual T First<T>() where T : Entity
	{
		return List.OfType<T>().First();
	}

	/// <summary>
	/// Make this entity the active one
	/// </summary>
	public virtual bool SetActive( BaseWeapon ent )
	{
		if ( Active == ent ) return false;
		if ( !Contains( ent ) ) return false;

		Active = ent;
		return true;
	}

	/// <summary>
	/// Try to add this entity to the inventory. Will return true
	/// if the entity was added successfully. 
	/// </summary>
	public virtual bool Add( BaseWeapon ent, bool makeActive = false )
	{
		Host.AssertServer();

		//
		// Can't pickup if already owned
		//
		if ( ent.Owner != null )
			return false;

		//
		// Let the inventory reject the entity
		//
		if ( !CanAdd( ent ) )
			return false;

		if ( ent is not BaseCarriable carriable )
			return false;

		//
		// Let the entity reject the inventory
		//
		if ( !carriable.CanCarry( Owner ) )
			return false;

		//
		// Passed!
		//
		ent.Parent = Owner;
		List.Add( ent );

		//
		// Let the item do shit
		//
		carriable.OnCarryStart( Owner );

		if ( makeActive )
		{
			SetActive( ent );
		}

		return true;
	}
}
