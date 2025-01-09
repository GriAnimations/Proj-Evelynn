using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bored : LookingBaseState
{
    
    
    
    public override void EnterState(LookingStateManager looking)
    {
        
    }

    public override void UpdateState(LookingStateManager looking)
    {
        if (!looking.waitingDone) return;
        looking.waitingDone = false;
        looking.Wait(Random.Range(2f, 4f));
        
        looking.lookingSpeed = Random.Range(0.1f, 0.35f);
        looking.ChooseLookPoint(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
    }

    private void ChoosePoint()
    {
        
    }
}
