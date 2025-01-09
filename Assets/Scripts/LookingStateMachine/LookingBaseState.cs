using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LookingBaseState
{
    public abstract void EnterState(LookingStateManager looking);
    public abstract void UpdateState(LookingStateManager looking);
}
