namespace OpenArena;

//
// This isn't exactly like the Quake movement controller
// ( some quirks have been tweaked or removed ) but it's much
// closer to arena shooter movement than the default WalkController
//
public class QuakeWalkController : PawnController
{
	public float BodyGirth => 32f;
	public float BodyHeight => 72f;
	public float EyeHeight => 64f;

	private Vector3 mins;
	private Vector3 maxs;

	public override void FrameSimulate()
	{
		EyeRotation = Input.Rotation;
	}

	public virtual void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	public virtual void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;

		var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
		var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

		SetBBox( mins, maxs );
	}

	public override void Simulate()
	{
		EyeRotation = Input.Rotation;
		EyeLocalPosition = Vector3.Up * ( EyeHeight * Pawn.Scale );

		UpdateBBox();
	}
}
