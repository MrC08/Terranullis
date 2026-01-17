using Godot;

public enum BiomeType
{
	ICE_SHEET, TUNDRA, BOREAL_FOREST, BOGLANDS, MAGELLANIC_RAINFOREST,
	SHRUBLANDS, PRARIE, FOREST, TEMPERATE_RAINFOREST,
	DESERT, EXTREME_DESERT, TROPICAL_RAINFOREST, JUNGLE, PAMPAS,
	SHALLOW_OCEAN, DEEP_OCEAN, SEA_ICE, CORAL_REEF
}

//				Xeric			Dry				Moderate		Wet				Drenched

//Ex. cold		Tundra			Tundra			Ice sheet		Ice sheet		Ice sheet
//Very cold		Tundra			Tundra			Boreal forest	Boreal forest	Boglands		
//Cold			Tundra			Boreal forest	Boreal forest	Boglands		Magellanic Rainforest

//Temerpate		Desert			Prarie			Forest			Forest			Temperate Rainforest	

//Hot			Desert			Shrublands		Prarie			Forest			Tropical Rainforest
//Very hot		Extreme desert	Desert			Pampas			Jungle			Tropical Rainforest
//Ex hot		Extreme desert	Desert			Jungle			Jungle			Tropical Rainforest

public static class BiomeHelper
{
	const float EX_COLD = 0.05f;
	const float VERY_COLD = 0.2f;
	const float COLD = 0.4f;
	const float TEMPERATE = 0.6f;
	const float HOT = 0.8f;
	const float VERY_HOT = 0.9f;

	const float XERIC = 0.2f;
	const float DRY = 0.4f;
	const float MODERATE = 0.6f;
	const float WET = 0.8f;

	public static Color GetBiomeColor(BiomeType biome)
	{
		return (new Color[] {
			new Color(0.95f, 0.95f, 1.0f),		// Ice sheet
			new Color(0.39f, 0.262f, 0.207f),	// Tundra
			new Color(0.211f, 0.34f, 0.303f),	// Boreal forest
			new Color(0.22f, 0.243f, 0.239f),	// Boglands
			new Color(0.0f, 0.3f, 0.225f),		// Magellanic fainforest

			new Color(0.41f, 0.57f, 0.336f),	// Shrublands
			new Color(0.494f, 0.65f, 0.357f),	// Prarie
			new Color(0.311f, 0.62f, 0.242f),	// Forest
			new Color(0.235f, 0.44f, 0.189f),	// Temperate rainforest

			new Color(0.732f, 0.77f, 0.308f),	// Desert
			new Color(0.77f, 0.539f, 0.308f),	// Extreme Desert
			new Color(0.426f, 1.0f, 0.16f),		// Tropical rainforest
			new Color(0.28f, 0.82f, 0.172f),	// Jungle
			new Color(0.373f, 0.8f, 0.16f),		// Pampas

			new Color(0.216f, 0.408f, 0.531f),	// Shallow Ocean
			new Color(0.173f, 0.33f, 0.51f),	// Deep Ocean
			new Color(0.376f, 0.532f, 0.71f),	// Sea Ice
			new Color(0.405f, 0.703f, 0.71f)	// Coral Reef
		}) [(int) biome];
	}

	public static BiomeType GetBiome(float temperature, float humidity, float elevation)
	{
		if (elevation <= 0)
		{
			if (temperature < VERY_COLD)
			{
				return BiomeType.SEA_ICE;
			} else if (temperature < HOT)
			{
				if (elevation > -0.5)
					return BiomeType.SHALLOW_OCEAN;
				else
					return BiomeType.DEEP_OCEAN;
			} else
			{
				if (elevation > -0.5)
					return BiomeType.CORAL_REEF;
				else
					return BiomeType.DEEP_OCEAN;
			}
		}
		
		if (temperature < EX_COLD / 2f)
		{
			return BiomeType.ICE_SHEET;
		} else if (temperature < EX_COLD)
		{
			if (humidity < DRY)
				return BiomeType.ICE_SHEET;
			else
				return BiomeType.TUNDRA;
		} else if (temperature < VERY_COLD)
		{
			if (humidity < DRY)
			{
				return BiomeType.TUNDRA;
			} else if (humidity < WET)
			{
				return BiomeType.BOREAL_FOREST;
			} else
			{
				return BiomeType.BOGLANDS;
			}
		} else if (temperature < COLD)
		{
			if (humidity < XERIC)
			{
				return BiomeType.TUNDRA;
			} else if (humidity < MODERATE)
			{
				return BiomeType.BOREAL_FOREST;
			} else if (humidity < WET)
			{
				return BiomeType.BOGLANDS;
			} else
			{
				return BiomeType.MAGELLANIC_RAINFOREST;
			}
		} else if (temperature < TEMPERATE)
		{
			if (humidity < XERIC)
			{
				return BiomeType.DESERT;
			} else if (humidity < DRY)
			{
				return BiomeType.PRARIE;
			} else if (humidity < WET)
			{
				return BiomeType.FOREST;
			} else
			{
				return BiomeType.TEMPERATE_RAINFOREST;
			}
		} else if (temperature < HOT)
		{
			if (humidity < XERIC)
			{
				return BiomeType.DESERT;
			} else if (humidity < DRY)
			{
				return BiomeType.SHRUBLANDS;
			} else if (humidity < MODERATE)
			{
				return BiomeType.PRARIE;
			} else if (humidity < WET)
			{
				return BiomeType.FOREST;
			} else
			{
				return BiomeType.TROPICAL_RAINFOREST;
			}
		} else if (temperature < VERY_HOT)
		{
			if (humidity < XERIC)
			{
				return BiomeType.EXTREME_DESERT;
			} else if (humidity < DRY)
			{
				return BiomeType.DESERT;
			} else if (humidity < MODERATE)
			{
				return BiomeType.PAMPAS;
			} else if (humidity < WET)
			{
				return BiomeType.FOREST;
			} else
			{
				return BiomeType.TROPICAL_RAINFOREST;
			}
		} else
		{
			if (humidity < XERIC)
			{
				return BiomeType.EXTREME_DESERT;
			} else if (humidity < DRY)
			{
				return BiomeType.DESERT;
			} else if (humidity < WET)
			{
				return BiomeType.JUNGLE;
			} else
			{
				return BiomeType.TROPICAL_RAINFOREST;
			}
		}
	}
}