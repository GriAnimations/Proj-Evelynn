using System.Collections;
using System.Collections.Generic;
using LookingStateMachine;
using UnityEngine;

public class Bored : LookingBaseState
{
    private int _boredCount;
    
    public override void EnterState(LookingStateManager looking)
    {
        looking.dartingSpeedUpperEnd = 0.5f;
        looking.dartingSpeedLowerEnd = 1.3f;
        
        looking.lookingSpeed = Random.Range(0.2f, 0.35f);
        
        looking.ChoosePoint(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
        looking.thinking = false;
        looking.EaseEmotions();
        Debug.Log("bored now");
    }

    public override void UpdateState(LookingStateManager looking)
    {
        if (!looking.waitingDone) return;
        looking.waitingDone = false;
        looking.Wait(Random.Range(5f, 15f));
        if (_boredCount >= 2)
        {
            looking.EaseEmotions();
        }
        _boredCount++;
        
        looking.lookingSpeed = Random.Range(0.2f, 0.35f);
        looking.ChoosePoint(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
    }

    public override void DoAction(LookingStateManager looking)
    {
        
    }
}
