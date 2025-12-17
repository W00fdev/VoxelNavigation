using Fraktalia.VoxelGen.Modify;
using UnityEngine;

namespace Fraktalia.VoxelGen.Samples
{
    public class MouseToVoxel_V2 : MonoBehaviour
	{

		public VoxelModifier_V2 Modifier;
		public VoxelModifierMode ModeLeftClick;
		public VoxelModifierMode ModeRightClick;
		
		public Camera SourceCamera;
		public Transform TargetPoint;

		public LayerMask TargetLayer = -1;
		public KeyCode ActivationButton = KeyCode.LeftControl;

		public float MaxDistance = 2000;

		public bool ShotGunEffect = false;
		public float ShotGunPower;

		private bool hashit;
		private void Start()
		{
			if (!Modifier) Modifier = GetComponent<VoxelModifier_V2>();
		}

		void FixedUpdate()
		{
			if (!Modifier || !SourceCamera) return;

			Vector3 mousePosition = Input.mousePosition;
			if (ShotGunEffect)
			{
				mousePosition += Random.insideUnitSphere * ShotGunPower;
			}

			Ray ray = SourceCamera.ScreenPointToRay(mousePosition);

			RaycastHit hit;
			if (Input.GetKey(ActivationButton) || ActivationButton == KeyCode.None)
			{
				if (Physics.Raycast(ray, out hit, MaxDistance, TargetLayer))
				{

					hashit = true;
					Vector3 point = hit.point;
					
					if (TargetPoint)
					{
						TargetPoint.position = point;

						if (Modifier.ReferenceGenerator)
						{
							TargetPoint.localScale = Vector3.one * Modifier.ReferenceGenerator.GetVoxelSize(Modifier.Depth);
						}
						else
						{
							TargetPoint.localScale = Modifier.ShapeModule.VoxelShape.GetGameIndicatorSize(Modifier);
						}
					}

					if (Input.GetMouseButton(0))
					{					
						Modifier.ApplyVoxelModifier(point, ModeLeftClick);
					}
					else if (Input.GetMouseButton(1))
					{
						Modifier.ApplyVoxelModifier(point, ModeRightClick);
					}
				}
				else
				{
					hashit = false;
				}
			}
			else
			{
				hashit = false;

				if (TargetPoint)
					TargetPoint.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if (!Modifier || !SourceCamera || !TargetPoint) return;

			if (Input.GetKey(ActivationButton) || ActivationButton == KeyCode.None)
			{
				TargetPoint.gameObject.SetActive(hashit);
				TargetPoint.transform.localScale = Modifier.ShapeModule.VoxelShape.GetGameIndicatorSize(Modifier);	
			}
			else
			{
				TargetPoint.gameObject.SetActive(false);
			}
		}
	}
}
