public static class ObjectExtensions
{
	public static string GetLibraryName( this object obj )
	{
		return TypeLibrary.GetDescription( obj.GetType() )?.GetAttribute<LibraryAttribute>()?.Name ?? "none";
	}
}
