namespace OpenArena;

[Title( "Player" ), Icon( "emoji_people" )]
public partial class Player : AnimatedEntity
{
	[Net, Predicted] public PawnController Controller { get; set; }
	[Net, Predicted] public PawnAnimator Animator { get; set; }
	[Net, Predicted] public Entity ActiveChild { get; set; }
	public Entity LastActiveChild { get; set; }

	public Inventory Inventory { get; protected set; }

	public CameraMode CameraMode
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	private TimeSince timeSinceDied;
	private TimeSince timeSinceLastFootstep = 0;

	public ModelEntity Corpse { get; set; }

	public override void Spawn()
	{
		EnableLagCompensation = true;

		Tags.Add( "player" );

		base.Spawn();
	}

	public Player()
	{
		Inventory = new Inventory( this );
	}

	public override void OnKilled()
	{
		Game.Current?.OnKilled( this );
		Client?.AddInt( "deaths", 1 );

		timeSinceDied = 0;
		LifeState = LifeState.Dead;
		StopUsing();

		Inventory.DeleteContents();
		ActiveChild = null;
		LastActiveChild = null;

		CameraMode = new SpectateRagdollCamera();
		EnableDrawing = false;
		BecomeRagdollOnClient( To.Everyone );
	}

	public virtual void CreateHull()
	{
		CollisionGroup = CollisionGroup.Player;
		AddCollisionLayer( CollisionLayer.Player );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		MoveType = MoveType.MOVETYPE_WALK;
		EnableHitboxes = true;
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( input.StopProcessing )
			return;

		if ( IsZooming )
			input.ViewAngles = Angles.Lerp( input.ViewAngles, input.OriginalViewAngles, 0.5f );

		ActiveChild?.BuildInput( input );
		Controller?.BuildInput( input );

		if ( input.StopProcessing )
			return;

		Animator?.BuildInput( input );
	}

	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();
		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public virtual float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 0.2f;
	}

	public override void StartTouch( Entity other )
	{
		if ( IsClient ) return;

		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );
			return;
		}

		Inventory?.Add( other, Inventory.Active == null );
	}

	public virtual void SimulateActiveChild( Client cl, Entity child )
	{
		if ( LastActiveChild != ActiveChild )
		{
			Log.Trace( $"{LastActiveChild} is not {ActiveChild}" );

			if ( LastActiveChild is BaseCarriable previousBc )
				previousBc?.ActiveEnd( Owner, previousBc.Owner != Owner );

			if ( ActiveChild is BaseCarriable nextBc )
				nextBc?.ActiveStart( Owner );
		}

		LastActiveChild = ActiveChild;

		if ( !ActiveChild.IsValid() )
			return;

		if ( ActiveChild.IsAuthority )
			ActiveChild.Simulate( cl );
	}

	public override void OnChildAdded( Entity child )
	{
		Inventory?.OnChildAdded( child );
	}

	public override void OnChildRemoved( Entity child )
	{
		Inventory?.OnChildRemoved( child );
	}

	[ClientRpc]
	public void RpcDamageDealt( bool isKill, int victimNetworkId )
	{
		var victim = All.OfType<Player>().First( x => x.NetworkIdent == victimNetworkId );
		Log.Trace( $"We did damage to {victim}" );

		if ( isKill )
			PlaySound( "kill" );

		PlaySound( "hit" );
	}
}
