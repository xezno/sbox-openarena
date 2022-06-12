namespace OpenArena;

public class FirstPersonCamera : Sandbox.FirstPersonCamera
{
	public override void Update()
	{
		base.Update();

		ZNear = 1;
		ZFar = 25000;
	}
}
