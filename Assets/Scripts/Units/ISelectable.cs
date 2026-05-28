using UnityEngine;
namespace RTS.Units
{
    public interface ISelectable
    {
        void Select();
        void Deselect();
        void ApplyDecalProjectile(Vector3 position);
    }
}