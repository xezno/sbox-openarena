namespace OpenArena;

partial class Player
{
	[Net] private bool IsInvincible { get; set; }

	public void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		CameraMode = new FirstPersonCamera();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		WaterLevel = 0;

		CreateHull();

		Game.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();
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
		{
			if ( timeSinceDied > 3 && IsServer )
			{
				Respawn();
			}

			return;
		}

		Controller?.Simulate( cl, this, Animator );
		SimulateActiveChild( cl, ActiveChild );
		TickPlayerUse();
		TickInventorySlot();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );

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

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if ( IsInvincible && info.Attacker.IsValid() )
			return;

		base.TakeDamage( info );
		this.ProceduralHitReaction( info );

		// Add a score to the killer
		if ( LifeState == LifeState.Dead && info.Attacker != null )
		{
			if ( info.Attacker.Client != null && info.Attacker != this )
			{
				info.Attacker.Client.AddInt( "kills" );
			}
		}

		// Tell attacker that they did damage to us
		if ( IsServer && info.Attacker != this )
		{
			RpcDamageDealt( To.Single( info.Attacker ), LifeState == LifeState.Dead, NetworkIdent );
		}
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( ActiveChild is BaseWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}
}
