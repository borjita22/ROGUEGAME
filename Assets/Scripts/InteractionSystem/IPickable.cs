using UnityEngine;

public interface IPickable
{
    void PickUp(Transform parent);

    void Drop();
}
