using Godot;

public static class BlockTable
{
	private static readonly BlockType[] Table =
	{
		new BlockType(0, "Air").SetInvisibility(true),
		new BlockType(1, "Grass").SetAllTextures(2, 3, 1, 1, 1, 1),
		new BlockType(2, "Dirt").SetAllTextures(3),
		new BlockType(3, "idk").SetAllTextures(0),
		new BlockType(4, "Water").SetAllTextures(4).SetTransparency(true)
	};

	public static BlockType Get(ulong block)
	{
		return Table[block & 65535];
	}
}