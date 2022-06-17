namespace OpenArena;

[EditorModel( "models/world/weapon_spawn.vmdl" )]
public partial class BasePickup : PredictedTrigger
{
	private SceneObject weaponSceneObject;

	[Net, Predicted] private TimeUntil TimeUntilReady { get; set; } = 0;
	private float CooldownPeriod => 5f;
	private bool IsReady => TimeUntilReady <= 0;

	private float baseOffset;
	private Particles progressParticles;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/world/weapon_spawn.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		CollisionGroup = CollisionGroup.Trigger;
	}

	protected void SetGhostModel( string modelName )
	{
		var weaponAttachmentTransform = GetAttachment( "weapon" ) ?? default;
		weaponSceneObject = new( Map.Scene, Model.Load( modelName ), weaponAttachmentTransform );

		progressParticles = Particles.Create( "particles/pickup.vpcf" );

		baseOffset = Rand.Float( 0, MathF.PI * 2 ); // 0 to 1 sine cycles
	}

	[Event.Frame]
	public void OnFrame()
	{
		Host.AssertClient();

		float t = TimeUntilReady.Relative.LerpInverse( CooldownPeriod, 0 );

		{
			progressParticles.SetPosition( 0, Position + Vector3.Up * 0.25f );
			progressParticles.SetPositionComponent( 1, 0, t );
			progressParticles.SetPositionComponent( 1, 1, IsReady ? 1 : 0 );
		}

		{
			float sinX = Time.Now + baseOffset;
			Vector3 bobbingOffset = Vector3.Up * MathF.Sin( sinX * 2f ) * 4f;
			weaponSceneObject.Rotation = Rotation.From( 0, sinX * 90f, 0 );
			weaponSceneObject.Position += bobbingOffset * Time.Delta;

			weaponSceneObject.Flags.IsTranslucent = true;
			weaponSceneObject.Flags.IsOpaque = false;

			float alpha = t < 1.0f ? 0.5f : 1.0f;
			weaponSceneObject.ColorTint = Color.White * alpha;
		}
	}

	public override void PredictedStartTouch( Player player )
	{
		base.PredictedStartTouch( player );

		if ( !IsReady )
			return;

		if ( IsServer )
			OnPickup( player );

		TimeUntilReady = CooldownPeriod;
	}

	public virtual void OnPickup( Player player )
	{

	}
}

