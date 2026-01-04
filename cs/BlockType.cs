public struct BlockType
{
	public ushort ID;

	public string Name;

	public bool IsTransparent;

	public ushort TextureTop;
	public ushort TextureBottom;
	public ushort TextureNorth;
	public ushort TextureSouth;
	public ushort TextureWest;
	public ushort TextureEast;


	public BlockType(ushort ID, string Name)
	{
		this.ID = ID;
		this.Name = Name;

		IsTransparent = false;
		SetAllTextures(0);
	}

	public BlockType SetTransparency(bool IsTransparent)
	{
		this.IsTransparent = IsTransparent;
		return this;
	}

	public BlockType SetAllTextures(ushort texture)
	{
		this.TextureTop = texture;
		this.TextureBottom = texture;
		this.TextureNorth = texture;
		this.TextureSouth = texture;
		this.TextureWest = texture;
		this.TextureEast = texture;

		return this;
	}

	public BlockType SetAllTextures(ushort TextureTop, ushort TextureBottom, ushort TextureNorth, ushort TextureSouth, ushort TextureWest, ushort TextureEast)
	{
		this.TextureTop = TextureTop;
		this.TextureBottom = TextureBottom;
		this.TextureNorth = TextureNorth;
		this.TextureSouth = TextureSouth;
		this.TextureWest = TextureWest;
		this.TextureEast = TextureEast;

		return this;
	}

	public readonly ulong Compress()
	{
		return ID;
	}
}