namespace OpenArena;

[Title( "Base Weapon" ), Icon( "sports_martial_arts" )]
public partial class BaseWeapon : BaseCarriable
{
	public virtual float Rate => 5.0f;

	public override void Spawn()
	{
		base.Spawn();

		CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
		SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it
	}

	[Net, Predicted]
	public TimeSince TimeSinceAttack { get; set; }

	public override void Simulate( Client player )
	{
		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSinceAttack = 0;
				AttackPrimary();
			}
		}
	}

	public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( InputButton.PrimaryAttack ) ) return false;

		var rate = Rate;
		if ( rate <= 0 ) return true;

		return TimeSinceAttack > ( 1 / rate );
	}

	public virtual void AttackPrimary()
	{

	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool InWater = Map.Physics.IsPointWater( start );

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Water, !InWater )
				.HitLayer( CollisionLayer.Debris )
				.Ignore( Owner )
				.Ignore( this )
				.Size( radius )
				.Run();

		yield return tr;
	}

	public override Sound PlaySound( string soundName, string attachment )
	{
		if ( Owner.IsValid() )
			return Owner.PlaySound( soundName, attachment );

		return base.PlaySound( soundName, attachment );
	}

	public virtual void RenderHud( Vector2 screenSize )
	{

	}
}
