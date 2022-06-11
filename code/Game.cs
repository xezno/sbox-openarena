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

namespace OpenArena;

public partial class ArenaGame : Sandbox.Game
{
	public ArenaGame()
	{
		if ( IsServer )
			_ = new Hud();
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		pawn.Respawn();
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
		void DrawInputKey( Vector2 pos, string key, InputButton button, Vector2? size = null )
		{
			size ??= new Vector2( 64, 64 );

			bool pressed = Input.Down( button );
			var color = pressed ? Color.White : Color.White.WithAlpha( 0.1f );
			var corners = new Vector4( 8 );

			draw.Color = Color.White;
			draw.BoxWithBorder( new Rect( pos, size ?? default ), color, 2f, Color.White, corners );

			var textSize = draw.TextSize( pos, key );
			var center = ( size ?? default ) / 2.0f - new Vector2( textSize.width / 2.0f, textSize.height / 2.0f );

			draw.FontFamily = "Rajdhani";
			draw.FontWeight = 800;
			draw.Color = pressed ? Color.Black : Color.White;

			draw.Text( pos + center, key );
		}

		Vector2 start = new( screenSize.x - 128 - 32 - 64, screenSize.y - 128 - 32 - 64 );

		DrawInputKey( start + new Vector2( 64, 0 ), "W", InputButton.Forward );
		DrawInputKey( start + new Vector2( 0, 64 ), "A", InputButton.Left );
		DrawInputKey( start + new Vector2( 64, 64 ), "S", InputButton.Back );
		DrawInputKey( start + new Vector2( 128, 64 ), "D", InputButton.Right );

		DrawInputKey( start + new Vector2( 0, 128 ), "Jump", InputButton.Jump, new Vector2( 192, 64 ) );
	}

}

