using UnityEngine;

public interface IPickable
{
    public PickableWeight weight { get; }
    void PickUp(Transform parent);

    void Drop();
}

public enum PickableWeight
{
    Light,
    Heavy
}
