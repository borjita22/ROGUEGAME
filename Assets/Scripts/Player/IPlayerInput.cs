using System;
using UnityEngine;

public interface IPlayerInput
{
    Vector2 MovementInput { get; }
    Vector2 AimingInput { get;  }

    bool isAttacking { get; }


    event Action OnAttack;
}
