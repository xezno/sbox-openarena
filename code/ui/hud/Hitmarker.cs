namespace OpenArena;

public partial class Hitmarker : Panel
{
	public static Hitmarker Instance { get; private set; }
	private static HitmarkerInstance currentHitmarkerInstance;

	public Hitmarker()
	{
		Instance = this;
		StyleSheet.Load( "/ui/hud/Hitmarker.scss" );
	}

	[ArenaEvent.Player.DidDamage]
	public void OnDidDamage( Vector3 position, float amount )
	{
		currentHitmarkerInstance?.Delete();
		currentHitmarkerInstance = new HitmarkerInstance( this );
	}

	public class HitmarkerInstance : Panel
	{
		private TimeSince timeSinceCreated;
		private float duration = 0.2f; // in seconds

		public HitmarkerInstance( Panel parent )
		{
			Parent = parent;

			timeSinceCreated = 0;
		}

		public override void Tick()
		{
			base.Tick();

			float t = timeSinceCreated.Relative.LerpInverse( 0, duration );
			if ( t >= 1 )
				Delete();

			// Fade in/out (0 to 1 to 0) - https://www.desmos.com/calculator/kscda6lwcu
			float opacity = MathF.Sin( t * MathF.PI );
			Style.Opacity = opacity;
		}
	}
}
