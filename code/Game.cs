global using Sandbox;
global using SandboxEditor;
global using Sandbox.UI.Construct;
global using System;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;

namespace OpenArena;

public partial class ArenaGame : Sandbox.Game
{
	public ArenaGame()
	{
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
		}
	}
}
