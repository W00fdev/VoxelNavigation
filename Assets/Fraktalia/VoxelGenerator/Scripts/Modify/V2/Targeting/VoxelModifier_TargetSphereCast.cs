using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fraktalia.VoxelGen.Infinity;

namespace Fraktalia.VoxelGen.Modify
{

	[System.Serializable]
	public class VoxelModifier_TargetSphereCast : VoxelModifier_Target
	{
		public LayerMask SphereCastLayer = int.MaxValue;
		public int Maximum = 3;
		public float Radius = 10;

		private VoxelGenerator lastReference;
		public override VoxelGenerator Reference {
			get
			{
				if(lastReference == null)
				{
					return base.Reference;
				}

				return lastReference;
			}
			
		}
		public override void FetchGenerators(List<VoxelGenerator> targets, Vector3 worldPosition, Vector3 extents)
		{
			base.FetchGenerators(targets, worldPosition, extents);

			List<VoxelGenerator> fetchedGenerators = FindGeneratorsBySphereCast(worldPosition, Radius, SphereCastLayer);

			for (int i = 0; i < fetchedGenerators.Count; i++)
			{
				if (i >= Maximum) break;
				VoxelGenerator generator = fetchedGenerators[i];
				if(!targets.Contains(generator))
				{
					targets.Add(generator);
					if (i == 0) lastReference = fetchedGenerators[i];
				}			
			}
		}

		public List<VoxelGenerator> FindGeneratorsBySphereCast(Vector3 worldPosition, float radius, LayerMask mask)
		{
			Collider[] impact = Physics.OverlapSphere(worldPosition, radius, mask);

			List<VoxelGenerator> result = new List<VoxelGenerator>();
			for (int i = 0; i < impact.Length; i++)
			{
				VoxelGenerator generator = impact[i].GetComponentInParent<VoxelGenerator>();
				if (generator)
				{
					result.Add(generator);
				}
			}
			return result;
		}
	}

	[System.Serializable]
	public class VoxelModifier_TargetMultiblock : VoxelModifier_Target
	{
		public VoxelGeneratorMultiblock MultiBlock;
		public List<VoxelGeneratorMultiblock> AdditionalMultiBlocks = new List<VoxelGeneratorMultiblock>();

		public override VoxelGenerator Reference
		{
			get
			{
				if (MultiBlock) return MultiBlock.Reference;

				return null;
			}

		}
		public override void FetchGenerators(List<VoxelGenerator> targets, Vector3 worldPosition, Vector3 extents)
		{
			base.FetchGenerators(targets, worldPosition, extents);

			if (MultiBlock)
			{
				MultiBlock.WorldPositionToGenerators(targets, worldPosition - extents, worldPosition + extents);
			}

            for (int i = 0; i < AdditionalMultiBlocks.Count; i++)
            {
				if(AdditionalMultiBlocks[i])
				AdditionalMultiBlocks[i].WorldPositionToGenerators(targets, worldPosition - extents, worldPosition + extents);
			}
		}
	}
	
	public class VoxelModifier_TargetInfinitySystem : VoxelModifier_Target
	{
		public InfinityVoxelSystem InfinitySystem;
		public List<InfinityVoxelSystem> AdditionalInfinitySystems = new List<InfinityVoxelSystem>();

		public override VoxelGenerator Reference
		{
			get
			{
				if (InfinitySystem && InfinitySystem.Initialized) return InfinitySystem.ChunkTemplate.Generator;

				return null;
			}

		}
		public override void FetchGenerators(List<VoxelGenerator> targets, Vector3 worldPosition, Vector3 extents)
		{
			base.FetchGenerators(targets, worldPosition, extents);

			if (InfinitySystem)
			{
				if (InfinitySystem.Initialized)
					InfinitySystem.WorldPositionToGenerators(worldPosition, extents, extents, targets);
			}

			for (int i = 0; i < AdditionalInfinitySystems.Count; i++)
			{
				if (AdditionalInfinitySystems[i])
				{
					if (AdditionalInfinitySystems[i].Initialized)
						AdditionalInfinitySystems[i].WorldPositionToGenerators(worldPosition, extents, extents, targets);
				}
			}
		}
	}

}
