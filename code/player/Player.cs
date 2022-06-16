namespace OpenArena;

partial class Player
{
	[Net] private bool IsInvincible { get; set; }
	[ConVar.Replicated( "oa_debug_player" )] public static bool Debug { get; set; }
	private Announcer Announcer { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		if ( IsLocalPawn )
		{
			Announcer = new();
			Inventory.Owner = this;
		}
	}

	private void TickInventorySlot()
	{
		if ( Input.Pressed( InputButton.Slot1 ) ) Inventory.SetActiveSlot( WeaponSlot.Melee );
		if ( Input.Pressed( InputButton.Slot2 ) ) Inventory.SetActiveSlot( WeaponSlot.Pistol );
		if ( Input.Pressed( InputButton.Slot3 ) ) Inventory.SetActiveSlot( WeaponSlot.Shotgun );
		if ( Input.Pressed( InputButton.Slot4 ) ) Inventory.SetActiveSlot( WeaponSlot.FullAuto );
		if ( Input.Pressed( InputButton.Slot5 ) ) Inventory.SetActiveSlot( WeaponSlot.Rifle );
		if ( Input.Pressed( InputButton.Slot6 ) ) Inventory.SetActiveSlot( WeaponSlot.Special );
		if ( Input.Pressed( InputButton.Slot7 ) ) Inventory.SetActiveSlot( WeaponSlot.Explosive );
		if ( Input.Pressed( InputButton.Slot8 ) ) Inventory.SetActiveSlot( WeaponSlot.Super );
	}

	public override void Simulate( Client cl )
	{
		if ( LifeState == LifeState.Dead )
			return;

		Health = Health.Clamp( 0, 200 );

		if ( Health > 100 )
			Health -= Time.Delta;

		Corpse?.Delete();

		Controller?.Simulate( cl, this, Animator );
		SimulateActiveChild( cl, ActiveChild );
		TickPlayerUse();
		TickInventorySlot();

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( CameraMode is FirstPersonCamera )
				CameraMode = new ThirdPersonCamera();
			else
				CameraMode = new FirstPersonCamera();
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );

		if ( Debug )
		{
			DebugOverlay.ScreenText( "[PLAYER]\n" +
				$"ActiveChild:                 {ActiveChild}\n" +
				$"LastActiveChild:             {LastActiveChild}\n" +
				$"Health:                      {Health}\n" +
				$"God:                         {IsInvincible}",
				new Vector2( 60, 150 ) );

			DebugOverlay.ScreenText( "[INVENTORY]\n" +
				$"ActiveSlot:                  {Inventory.GetActiveSlot()}\n" +
				$"Active:                      {Inventory.Active}\n" +
				$"Slot 0:                      {Inventory.GetSlot( WeaponSlot.Melee )}\n" +
				$"Slot 1:                      {Inventory.GetSlot( WeaponSlot.Pistol )}\n" +
				$"Slot 2:                      {Inventory.GetSlot( WeaponSlot.Shotgun )}\n" +
				$"Slot 3:                      {Inventory.GetSlot( WeaponSlot.FullAuto )}\n" +
				$"Slot 4:                      {Inventory.GetSlot( WeaponSlot.Rifle )}\n" +
				$"Slot 5:                      {Inventory.GetSlot( WeaponSlot.Special )}\n" +
				$"Slot 6:                      {Inventory.GetSlot( WeaponSlot.Explosive )}\n" +
				$"Slot 7:                      {Inventory.GetSlot( WeaponSlot.Super )}\n",
				new Vector2( 60, 350 ) );
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		LastDamageInfo = info;

		if ( IsInvincible && info.Attacker.IsValid() )
			return;

		float newHealth = Health - info.Damage;

		base.TakeDamage( info );

		// Do a few things if this killed the player
		if ( LifeState == LifeState.Dead )
		{
			// Tell ourselves that we died
			RpcOnDeath( To.Single( this ), info.Attacker?.NetworkIdent ?? -1 );

			// Tell attacker that they killed us
			if ( info.Attacker != null && info.Attacker.Client != null && info.Attacker != this )
			{
				RpcOnKill( To.Single( info.Attacker ), this.NetworkIdent );

				info.Attacker.Client.AddInt( "kills" );
			}

			// Gibbing & ragdolling
			bool shouldGib = newHealth <= -50;

			if ( info.Flags.HasFlag( DamageFlags.AlwaysGib ) )
				shouldGib = true;

			if ( info.Flags.HasFlag( DamageFlags.DoNotGib ) )
				shouldGib = false;

			if ( shouldGib )
			{
				// Sometimes we might not get a damage position (i.e. if it was through
				// an explosive or trigger) so we'll take the player's position and move
				// up a little bit to make things still look okay
				if ( info.Position == Vector3.Zero )
					info.Position = Position + new Vector3( 0, 0, 32 );

				BecomeGibsOnClient( To.Everyone, info.Position );
			}
			else
			{
				BecomeRagdollOnClient( To.Everyone );
			}
		}
		else
		{
			this.ProceduralHitReaction( info );
		}

		// Tell attacker that they did damage to us
		if ( IsServer && info.Attacker != this )
		{
			RpcDamageDealt( To.Single( info.Attacker ),
				LifeState == LifeState.Dead, info.Position, info.Damage, NetworkIdent );
		}
	}

	public bool CanMove()
	{
		return true;
	}

	[ClientRpc]
	public void RpcOnKill( int victimIdent )
	{
		var victim = Entity.All.OfType<Player>().FirstOrDefault( x => x.NetworkIdent == victimIdent );
		Event.Run( ArenaEvent.Player.Kill.Name, victim, LastDamageInfo );
	}

	[ClientRpc]
	public void RpcOnDeath( int attackerIdent )
	{
		var attacker = Entity.All.OfType<Player>().FirstOrDefault( x => x.NetworkIdent == attackerIdent );
		Event.Run( ArenaEvent.Player.Death.Name, attacker, LastDamageInfo );
	}

	[ClientRpc]
	public void RpcDamageDealt( bool isKill, Vector3 position, float damageAmount, int victimNetworkId )
	{
		var victim = All.OfType<Player>().First( x => x.NetworkIdent == victimNetworkId );
		Log.Trace( $"We did damage to {victim}" );

		if ( isKill )
			PlaySound( "kill" );

		float t = damageAmount.LerpInverse( 0.0f, 100.0f );
		float pitch = MathX.LerpTo( 1.0f, 1.25f, t );

		// TODO(AG) : Am I fucking something up or does SetPitch not work
		Sound.FromScreen( "hit" ).SetRandomPitch( pitch, pitch );

		Event.Run( ArenaEvent.Player.DidDamage.Name, position, damageAmount );
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( ActiveChild is BaseWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}
}
