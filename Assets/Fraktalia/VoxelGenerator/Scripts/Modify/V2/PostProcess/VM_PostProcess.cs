using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Fraktalia.Core.Collections;
using System;

namespace Fraktalia.VoxelGen.Modify
{
	[System.Serializable]
	public class VM_PostProcessContainer
	{
		public enum PostProcessType
		{
			None,
			Between,
			CopyPaste,
			Hardness,
			Indestructible,
			ShapeFill,
			Threshold,
			Destruction,
			SolidPaint
		}

		public string Name = "";
		public PostProcessType ProcessType;
		public bool Disabled;

		[SerializeReference] 
		private VM_PostProcess _postprocess = new VM_PostProcess_Nothing();
		[SerializeReference] [HideInInspector] private VM_PostProcess_Nothing _nothing = new VM_PostProcess_Nothing();
		[SerializeReference] [HideInInspector] private VM_PostProcess_Between _between = new VM_PostProcess_Between();
		[SerializeReference] [HideInInspector] private VM_PostProcess_CopyPaste _copypaste = new VM_PostProcess_CopyPaste();
		[SerializeReference] [HideInInspector] private VM_PostProcess_Destruction _destruction = new VM_PostProcess_Destruction();
		[SerializeReference] [HideInInspector] private VM_PostProcess_Hardness _hardness = new VM_PostProcess_Hardness();
		[SerializeReference] [HideInInspector] private VM_PostProcess_Indestructible _indestructible = new VM_PostProcess_Indestructible();
		[SerializeReference] [HideInInspector] private VM_PostProcess_ShapeFill _shapefill = new VM_PostProcess_ShapeFill();
		[SerializeReference] [HideInInspector] private VM_PostProcess_SolidPaint _solidpaint = new VM_PostProcess_SolidPaint();
		[SerializeReference] [HideInInspector] private VM_PostProcess_Threshold _threshold = new VM_PostProcess_Threshold();

		public VM_PostProcess PostProcess
		{
			get
			{
				if (_postprocess == null) _postprocess = new VM_PostProcess_Nothing();
				return _postprocess;
			}
			set
			{
				_postprocess = value;
			}
		}

		public void ConvertToDerivate()
		{
			CleanUp();
            switch (ProcessType)
            {
                case PostProcessType.None:
					if (_nothing == null) _nothing = new VM_PostProcess_Nothing();
					_postprocess = _nothing;
					break;
				case PostProcessType.Between:
					if (_between == null) _between = new VM_PostProcess_Between();
					_postprocess = _between;
					break;
                case PostProcessType.CopyPaste:
					if (_copypaste == null) _copypaste = new VM_PostProcess_CopyPaste();
					_postprocess = _copypaste;
					break;
                case PostProcessType.Hardness:
					if (_hardness == null) _hardness = new VM_PostProcess_Hardness();
					_postprocess = _hardness;
					break;
                case PostProcessType.Indestructible:
					if (_indestructible == null) _indestructible = new VM_PostProcess_Indestructible();
					_postprocess = _indestructible;				
					break;
                case PostProcessType.ShapeFill:
					if (_shapefill == null) _shapefill = new VM_PostProcess_ShapeFill();
					_postprocess = _shapefill;	
					break;
                case PostProcessType.Threshold:
					if (_threshold == null) _threshold = new VM_PostProcess_Threshold();
					_postprocess = _threshold;
					break;
                case PostProcessType.Destruction:
					if (_destruction == null) _destruction = new VM_PostProcess_Destruction();
					_postprocess = _destruction;
					break;
                case PostProcessType.SolidPaint:
					if (_solidpaint == null) _solidpaint = new VM_PostProcess_SolidPaint();
					_postprocess = _solidpaint;
					break;
                default:
                    break;
            }       
		}

		public void CleanUp()
		{			
			if (_postprocess != null) _postprocess.CleanUp();
		}
	}

	[System.Serializable]
	public class VM_PostProcess
	{
		//NOTE: Lists here as property will crash Unity when more than one target is selected in inspector, after clicling the + icon.

		public virtual void ApplyPostprocess(FNativeList<NativeVoxelModificationData_Inner> modifierData, VoxelGenerator generator, VoxelModifier_V2 modifier, VoxelModifierMode mode)
		{

		}

		public virtual void FinalizeModification(FNativeList<NativeVoxelModificationData_Inner> modifierData,
			FNativeList<NativeVoxelModificationData_Inner> preVoxelData,
			FNativeList<NativeVoxelModificationData_Inner> postVoxelData, VoxelGenerator generator, VoxelModifier_V2 modifier)
		{

		}

        public virtual void CleanUp()
        {
			
        }

		public virtual void DrawEditorPreview(VoxelModifier_V2 modifier, bool isSafe, Vector3 worldPosition, Vector3 worldNormal)
		{

		}	
	}

	[System.Serializable]
	public class VM_PostProcess_Nothing : VM_PostProcess { }
}
