using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fraktalia.VoxelGen.Modify
{
	[System.Serializable]
	public class TargetingModuleContainer
	{
		public enum TargetingModuleType
		{
			Default,
			SphereCast,
			MultiBlock,
			InfinitySystem
		}

		public List<VoxelGenerator> AlwaysModify = new List<VoxelGenerator>();
		public List<VoxelGenerator> NeverModify = new List<VoxelGenerator>();

		public TargetingModuleType TargetingType;
		private bool isInitilized;

		[SerializeReference] private VoxelModifier_Target _targetModule = new VoxelModifier_Target();
		[SerializeReference] [HideInInspector] private VoxelModifier_Target_Default _targetDefault = new VoxelModifier_Target_Default();
		[SerializeReference] [HideInInspector] private VoxelModifier_TargetSphereCast _targetSphereCast = new VoxelModifier_TargetSphereCast();
		[SerializeReference] [HideInInspector] private VoxelModifier_TargetMultiblock _targetMultiBlock = new VoxelModifier_TargetMultiblock();
		[SerializeReference] [HideInInspector] private VoxelModifier_TargetInfinitySystem _targetInfinitySystem = new VoxelModifier_TargetInfinitySystem();

		public VoxelModifier_Target TargetingModule
		{
			get
			{
				if (_targetModule == null) _targetModule = new VoxelModifier_Target();
				return _targetModule;
			}
			set
			{
				_targetModule = value;
			}
		}

		public void ConvertToDerivate()
		{	
			switch (TargetingType)
			{				
				case TargetingModuleType.Default:
					if (_targetDefault == null) _targetDefault = new VoxelModifier_Target_Default();
					_targetModule = _targetDefault;
					break;
				case TargetingModuleType.SphereCast:
					if (_targetSphereCast == null) _targetSphereCast = new VoxelModifier_TargetSphereCast();
					_targetModule = _targetSphereCast;
					break;
				case TargetingModuleType.MultiBlock:
					if (_targetMultiBlock == null) _targetMultiBlock = new VoxelModifier_TargetMultiblock();
					_targetModule = _targetMultiBlock;
					break;
				case TargetingModuleType.InfinitySystem:
					if (_targetInfinitySystem == null) _targetInfinitySystem = new VoxelModifier_TargetInfinitySystem();
					_targetModule = _targetInfinitySystem;
					break;
				default:
					break;
			}
		}	

		public VoxelGenerator Reference
        {
			get
			{
				if (AlwaysModify.Count > 0)
				{
					return AlwaysModify[0];
				}

				return _targetModule.Reference;				
			}
		}

        internal void FetchGenerators(List<VoxelGenerator> targets, Vector3 worldPosition, Vector3 extents)
        {		
			targets.AddRange(AlwaysModify);
			_targetModule.FetchGenerators(targets, worldPosition, extents);

            for (int i = 0; i < NeverModify.Count; i++)
            {
				
				if(targets.Contains(NeverModify[i]))
                {
					targets.Remove(NeverModify[i]);
                }
            }

        }
    }

	[System.Serializable]
	public class VoxelModifier_Target
	{
		public virtual VoxelGenerator Reference
		{
			get
			{				
				return null;
			}
		}
		
		public virtual void FetchGenerators(List<VoxelGenerator> targets, Vector3 worldPosition, Vector3 extents)
		{
			
		}
	}

	[System.Serializable]
	public class VoxelModifier_Target_Default : VoxelModifier_Target { }
}
