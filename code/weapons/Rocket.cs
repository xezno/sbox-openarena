namespace OpenArena;

public partial class Rocket : ModelEntity
{
	[Net, Predicted] public TimeSince LifeTime { get; private set; }
	public float MaxLifetime => 2.5f;
	[Net, Predicted] public bool ShouldMove { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/light_arrow.vmdl" );

		LifeTime = 0;
		ShouldMove = true;
		Predictable = true;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( !IsAuthority )
			return;

		if ( !ShouldMove )
			return;

		var target = Position + Velocity * Time.Delta;
		var tr = Trace.Ray( Position, target ).Ignore( Owner ).Run();
		DebugOverlay.Line( Position, target, 5f, false );

		if ( tr.Hit || LifeTime > MaxLifetime )
		{
			// Explode
			ArenaGame.Explode( tr.EndPosition, 40f, Owner );
			ShouldMove = false;

			if ( IsServer )
				Delete();
		}

		Position = target;
	}
}
