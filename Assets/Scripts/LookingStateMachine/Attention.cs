using System.Collections;
using System.Collections.Generic;
using LookingStateMachine;
using UnityEngine;

public class Attention : LookingBaseState
{
    private int _boredCounter;
    
    public override void EnterState(LookingStateManager looking)
    {
        Debug.Log("attention");
        looking.dartingSpeedUpperEnd = 0.5f;
        looking.dartingSpeedLowerEnd = 1.3f;
        
        looking.lookingSpeed = Random.Range(0.2f, 0.45f);
        
        looking.ChoosePoint(Random.Range(-0.1f, 0.1f), Random.Range(-0.3f, 0.3f));
        looking.thinking = false;
    }

    public override void UpdateState(LookingStateManager looking)
    {
        if (!looking.waitingDone) return;
        looking.waitingDone = false;
        
        looking.StartSpecificBody(45, Random.Range(-0.05f, 0.05f), Random.Range(2f, 5f));
        looking.StartSpecificBody(46, 0, Random.Range(2f, 5f));
        looking.StartSpecificBody(29, 0, Random.Range(1f, 2f));
        
        looking.Wait(Random.Range(3f, 13f));
        _boredCounter++;

        var distractionChance = Random.Range(1, 3);
        
        if (distractionChance == 1)
        {
            looking.StartDistracted();
        }
        else if (_boredCounter >= 4)
        {
            _boredCounter = 0;
            looking.SwitchState(looking.BoredState);
        }
        else
        {
            looking.lookingSpeed = Random.Range(0.2f, 0.45f);
            looking.ChoosePoint(Random.Range(-0.1f, 0.1f), Random.Range(-0.3f, 0.3f));
        }
    }
    
    public override void DoAction(LookingStateManager looking)
    {
        
    }
}
