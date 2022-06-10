namespace OpenArena;

[Library( "oa_jumppad" )]
[Title( "Jump Pad" ), Icon( "arrow_upward" ), Category( "World" )]
[Line( "targetname", "TargetEntity" )]
[HammerEntity]
public partial class JumpPad : BaseTrigger
{
	[Net, Property, FGDType( "target_destination" )]
	public string TargetEntity { get; set; } = "";

	[Net, Property]
	public float VerticalForce { get; set; } = 256f;

	[Net, Property]
	public float Force { get; set; } = 1024f;

	public override void Touch( Entity other )
	{
		if ( !IsServer )
			return;

		if ( other is not Player player )
			return;

		var target = Entity.All.FirstOrDefault( x => x.Name == TargetEntity );

		if ( !target.IsValid() )
			return;

		var direction = ( target.Position - other.Position ).Normal;

		if ( player.Controller is WalkController controller )
		{
			var impulse = ( direction * Force ) + ( Vector3.Up * VerticalForce );
			controller.ApplyImpulse( impulse );
		}

		base.Touch( other );
	}
}
