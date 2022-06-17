namespace OpenArena;

[Library( "oa_jumppad" )]
[Title( "Jump Pad" ), Icon( "arrow_upward" ), Category( "World" )]
[Line( "targetname", "TargetEntity" )]
[HammerEntity]
public partial class JumpPad : PredictedTrigger
{
	[Net, Property, FGDType( "target_destination" )]
	public string TargetEntity { get; set; } = "";

	[Net, Property]
	public float VerticalForce { get; set; } = 256f;

	[Net, Property]
	public float Force { get; set; } = 1024f;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public override void PredictedTouch( Player player )
	{
		var target = Entity.All.FirstOrDefault( x => x.Name == TargetEntity );

		if ( !target.IsValid() )
			return;

		if ( player.Controller is not QuakeWalkController walkController )
			return;

		var direction = ( target.Position - walkController.Position ).Normal;

		var impulse = ( direction * Force ) + ( Vector3.Up * VerticalForce );
		walkController.ApplyImpulse( impulse );

		base.PredictedTouch( player );
	}
}
