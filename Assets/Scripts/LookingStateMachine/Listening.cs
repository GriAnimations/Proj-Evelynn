using UnityEngine;

namespace LookingStateMachine
{
    public class Listening : LookingBaseState
    {
        
        
        public override void EnterState(LookingStateManager looking)
        {
            Debug.Log("listening");
            
            looking.dartingSpeedUpperEnd = 0.5f;
            looking.dartingSpeedLowerEnd = 1.3f;
            
            looking.lookingSpeed = Random.Range(0.2f, 0.45f);
            
            looking.StartSpecificBody(45, Random.Range(-0.05f, 0.05f), Random.Range(2f, 5f));
            looking.StartSpecificBody(46, 0, Random.Range(2f, 5f));
            looking.StartSpecificBody(29, 0, Random.Range(1f, 2f));
            
            looking.ChoosePoint(0, 0);
            looking.thinking = false;
            
            //tilt head
            //perch up a lil and ease around
        }

        public override void UpdateState(LookingStateManager looking)
        {
            
        }

        public override void DoAction(LookingStateManager looking)
        {
            
        }
    }
    
}
