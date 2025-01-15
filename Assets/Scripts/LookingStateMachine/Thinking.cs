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
            Debug.Log("thinking");
        }

        public override void UpdateState(LookingStateManager looking)
        {
            if (!looking.thinking)
            {
                looking.thinking = true;
            }
            
            if (_switchAction)
            {
                _switchAction = false;
                looking.DoAction(looking.ThinkingState);
            }
            
            if (!_doneThinking) return;
            looking.thinking = false;
            looking.SwitchState(looking.AttentionState);
        }

        public override void DoAction(LookingStateManager looking)
        {
            looking.thinking = true;
            
            var action = ChooseActions();
            
            switch (action)
            {
                case 1:
                    looking.dartingSpeedUpperEnd = 0.5f;
                    looking.dartingSpeedLowerEnd = 0.1f;
                    
                    looking.lookingSpeed = Random.Range(0.2f, 0.3f);
                    looking.ChoosePoint(Random.Range(0.1f, -0.1f), Random.Range(0.2f, -0.2f));
                    looking.StartSpecificMouth("Mouth_AH", Random.Range(2f, 2.5f), Random.Range(0.2f, 0.3f));
                    
                    looking.StartCoroutine(WaitForAction(Random.Range(4f, 5f)));
                    break;
                case 2:
                    looking.dartingSpeedUpperEnd = 1f;
                    looking.dartingSpeedLowerEnd = 0.3f;
                    
                    looking.lookingSpeed = Random.Range(0.2f, 0.3f);
                    looking.ChoosePoint(ChooseX(), Random.Range(-0.8f, -1f));
                    looking.StartSpecificEmotion(6, Random.Range(2f, 2.5f), Random.Range(0.3f, 0.8f));
                    
                    looking.StartCoroutine(WaitForAction(Random.Range(4f, 5f)));
                    break;
                case 3:
                    looking.dartingSpeedUpperEnd = 1f;
                    looking.dartingSpeedLowerEnd = 0.3f;
                    
                    looking.lookingSpeed = Random.Range(0.2f, 0.3f);
                    looking.ChoosePoint(ChooseX(), Random.Range(0.8f, 1f));
                    looking.StartSpecificEmotion(6, Random.Range(2f, 2.5f), Random.Range(0.3f, 0.8f));
                    looking.StartSpecificMouth("Mouth_AH", Random.Range(2f, 3.5f), Random.Range(0.3f, 0.6f));
                    
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
