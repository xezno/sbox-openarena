namespace OpenArena;

partial class Player
{
	[ClientRpc]
	void BecomeGibsOnClient( Vector3 position )
	{
		_ = Particles.Create( "particles/gib.vpcf", position );
	}
}
