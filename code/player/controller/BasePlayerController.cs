﻿namespace Fortress;

[Library]
public abstract class BasePlayerController : PawnController
{
	[ConVar.Replicated( "ft_debug_playercontroller" )]
	public static bool Debug { get; set; } = false;

	/// <summary>
	/// Any bbox traces we do will be offset by this amount.
	/// todo: this needs to be predicted
	/// </summary>
	public Vector3 TraceOffset;

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		//pFilter->m_attr.SetCollisionGroup( collisionGroup ); // COLLISION_GROUP_PLAYER_MOVEMENT
		// pFilter->m_attr.SetHitSolidRequiresGenerateContacts( true );

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.HitLayer( CollisionLayer.All, false )
					.HitLayer( CollisionLayer.Solid, true )
					.HitLayer( CollisionLayer.GRATE, true )
					.HitLayer( CollisionLayer.PLAYER_CLIP, true )
					.HitLayer( CollisionLayer.WINDOW, true )
					.HitLayer( CollisionLayer.NPC, true )
					.Ignore( Pawn )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	/// <summary>
	/// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
	/// want to use the built in functions
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Vector3.One * -1, Vector3.One, liftFeet );
	}

	/// <summary>
	/// This is temporary, get the hull size for the player's collision
	/// </summary>
	public virtual BBox GetHull()
	{
		return new BBox( -10, 10 );
	}


	public override void FrameSimulate()
	{
		base.FrameSimulate();

		EyeRotation = Input.Rotation;
	}
}
