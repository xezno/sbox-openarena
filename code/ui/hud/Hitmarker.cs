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
	public void OnDidDamage()
	{
		currentHitmarkerInstance?.Delete();
		currentHitmarkerInstance = new HitmarkerInstance( this );
	}

	public class HitmarkerInstance : Panel
	{
		private TimeSince timeSinceCreated;
		private float duration = 0.1f; // in seconds

		public HitmarkerInstance( Panel parent )
		{
			Parent = parent;

			timeSinceCreated = 0;

			Style.Width = 96f;
			Style.Height = 96f;
		}

		public override void Tick()
		{
			base.Tick();

			float t = timeSinceCreated.Relative.LerpInverse( 0, duration );
			if ( t >= 1 )
				Delete();

			Style.Opacity = 1.0f - t;

			Log.Trace( Style.Opacity );
		}
	}
}
