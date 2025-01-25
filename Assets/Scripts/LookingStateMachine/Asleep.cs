using UnityEngine;
using UnityEngine.Windows;

namespace LookingStateMachine
{
    public class Asleep : LookingBaseState
    {
        public bool StillAsleep;
        public bool FranticLookAround = false;

        private bool _doneThat;
        
        
        public override void EnterState(LookingStateManager looking)
        {
            StillAsleep = true;
            looking.ChangeShockIncrease(-20f);
            looking.live2DModel.Parameters[0].Value = 1f;
            looking.live2DModel.Parameters[28].Value = -1f;
        }

        public override void UpdateState(LookingStateManager looking)
        {
            if (!StillAsleep)
            {
                if (!_doneThat)
                {
                    _doneThat = true;
                    looking.DoAction(looking.AsleepState);
                }

                if (!FranticLookAround) return;
                
                if (!looking.waitingDone) return;
                looking.waitingDone = false;
                
                looking.Wait(Random.Range(0.7f, 1.5f));
                
                looking.ChoosePoint(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
            }
            else
            {
                if (!looking.waitingDone) return;
                looking.waitingDone = false;
                looking.Wait(Random.Range(2f, 10f));
                looking.DoAction(looking.AsleepState);
            }
        }

        public override void DoAction(LookingStateManager looking)
        {
            if (StillAsleep)
            {
                var randomNumber = Random.Range(0, 5);
                switch (randomNumber)
                {
                    case 0:
                        looking.StartSpecificEmotion(4, Random.Range(0.8f, 1.5f), Random.Range(0.1f, 0.4f));
                        break;
                    case 1:
                        looking.StartSpecificEmotion(1, Random.Range(3f, 5f), Random.Range(0.1f, 0.4f));
                        looking.StartSpecificEmotion(2, Random.Range(3f, 5f), Random.Range(0.1f, 0.4f));
                        break;
                    case 2:
                        looking.StartSpecificEmotion(6, Random.Range(3f, 5f), Random.Range(0.1f, 0.4f));
                        break;
                    case 3:
                        looking.StartSpecificEmotion(17, Random.Range(3f, 5f), Random.Range(0.1f, 0.4f));
                        break;
                    default:
                        looking.StartSpecificMouth("Mouth_M", Random.Range(2f, 4f), Random.Range(0.3f, 0.8f));
                        break;
                }
            }
            else
            {
                looking.lookingSpeed = Random.Range(0.2f, 0.35f);
                looking.ChoosePoint(0, Random.Range(1f, 1.3f));
                looking.StartSpecificMouth("Mouth_A_AY_R", Random.Range(2f, 4f), Random.Range(0.4f, 0.8f));
                looking.StartSpecificEmotion(1, Random.Range(1f, 2f), Random.Range(0.5f, 1f));
                looking.StartSpecificEmotion(2, Random.Range(1f, 2f), Random.Range(0.5f, 1f));
                looking.StartSpecificEmotion(4, Random.Range(1f, 2f), Random.Range(0.2f, 0.4f));
                looking.StartSpecificEmotion(5, Random.Range(1f, 2f), Random.Range(0.5f, 1f));
                looking.StartBootUpSequence();
            }
        }
    }
}
