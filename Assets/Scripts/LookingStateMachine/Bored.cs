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
    }

    public override void UpdateState(LookingStateManager looking)
    {
        if (!looking.waitingDone) return;
        looking.waitingDone = false;
        looking.Wait(Random.Range(2f, 10f));
        if (_boredCount >= 2)
        {
            looking.EaseEmotions();
        }
        _boredCount++;
        
        looking.DoAction(looking.BoredState);
        
        looking.lookingSpeed = Random.Range(0.2f, 0.35f);
        looking.ChoosePoint(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
    }

    public override void DoAction(LookingStateManager looking)
    {
        var randomNumber = Random.Range(0, 5);
        switch (randomNumber)
        {
            case 0:
                looking.StartSpecificEmotion(4, Random.Range(1f, 4f), Random.Range(0.1f, 0.4f));
                break;
            case 1:
                looking.StartSpecificEmotion(1, Random.Range(1f, 4f), Random.Range(0.1f, 0.4f));
                looking.StartSpecificEmotion(2, Random.Range(1f, 4f), Random.Range(0.1f, 0.4f));
                break;
            case 2:
                looking.StartSpecificEmotion(6, Random.Range(1f, 4f), Random.Range(0.1f, 0.4f));
                break;
            case 3:
                looking.StartSpecificEmotion(17, Random.Range(1f, 4f), Random.Range(0.1f, 0.4f));
                break;
            default:
                looking.StartSpecificMouth("Mouth_M", Random.Range(1f, 4f), Random.Range(0.1f, 0.4f));
                break;
        }
    }
}
