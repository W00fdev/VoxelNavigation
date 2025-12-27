using UnityEngine;

namespace Map
{
    public static class MapCuller
    {
        public static void Cull(ref bool[,,] voxels, out int totalFilledVoxelsCount)
        {
			int width = voxels.GetLength(0);
			int height = voxels.GetLength(1);
			int depth = voxels.GetLength(2);
			totalFilledVoxelsCount = 0;

			for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (voxels[x, y, z])
                        {
							if (x + 1 >= width
	                        || y + 1 >= height
	                        || y - 1 <= 0
	                        || x - 1 <= 0
	                        || z + 1 >= depth
	                        || z - 1 <= 0)
							{
								totalFilledVoxelsCount++;
								continue;
							}

							voxels[x, y, z] = !(
								voxels[x + 1, y, z]
							 && voxels[x - 1, y, z]
							 && voxels[x, y + 1, z]
							 && voxels[x, y - 1, z]
							 && voxels[x, y, z + 1]
							 && voxels[x, y, z - 1]
							 );

							if (voxels[x, y, z])
								totalFilledVoxelsCount++;
							else
							{
								UnityEngine.Debug.Log("Voxel culled");
							}
						}
                    }
                }
            }
        }
    }
}