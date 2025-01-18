using UnityEngine;

namespace LookingStateMachine
{
    public class Talking : LookingBaseState
    {
        private float _lowerEndX;
        private float _upperEndX;

        private float _lowerEndY;
        private float _upperEndY;

        private float _actionUnit1;
        private float _actionUnit2;
        private float _actionUnit4;
        private float _actionUnit10;
        private float _actionUnit12;
        
        public override void EnterState(LookingStateManager looking)
        {
            Debug.Log("talking");
            
            looking.dartingSpeedUpperEnd = 0.5f;
            looking.dartingSpeedLowerEnd = 1.3f;
            
            looking.lookingSpeed = Random.Range(0.2f, 0.45f);
            looking.ChoosePoint(0, 0);
        }

        public override void UpdateState(LookingStateManager looking)
        {
            if (looking.emotionManager.talkingLookChange)
            {
                _actionUnit1 = looking.emotionManager.currentActionUnits[1];
                _actionUnit2 = looking.emotionManager.currentActionUnits[2];
                _actionUnit4 = looking.emotionManager.currentActionUnits[4];
                _actionUnit10 = looking.emotionManager.currentActionUnits[10];
                _actionUnit12 = looking.emotionManager.currentActionUnits[12];
                
                looking.emotionManager.talkingLookChange = false;
                looking.lookingSpeed = Random.Range(0.2f, 0.45f);

                if (_actionUnit4 >= 0.2f || (_actionUnit1 >= 0.2f && _actionUnit2 >= 0.2f))
                {
                    SetBounds(0, 0, 0, 0);
                    looking.ChoosePoint(_lowerEndX, _lowerEndY);
                }
                else if (_actionUnit1 >= 0.5f)
                {
                    SetBounds(-1.2f, 1.2f, -1f, -0.7f);
                    looking.ChoosePoint(Random.Range(_lowerEndX, _upperEndX), Random.Range(_lowerEndY, _upperEndY));
                }
                else if (_actionUnit12 >= 0.2f || _actionUnit10 >= 0.4f)
                {
                    SetBounds(-0.2f, 0.2f, -0.3f, 0.3f);
                    looking.ChoosePoint(Random.Range(_lowerEndX, _upperEndX), Random.Range(_lowerEndY, _upperEndY));
                }
                else
                {
                    SetBounds(-0.4f, 0.4f, -0.4f, 0.4f);
                    looking.ChoosePoint(Random.Range(_lowerEndX, _upperEndX), Random.Range(_lowerEndY, _upperEndY));
                }

                void SetBounds(float lowerX, float upperX, float lowerY, float upperY)
                {
                    _lowerEndX = lowerX;
                    _upperEndX = upperX;
                    _lowerEndY = lowerY;
                    _upperEndY = upperY;
                }
            }
            
            if (!looking.waitingDone) return;
            looking.waitingDone = false;
            looking.Wait(Random.Range(0.8f, 15f));
            
            looking.ChoosePoint(Random.Range(_lowerEndX, _upperEndX), Random.Range(_lowerEndY, _upperEndY));
        }

        public override void DoAction(LookingStateManager looking)
        {
        
        }
    }
}
