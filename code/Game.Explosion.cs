namespace OpenArena;

partial class ArenaGame
{
	public static void Explode( Vector3 position, float damage = 100, Entity owner = null )
	{
		var sourcePos = position;
		var radius = 120f;
		var overlaps = All.Where( x => Vector3.DistanceBetween( sourcePos, x.Position ) <= radius ).ToList();

		DebugOverlay.Sphere( position, radius, Color.Red, 5f );

		foreach ( var overlap in overlaps )
		{
			// Check if this is something we can explode
			if ( overlap is not ModelEntity ent || !ent.IsValid() )
				continue;

			if ( ent.LifeState != LifeState.Alive || !ent.PhysicsBody.IsValid() || ent.IsWorld )
				continue;

			if ( !ent.IsAuthority )
				continue;

			// Check to make sure there's nothing in the way
			var tr = Trace.Ray( position, ent.Position ).Run();

			if ( tr.Hit && tr.Entity != ent )
				continue;

			var dir = ( overlap.Position - position ).Normal;
			dir = dir.WithZ( 1 ).Normal; // Shoot up into the air

			var dist = Vector3.DistanceBetween( position, overlap.Position + ent.CollisionBounds.Center );
			var force = ent.PhysicsBody.Mass * 0.25f;

			float damageFrac = dist.LerpInverse( radius, 64 );

			// Rocket jumping overrides
			if ( ent == owner )
			{
				force *= 1.5f;
				damageFrac *= 0.3f;

				if ( owner is Player { Controller: WalkController { Duck.IsActive: true } } )
				{
					force *= 1.5f;
				}
			}

			// Clear ground entity
			ent.GroundEntity = null;
			if ( ent is Player { Controller: WalkController playerController } )
				playerController.ClearGroundEntity();

			// Apply impulse & damage
			ent.ApplyAbsoluteImpulse( dir * force );
			ent.TakeDamage( DamageInfo.Generic( damage * damageFrac )
									  .WithAttacker( owner )
									  .WithFlag( DamageFlags.AlwaysGib )
									  .WithPosition( ent.Position + ent.CollisionBounds.Center ) );
		}

		using ( Prediction.Off() )
		{
			var particle = Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", position );
			Sound.FromWorld( "explosion.small", position );
		}
	}
}
