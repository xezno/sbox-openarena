namespace OpenArena;

public static class DamageInfoExtensions
{
	public static bool IsHeadshot( this DamageInfo damageInfo )
	{
		return damageInfo.BoneIndex == 5; // Head bone is 5... TODO: add some sort of bone enum
	}
}
