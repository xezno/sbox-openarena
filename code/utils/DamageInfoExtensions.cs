namespace OpenArena;

public static class DamageInfoExtensions
{
	public static bool IsHeadshot( this DamageInfo damageInfo )
	{
		return damageInfo.BoneIndex == 11; // Head bone is 11... TODO: add some sort of bone enum
	}
}
