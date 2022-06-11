namespace OpenArena;

public class Announcer : Entity
{
	[ConVar.Replicated( "oa_debug_announcer" )] public static bool Debug { get; set; }

	private int FastKillStreak { get; set; }
	private int CurrentKillStreak { get; set; }
	private TimeSince TimeSinceLastKill { get; set; }

	private float TimeBetweenFastKills => 3.0f;
	private Queue<string> SoundQueue { get; set; } = new();
	private Sound CurrentlyPlayingSound { get; set; }

	public Player Player { get; set; }

	private void QueueAnnouncerSound( string soundId )
	{
		var soundName = $"{soundId}";

		// No empty sound names
		if ( string.IsNullOrEmpty( soundId ) )
			return;

		// No duplicates
		if ( SoundQueue.Contains( soundName ) )
			return;

		Log.Trace( $"Queueing announcer sound {soundName}" );
		SoundQueue.Enqueue( soundName );
	}

	[Event.Tick.Client]
	public void OnTick()
	{
		if ( Debug )
		{
			var queueString = string.Join( ", ", SoundQueue.Select( x => x.ToString() ).ToList() );
			DebugOverlay.ScreenText( "[ANNOUNCER]\n" +
				$"CurrentKillStreak:           {CurrentKillStreak}\n" +
				$"FastKillStreak:              {FastKillStreak}\n" +
				$"TimeSinceLastKill:           {TimeSinceLastKill}\n" +
				$"Sound Queue Count:           {SoundQueue.Count}\n" +
				$"Sound Queue:                 {queueString}",
				new Vector2( 60, 600 ) );
		}

		//
		// Play sounds from the sound queue if we can
		//
		if ( SoundQueue.Count > 0 )
		{
			if ( CurrentlyPlayingSound.Finished )
			{
				var nextSound = SoundQueue.Dequeue();
				Log.Trace( $"Playing announcer sound {nextSound}" );

				CurrentlyPlayingSound = Sound.FromScreen( nextSound );
			}
		}

		//
		// Reset fast kill streak if it's been a while since last kill
		//
		if ( TimeSinceLastKill > TimeBetweenFastKills )
			FastKillStreak = 0;
	}

	[ArenaEvent.Player.Kill]
	public void OnKill( Player victim, DamageInfo damageInfo )
	{
		CurrentKillStreak++;
		FastKillStreak++;

		TimeSinceLastKill = 0;

		if ( damageInfo.IsHeadshot() )
			QueueAnnouncerSound( "headshot" );

		string fastKillSound = FastKillStreak switch
		{
			// Kills => Sound name
			2 => "doublekill",
			3 => "multikill",
			4 => "ultrakill",
			5 => "monsterkill",
			_ => ""
		};

		QueueAnnouncerSound( fastKillSound );

		string killStreakSound = CurrentKillStreak switch
		{
			// Kills => Sound name
			5 => "killingspree",
			10 => "rampage",
			15 => "dominating",
			20 => "unstoppable",
			25 => "godlike",
			_ => ""
		};

		QueueAnnouncerSound( killStreakSound );
	}

	[ArenaEvent.Player.Death]
	public void OnDeath( Player attacker, DamageInfo damageInfo )
	{
		CurrentKillStreak = 0;
	}
}
