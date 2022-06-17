namespace OpenArena;

[Library( "oa_weapon_crowbar" )]
public class Crowbar : BaseWeapon
{
	public override void CreateViewModel()
	{
		base.CreateViewModel();

		ViewModelEntity?.SetAnimGraph( "models/first_person/first_person_arms_punching.vanmgrph" );
	}

	public override void AttackPrimary()
	{
		SwingCrowbar();
	}

	private TraceResult TraceCrowbarSwing()
	{
		using var _ = LagCompensation();

		Vector3 start = Owner.EyePosition;
		Vector3 end = Owner.EyePosition + Owner.EyeRotation.Forward * 64f;

		float radius = 2.0f;

		bool inWater = Map.Physics.IsPointWater( start );

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Water, !inWater )
				.HitLayer( CollisionLayer.Debris )
				.Ignore( Owner )
				.Ignore( this )
				.Size( radius )
				.Run();

		return tr;
	}

	private void SwingCrowbar()
	{
		var tr = TraceCrowbarSwing();

		//
		// Swing damage etc.
		//
		if ( tr.Hit )
		{
			tr.Surface.DoBulletImpact( tr );
			if ( tr.Entity.IsValid() && !tr.Entity.IsWorld )
			{
				tr.Entity.TakeDamage( CreateDamageInfo( tr ) );
			}
		}

		//
		// Swing effects
		//
		CreateSwingEffects();
	}

	private void CreateSwingEffects()
	{
		ViewModelEntity?.SetAnimParameter( "attack", true );
	}
}
