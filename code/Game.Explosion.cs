namespace OpenArena;

partial class ArenaGame
{
	public static void Explode( Vector3 position, float damage = 100, Entity owner = null )
	{
		Host.AssertServer();

		var sourcePos = position;
		var radius = 256f;
		var overlaps = All.Where( x => Vector3.DistanceBetween( sourcePos, x.Position ) <= radius ).ToList();

		foreach ( var overlap in overlaps )
		{
			if ( overlap is not ModelEntity ent || !ent.IsValid() ) continue;
			if ( ent.LifeState != LifeState.Alive || !ent.PhysicsBody.IsValid() || ent.IsWorld ) continue;

			var dir = ( overlap.Position - position ).Normal;
			var dist = Vector3.DistanceBetween( position, overlap.Position );

			if ( dist > radius ) continue;

			var distanceFactor = 1.0f - Math.Clamp( dist / radius, 0, 1 );
			float mul = 0.25f;
			var force = distanceFactor * ent.PhysicsBody.Mass * mul;

			if ( ent.GroundEntity != null )
			{
				ent.GroundEntity = null;
				if ( ent is Player { Controller: WalkController playerController } )
					playerController.ClearGroundEntity();
			}

			if ( ent == owner )
			{
				force *= 1.5f;
				if ( owner is Player { Controller: WalkController { Duck.IsActive: true } } )
				{
					force *= 1.5f;
				}
			}

			var tr = Trace.Ray( position, ent.Position ).Run();
			if ( tr.Hit && tr.Entity != ent )
				continue;

			ent.TakeDamage( DamageInfo.Generic( damage ).WithAttacker( owner ).WithFlag( DamageFlags.AlwaysGib ) );
			ent.ApplyAbsoluteImpulse( dir * force );
		}

		using ( Prediction.Off() )
		{
			var particle = Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", position );
			Sound.FromWorld( "explosion.small", position );
		}
	}
}
