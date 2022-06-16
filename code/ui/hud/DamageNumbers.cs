namespace OpenArena;

public class DamageNumbers : Panel
{
	public static DamageNumbers Instance { get; set; }

	public DamageNumbers()
	{
		Instance = this;
		SetClass( "damage-numbers-container", true );
		StyleSheet.Load( "/ui/hud/DamageNumbers.scss" );
	}

	private class DamageNumber : Label
	{
		private Vector3 WorldPoint { get; set; }

		TimeSince timeAlive;

		public DamageNumber( Vector3 worldPoint, float amount )
		{
			WorldPoint = worldPoint;
			Text = $"-{amount.CeilToInt()}";

			StyleSheet.Load( "/ui/hud/DamageNumbers.scss" );

			_ = TransitionOut();

			var rand = ( Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random ) * 0.25f;
			Velocity = ( rand.Normal * 500 ).WithY( -500 );
			timeAlive = 0;
		}

		private Vector2 Position { get; set; }
		private Vector2 Velocity { get; set; }

		public override void Tick()
		{
			var ndcPos = WorldPoint.ToScreen();

			float scale = Screen.Height / 1080f;

			Position += Velocity * Time.Delta;
			Velocity += new Vector2( 0, 1500 ) * Time.Delta;

			var ndcVec2 = new Vector2( ndcPos.x, ndcPos.y );
			var screenPos = ndcVec2 * Screen.Size / scale;

			Style.Left = Length.Pixels( screenPos.x + Position.x );
			Style.Top = Length.Pixels( screenPos.y + Position.y );
			Style.Opacity = 1.0f - ( timeAlive * 2 );
		}

		async Task TransitionOut()
		{
			await Task.Delay( 750 );
			Delete();
		}
	}

	[ArenaEvent.Player.DidDamage]
	public void AddNumbers( Vector3 pos, float amount )
	{
		var numberElem = new DamageNumber( pos, amount );
		numberElem.Parent = this;
	}
}
