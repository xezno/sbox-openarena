namespace OpenArena;

[Library( "oa_info_target" )]
[HammerEntity]
[EditorModel( "models/editor/info_target.vmdl" )]
public class PredictedTarget : Entity
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
