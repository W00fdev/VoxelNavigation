using Fraktalia.Core.Collections;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Fraktalia.VoxelGen.Modify
{
	[System.Serializable]
	public class VoxelShapeContainer
	{
		public enum VoxelShapeType
        {
			Sphere,
			Box,
			Ellypse,
			Single,
			Diamond,
			Polygon
        }

		public VoxelShapeType ShapeType;

		[SerializeReference] private VoxelShape_Base _shape = new VoxelShape_Base();
		[SerializeReference] [HideInInspector] private VoxelShape_Sphere _shapeSphere = new VoxelShape_Sphere();
		[SerializeReference] [HideInInspector] private VoxelShape_Box _shapeBox = new VoxelShape_Box();
		[SerializeReference] [HideInInspector] private VoxelShape_Ellipsoid _shapeEllipsoid = new VoxelShape_Ellipsoid();
		[SerializeReference] [HideInInspector] private VoxelShape_Single _shapeSingle = new VoxelShape_Single();
		[SerializeReference] [HideInInspector] private VoxelShape_Diamond _shapeDiamond = new VoxelShape_Diamond();
		[SerializeReference] [HideInInspector] private VoxelShape_Polygon _shapePolygon = new VoxelShape_Polygon();

		public VoxelShape_Base VoxelShape
		{
			get
			{

				if (_shape == null) _shape = new VoxelShape_Base();
				return _shape;
			}
			set
			{
				_shape = value;
			}
		}

		public void ConvertToDerivate()
		{
			CleanUp();
            switch (ShapeType)
            {             
                case VoxelShapeType.Sphere:
					_shape = _shapeSphere;	
					break;
                case VoxelShapeType.Box:
					_shape = _shapeBox;
					break;
                case VoxelShapeType.Ellypse:
					_shape = _shapeEllipsoid;
					break;
				case VoxelShapeType.Single:
					_shape = _shapeSingle;
					break;
				case VoxelShapeType.Diamond:
					_shape = _shapeDiamond;
					break;
				case VoxelShapeType.Polygon:
					_shape = _shapePolygon;
					break;
				default:
                    break;
            }           
		}

		public void CleanUp()
		{	
			if (VoxelShape != null) VoxelShape.CleanUp();
		}
	}

	[System.Serializable]
	public class VoxelShape_Base
	{

		public bool ApplyObjectRotation;
		
		
		public FNativeList<NativeVoxelModificationData_Inner> ModifierTemplateData;

		protected Vector3 displacement;
		public int boundaryExtension = 1;

		protected float checksum = 0;

		public virtual void DrawEditorPreview(VoxelModifier_V2 modifier, bool isSafe, Vector3 worldPosition, Vector3 worldNormal)
		{

		}

		public virtual Vector3 GetGameIndicatorSize(VoxelModifier_V2 modifier)
		{
			return Vector3.one;
		}

		public void CreateModifierTemplate(Vector3 worldPosition, VoxelModifier_V2 modifier, VoxelGenerator target)
		{
			if (RequiresRecalculation(modifier, target) || !ModifierTemplateData.IsCreated)
			{
				float voxelsize = target.GetVoxelSize(modifier.Depth);
				Vector3 localcoord = target.transform.worldToLocalMatrix.MultiplyPoint(worldPosition);
				displacement = new Vector3(localcoord.x % voxelsize, localcoord.y % voxelsize, localcoord.z % voxelsize);

				if (!ModifierTemplateData.IsCreated) ModifierTemplateData = new FNativeList<NativeVoxelModificationData_Inner>(Allocator.Persistent);
				ModifierTemplateData.Clear();

				calculateTemplateData(modifier, target);
			}
		}

		protected virtual void calculateTemplateData(VoxelModifier_V2 modifier, VoxelGenerator target)
		{

		}

		public virtual void SetGeneratorDirty(VoxelModifier_V2 modifier, VoxelGenerator target, Vector3 worldPosition)
		{

		}

		public virtual Vector3 GetOffset(VoxelModifier_V2 modifier, VoxelGenerator target)
		{
			return Vector3.zero;
		}

		public virtual int GetVoxelModificationCount(VoxelModifier_V2 modifier, VoxelGenerator target)
		{
			return 0;
		}

		public void CleanUp()
		{
			checksum = float.MaxValue;
			if (ModifierTemplateData.IsCreated) ModifierTemplateData.Dispose();
		
		}

		public virtual bool RequiresRecalculation(VoxelModifier_V2 modifier, VoxelGenerator target)
		{			
			return true;
		}
		
		protected virtual float getCheckSum()
		{
			return 0;
		}

		protected Quaternion calculateRotation(Vector3 eulerRotation, VoxelModifier_V2 modifier)
		{
			Quaternion rot = Quaternion.Euler(eulerRotation);
			if (ApplyObjectRotation)
			{
				rot *= modifier.transform.rotation;
			}
			return rot;
		}
	}
}
