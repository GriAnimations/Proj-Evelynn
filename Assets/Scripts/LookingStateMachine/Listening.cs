using UnityEngine;

namespace LookingStateMachine
{
    public class Listening : LookingBaseState
    {
        
        
        public override void EnterState(LookingStateManager looking)
        {
            looking.dartingSpeedUpperEnd = 0.5f;
            looking.dartingSpeedLowerEnd = 1.3f;
            
            looking.lookingSpeed = Random.Range(0.2f, 0.45f);
            
            looking.StartSpecificBody(45, Random.Range(-0.05f, 0.05f), Random.Range(2f, 5f));
            looking.StartSpecificBody(46, 0, Random.Range(2f, 5f));
            looking.StartSpecificBody(29, Random.Range(-0.2f, 0.2f), Random.Range(1f, 2f));
            
            looking.ChoosePoint(0, 0);
            looking.thinking = false;
            
            //perch up a lil and ease around
        }

        public override void UpdateState(LookingStateManager looking)
        {
            if (!looking.waitingDone) return;
            looking.waitingDone = false;
            looking.Wait(Random.Range(3f, 15f));
            
            if (looking.automaticHead)
            {
                looking.StartNod(Random.Range(0.35f, 0.45f), Random.Range(1, 4));
                looking.StartSpecificEmotion(12, Random.Range(1f, 3f), Random.Range(0.1f, 0.4f));
            }
            
            looking.StartSpecificBody(45, Random.Range(-0.15f, 0.15f), Random.Range(2f, 5f));
            looking.StartSpecificBody(46, Random.Range(-0.5f, 0.5f), Random.Range(2f, 5f));
            looking.StartSpecificBody(29, Random.Range(-0.6f, 0.6f), Random.Range(1f, 2f));
            looking.StartSpecificMouth("Mouth_AH", Random.Range(2f, 2.5f), Random.Range(0f, 0.2f));
        }

        public override void DoAction(LookingStateManager looking)
        {
            
        }
    }
    
}
