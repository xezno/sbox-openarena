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
			Announcer = new();
	}

	private void TickInventorySlot()
	{
		if ( Input.Pressed( InputButton.Slot1 ) ) Inventory.SetActiveSlot( 0 );
		if ( Input.Pressed( InputButton.Slot2 ) ) Inventory.SetActiveSlot( 1 );
		if ( Input.Pressed( InputButton.Slot3 ) ) Inventory.SetActiveSlot( 2 );
		if ( Input.Pressed( InputButton.Slot4 ) ) Inventory.SetActiveSlot( 3 );
		if ( Input.Pressed( InputButton.Slot5 ) ) Inventory.SetActiveSlot( 4 );
		if ( Input.Pressed( InputButton.Slot6 ) ) Inventory.SetActiveSlot( 5 );
		if ( Input.Pressed( InputButton.Slot7 ) ) Inventory.SetActiveSlot( 6 );
		if ( Input.Pressed( InputButton.Slot8 ) ) Inventory.SetActiveSlot( 7 );
		if ( Input.Pressed( InputButton.Slot9 ) ) Inventory.SetActiveSlot( 8 );
	}

	public override void Simulate( Client cl )
	{
		if ( LifeState == LifeState.Dead )
			return;

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
				$"Slot 0:                      {Inventory.GetSlot( 0 )}\n" +
				$"Slot 1:                      {Inventory.GetSlot( 1 )}\n" +
				$"Slot 2:                      {Inventory.GetSlot( 2 )}\n" +
				$"Slot 3:                      {Inventory.GetSlot( 3 )}\n" +
				$"Slot 4:                      {Inventory.GetSlot( 4 )}\n" +
				$"Slot 5:                      {Inventory.GetSlot( 5 )}\n" +
				$"Slot 6:                      {Inventory.GetSlot( 6 )}\n" +
				$"Slot 7:                      {Inventory.GetSlot( 7 )}\n" +
				$"Slot 8:                      {Inventory.GetSlot( 8 )}\n" +
				$"Slot 9:                      {Inventory.GetSlot( 9 )}\n",
				new Vector2( 60, 350 ) );
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		LastDamageInfo = info;

		if ( IsInvincible && info.Attacker.IsValid() )
			return;

		base.TakeDamage( info );

		if ( LifeState == LifeState.Dead )
		{
			RpcOnDeath( To.Single( this ), info.Attacker?.NetworkIdent ?? -1 );

			if ( info.Attacker != null && info.Attacker.Client != null && info.Attacker != this )
			{
				RpcOnKill( To.Single( info.Attacker ), this.NetworkIdent );

				info.Attacker.Client.AddInt( "kills" );
			}

			bool shouldGib = Rand.Int( 0, 10 ) == 0;

			if ( info.Flags.HasFlag( DamageFlags.AlwaysGib ) )
				shouldGib = true;

			if ( info.Flags.HasFlag( DamageFlags.DoNotGib ) )
				shouldGib = false;

			if ( shouldGib )
			{
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
			RpcDamageDealt( To.Single( info.Attacker ), LifeState == LifeState.Dead, info.Damage, NetworkIdent );
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
	public void RpcDamageDealt( bool isKill, float damageAmount, int victimNetworkId )
	{
		var victim = All.OfType<Player>().First( x => x.NetworkIdent == victimNetworkId );
		Log.Trace( $"We did damage to {victim}" );

		if ( isKill )
			PlaySound( "kill" );

		float t = damageAmount.LerpInverse( 0.0f, 100.0f );
		float pitch = MathX.LerpTo( 1.0f, 1.25f, t );

		// TODO(AG) : Am I fucking something up or does SetPitch not work
		Sound.FromScreen( "hit" ).SetRandomPitch( pitch, pitch );
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( ActiveChild is BaseWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}
}
