using Godot;

public static class BlockTable
{
	private static readonly BlockType[] Table =
	{
		new BlockType(0, "Air").SetTransparency(true),
		new BlockType(1, "Grass").SetAllTextures(2, 3, 1, 1, 1, 1),
		new BlockType(2, "Dirt").SetAllTextures(3)
	};

	public static BlockType Get(ulong block)
	{
		return Table[block & 65535];
	}
}