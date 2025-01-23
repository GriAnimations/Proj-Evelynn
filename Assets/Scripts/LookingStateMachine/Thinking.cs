using System.Collections;
using UnityEngine;

namespace LookingStateMachine
{
    public class Thinking : LookingBaseState
    {
        private bool _doneThinking;
        private bool _switchAction;
        
        public override void EnterState(LookingStateManager looking)
        {
            _doneThinking = false;
            
            looking.dartingSpeedUpperEnd = 0.5f;
            looking.dartingSpeedLowerEnd = 1.3f;
            
            looking.lookingSpeed = Random.Range(0.2f, 0.45f);
            
            looking.DoAction(looking.ThinkingState);
            
            looking.StartBlinkingLights();
        }

        public override void UpdateState(LookingStateManager looking)
        {
            if (!_switchAction) return;
            _switchAction = false;
            looking.DoAction(looking.ThinkingState);
        }

        public override void DoAction(LookingStateManager looking)
        {
            var action = ChooseActions();
            
            switch (action)
            {
                case 1:
                    looking.dartingSpeedUpperEnd = 0.5f;
                    looking.dartingSpeedLowerEnd = 0.1f;
                    
                    looking.lookingSpeed = Random.Range(0.2f, 0.3f);
                    looking.ChoosePoint(Random.Range(0.1f, -0.1f), Random.Range(0.2f, -0.2f));
                    looking.StartSpecificMouth("Mouth_AH", Random.Range(2f, 2.5f), Random.Range(0.1f, 0.2f));
                    
                    looking.StartCoroutine(WaitForAction(Random.Range(4f, 5f)));
                    break;
                case 2:
                    looking.dartingSpeedUpperEnd = 1f;
                    looking.dartingSpeedLowerEnd = 0.3f;
                    
                    looking.lookingSpeed = Random.Range(0.2f, 0.3f);
                    looking.ChoosePoint(ChooseX(), Random.Range(-0.8f, -1.8f));
                    looking.StartSpecificEmotion(6, Random.Range(2f, 2.5f), Random.Range(0.3f, 0.8f));
                    
                    looking.StartSpecificBody(29, ChooseX(), Random.Range(1f, 2f));
                    
                    looking.StartCoroutine(WaitForAction(Random.Range(4f, 5f)));
                    break;
                case 3:
                    looking.dartingSpeedUpperEnd = 1f;
                    looking.dartingSpeedLowerEnd = 0.3f;
                    
                    looking.lookingSpeed = Random.Range(0.2f, 0.3f);
                    looking.ChoosePoint(ChooseX(), Random.Range(0.8f, 2f));
                    looking.StartSpecificEmotion(6, Random.Range(2f, 2.5f), Random.Range(0.3f, 0.8f));
                    //looking.StartSpecificMouth("Mouth_M", Random.Range(2f, 3.5f), Random.Range(0.1f, 0.2f));
                    
                    looking.StartSpecificBody(29, ChooseX() / 3, Random.Range(2f, 4f));
                    
                    looking.StartCoroutine(WaitForAction(Random.Range(4f, 5f)));
                    break;
            }
        }

        private static int ChooseActions()
        {
            return Random.Range(1, 4);
        }

        private static float ChooseX()
        {
            var leftOrRight = Random.Range(0, 2);
            return leftOrRight == 0 ? Random.Range(-1.1f, -0.5f) : Random.Range(1.1f, 0.5f);
        }

        private static int ChooseOutOfTwo()
        {
            return Random.Range(1, 3);
        }

        private IEnumerator WaitForAction(float time)
        {
            yield return new WaitForSeconds(time);
            _switchAction = true;
        }
        
    }
}
