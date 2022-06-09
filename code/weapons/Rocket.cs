namespace OpenArena;

public class Rocket : ModelEntity
{
	public TimeSince LifeTime { get; private set; }
	public float MaxLifetime => 2.5f;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/light_arrow.vmdl" );

		LifeTime = 0;
	}

	[Event.Frame]
	[Event.Tick]
	public void OnTick()
	{
		if ( !IsAuthority )
			return;

		var target = Position + Velocity * Time.Delta;
		var tr = Trace.Ray( Position, target ).Ignore( Owner ).Run();

		if ( IsServer )
		{
			if ( tr.Hit || LifeTime > MaxLifetime )
			{
				// Explode
				ArenaGame.Explode( tr.EndPosition, 10f, Owner );
				Delete();
			}
		}

		Position = target;
	}
}
