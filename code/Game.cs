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
			DrawKeyOverlay( screenSize );
		}
	}

	private void DrawKeyOverlay( Vector2 screenSize )
	{
		var draw = Render.Draw2D;
		void DrawInputKey( Vector2 pos, InputButton button, Vector2? size = null )
		{
			var key = Input.GetButtonOrigin( button ).ToUpper();
			var pressed = Input.Down( button );
			size ??= new Vector2( 64, 64 );

			//
			// Draw box
			//
			var corners = new Vector4( 4 );
			var boxColor = pressed ? Color.White
								   : Color.White.WithAlpha( 0.1f );
			var boxBorderColor = pressed ? Color.White
										 : Color.White.WithAlpha( 0.2f );

			// This is obsolete, but what do we replace it with?
			draw.BoxWithBorder( new Rect( pos, size ?? default ), boxColor, 2f, boxBorderColor, corners );

			//
			// Draw text
			//
			var textSize = draw.TextSize( pos, key ).Size;
			var textCenter = ( ( size ?? default ) - textSize ) / 2.0f;

			draw.FontFamily = "Rajdhani";
			draw.FontWeight = 800;
			draw.Color = pressed ? Color.Black : Color.White;
			draw.Text( pos + textCenter, key );
		}

		Vector2 margins = new( 8, 8 );
		Vector2 start = new( screenSize.x - 64 - ( 64 + margins.x ) * 3, screenSize.y - 64 - ( 64 + margins.y ) * 3 );
		Vector2 position = start;

		position += new Vector2( 64, 0 ) + margins.WithY( 0 );
		DrawInputKey( position, InputButton.Forward );

		position += new Vector2( -64, 64 ) + margins.WithX( -margins.x );
		DrawInputKey( position, InputButton.Left );

		position += new Vector2( 64, 0 ) + margins.WithY( 0 );
		DrawInputKey( position, InputButton.Back );

		position += new Vector2( 64, 0 ) + margins.WithY( 0 );
		DrawInputKey( position, InputButton.Right );

		position = start + new Vector2( 0, 128 ) + new Vector2( 0, margins.y * 2 );
		DrawInputKey( position, InputButton.Jump, new Vector2( 192 + margins.x * 2, 64 ) );
	}

}

