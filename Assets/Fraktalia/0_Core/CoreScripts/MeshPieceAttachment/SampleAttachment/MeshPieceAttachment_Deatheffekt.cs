using UnityEngine;
using System.Collections;
using Fraktalia.Utility;

namespace Fraktalia.Core.LMS
{
	[ExecuteInEditMode]
    public class MeshPieceAttachment_Deatheffekt : MeshPieceAttachment
    {
        public GameObject DeathEffekt;
        private bool Initialized = false;
        public override void Effect(GameObject piece)
        {
            MeshPieceAttachment_Deatheffekt decay = piece.AddComponent<MeshPieceAttachment_Deatheffekt>();
            decay.DeathEffekt = DeathEffekt;
            decay.Initialized = true;
        }

        private void OnDestroy()
        {
            if (!Initialized) return;
            Transform instance = Instantiate(DeathEffekt, transform.position, transform.rotation, null).transform;
            instance.transform.position = transform.position;
            instance.transform.rotation = transform.rotation;
        }
    }
}
