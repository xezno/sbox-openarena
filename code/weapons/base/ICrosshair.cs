namespace OpenArena;

public interface ICrosshair
{
	//
	// TODO:
	// Using timeSinceAttack here directly is a bit shit. It'd be nice
	// if it could just be passed a float from 0-1 instead
	//
	void RenderHud( TimeSince timeSinceAttack, Vector2 screenSize );
}
