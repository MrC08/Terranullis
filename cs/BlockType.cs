public struct BlockType
{
	public ushort ID;

	public string Name;

	public bool IsTransparent;
	public bool IsInvisible;

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
		IsInvisible = false;
		SetAllTextures(0);
	}

	public BlockType SetTransparency(bool IsTransparent)
	{
		this.IsTransparent = IsTransparent;
		return this;
	}

	public BlockType SetInvisibility(bool IsInvisible)
	{
		this.IsInvisible = IsInvisible;
		if (IsInvisible)
			this.IsTransparent = true;
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

	public override readonly bool Equals(object obj)
	{
		if (obj == null)
			return false;

		return Compress() == ((BlockType) obj).Compress();
	}

	public static bool operator ==(BlockType left, BlockType right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BlockType left, BlockType right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		throw new System.NotImplementedException();
	}
}