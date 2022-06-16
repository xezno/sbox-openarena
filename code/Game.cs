global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using SandboxEditor;
global using System;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;

namespace OpenArena;

public partial class ArenaGame : Sandbox.Game
{
	[Net] public BaseGamemode Gamemode { get; set; }

	public ArenaGame()
	{
		if ( IsServer )
		{
			_ = new Hud();
			Gamemode = new DeathmatchGamemode();
		}
	}

	[ConCmd.Admin( "oa_set_gamemode" )]
	public static void SetGamemode( string gamemodeLibraryName )
	{
		var game = Current as ArenaGame;
		if ( game == null )
			return;

		game.Gamemode = TypeLibrary.Create<BaseGamemode>( gamemodeLibraryName );
		Log.Trace( $"Setting gamemode to {game.Gamemode}" );
	}

	public override void Simulate( Client cl )
	{
		Gamemode?.Simulate();
		base.Simulate( cl );
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		Gamemode.RespawnPlayer( pawn );
		client.Pawn = pawn;
	}

	public override void RenderHud()
	{
		base.RenderHud();

		if ( Local.Pawn is not Player player )
			return;

		//
		// scale the screen using a matrix, so the scale math doesn't invade everywhere
		// (other than having to pass the new scale around)
		//

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		using ( Render.Draw2D.MatrixScope( matrix ) )
		{
			player.RenderHud( screenSize );
		}
	}
}

