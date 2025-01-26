using System;
using System.Collections;
using Live2D.Cubism.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LookingStateMachine
{
    public class BlinkingStuff : MonoBehaviour
    {
        [SerializeField] private CubismRenderer internetConnection;
        [SerializeField] private CubismRenderer eyeLeft;
        [SerializeField] private CubismRenderer eyeRight;

        [SerializeField] private CubismRenderer[] thingsToFadeOut; 
    
        [SerializeField] private LookingStateManager lookingStateManager;

        private Coroutine _eyeColourRoutine;
        private bool _eyeFade;
        
        private Color _originalEyeColor;
        //add other lights

        private void Start()
        {
            _originalEyeColor = eyeLeft.Color;
            internetConnection.Color = new Color(1, 1, 1, 0);
        }

        public void OriginalColour()
        {
            internetConnection.Color = new Color(1, 1, 1, 1);
            eyeLeft.Color = new Color(1, 1, 1, 1);
            eyeRight.Color = new Color(1, 1, 1, 1);

            foreach (var i in thingsToFadeOut)
            {
                i.Color = new Color(1, 1, 1, 1);
            }
        }

        public void StartInternetBlink()
        {
            StartCoroutine(Blinking());
        }
    
        public void StartEyesBlink()
        {
            StartCoroutine(FadeInOut(eyeLeft, eyeRight));
        }

        private IEnumerator Blinking()
        {
            var turnedOff = false;
            while (lookingStateManager.thinking)
            {
                internetConnection.Color = new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, Random.Range(0.8f, 1.0f));
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                internetConnection.Color = new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, Random.Range(0.1f, 0.35f));
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }
            
            var elapsedTime = 0f;
            var randomTime = Random.Range(2f, 4f);
            while (elapsedTime <= randomTime)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                var alpha = Mathf.Lerp(0.1f, 1f, normalizedTime);
                
                if (Random.Range(0, 10) == 0)
                {
                    turnedOff = !turnedOff;
                }

                internetConnection.Color = turnedOff ? new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, Random.Range(0.8f, 1.0f)) : 
                    new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, alpha);
                
                yield return null;
            }
            
            internetConnection.Color = new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, 1);
        }

        private IEnumerator ShortBlink(float duration)
        {
            var turnedOff = false;
            
            var elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                
                if (Random.Range(0, 10) == 0)
                {
                    turnedOff = !turnedOff;
                }

                internetConnection.Color = turnedOff ? new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, Random.Range(0.8f, 1.0f)) : 
                    new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, Random.Range(0, 0.3f));

                yield return null;
            }

            internetConnection.Color = new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, 1);
        }

        private IEnumerator ShortFade(float duration, Color targetColour)
        {
            yield return new WaitForSeconds(duration*2);
           
            var elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                
                var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                
                var valueR = Mathf.Lerp(internetConnection.Color.r, targetColour.r, normalizedTime);
                var valueG = Mathf.Lerp(internetConnection.Color.g, targetColour.g, normalizedTime);
                var valueB = Mathf.Lerp(internetConnection.Color.b, targetColour.b, normalizedTime);
                
                internetConnection.Color = new Color(valueR, valueG, valueB);

                yield return null;
            }

            internetConnection.Color = new Color(targetColour.r, targetColour.g, targetColour.b, 1);
        }

        public void ColourChange(Color colour, float duration, bool fadeOut)
        {
            internetConnection.Color = colour;
            
            StartCoroutine(ShortBlink(duration));

            if (fadeOut)
            {
                StartCoroutine(ShortFade(duration, _originalEyeColor));
            }
        }

        public void StartEyeFade(float duration)
        {
            StartCoroutine(EyeFade(duration));
        }
        
        private IEnumerator EyeFade(float duration)
        {
            var elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                
                var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                
                var alpha = Mathf.Lerp(1, 0, normalizedTime);
                
                eyeLeft.Color = new Color(eyeLeft.Color.r, eyeLeft.Color.g, eyeLeft.Color.b, alpha);
                eyeRight.Color = new Color(eyeRight.Color.r, eyeRight.Color.g, eyeRight.Color.b, alpha);
                internetConnection.Color = new Color(internetConnection.Color.r, internetConnection.Color.g, internetConnection.Color.b, alpha);

                foreach (var i in thingsToFadeOut)
                {
                    i.Color = new Color(i.Color.r, i.Color.g, i.Color.b, alpha);
                }

                yield return null;
            }
        }

        private IEnumerator FadeInOut(CubismRenderer leftEye, CubismRenderer rightEye)
        {
            var currentColour = leftEye.Color;
            
            while (_eyeFade)
            {
                var elapsedTime = 0f;
                var randomTime = Random.Range(0.1f, 0.4f);
                var randomAlpha = Random.Range(0.6f, 0.8f);
        
                while (elapsedTime <= randomTime)
                {
                    elapsedTime += Time.deltaTime;
                    var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                    var preValue = EasingFunctions.InOutBack(normalizedTime);

                    var value = randomAlpha + (currentColour.a - randomAlpha) * preValue;
            
                    leftEye.Color = new Color(currentColour.r, currentColour.g, currentColour.b, value);
                    rightEye.Color = new Color(currentColour.r, currentColour.g, currentColour.b, value);

                    yield return null;
                }
        
                while (elapsedTime <= randomTime)
                {
                    elapsedTime += Time.deltaTime;
                    var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                    var preValue = EasingFunctions.InOutBack(normalizedTime);

                    var value = currentColour.a + (randomAlpha - currentColour.a) * preValue;
            
                    leftEye.Color = new Color(currentColour.r, currentColour.g, currentColour.b, value);
                    rightEye.Color = new Color(currentColour.r, currentColour.g, currentColour.b, value);

                    yield return null;
                }
                
            }
            leftEye.Color = currentColour;
            rightEye.Color = currentColour;
        }

        public void InternetConnectionTurnOn()
        {
            
        }


        public void EyeColourDecider(int actionUnit)
        {
            _eyeFade = true;
            switch (actionUnit)
            {
                case 4:
                    if (_eyeColourRoutine != null) StopCoroutine(_eyeColourRoutine);
                    StartEyesBlink();
                    _eyeColourRoutine = StartCoroutine(BriefEyeColourChange(1, 0.5f, 0.5f));
                    break;
                case 12:
                    if (_eyeColourRoutine != null) StopCoroutine(_eyeColourRoutine);
                    StartEyesBlink();
                    _eyeColourRoutine = StartCoroutine(BriefEyeColourChange(0.5f, 1, 1));
                    break;
                case 1:
                    if (_eyeColourRoutine != null) StopCoroutine(_eyeColourRoutine);
                    StartEyesBlink();
                    _eyeColourRoutine = StartCoroutine(BriefEyeColourChange(0.5f, 0.5f, 1));
                    break;
                case 10:
                    if (_eyeColourRoutine != null) StopCoroutine(_eyeColourRoutine);
                    StartEyesBlink();
                    _eyeColourRoutine = StartCoroutine(BriefEyeColourChange(0.5f, 1, 0.5f));
                    break;
            }
        }
        
        private IEnumerator BriefEyeColourChange(float targetRed, float targetGreen, float targetBlue)
        {
            var currentColour = eyeLeft.Color;
            
            var elapsedTime = 0f;
            var randomTime = Random.Range(1f, 2f);
        
            while (elapsedTime <= randomTime)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                var preValue = EasingFunctions.InOutBack(normalizedTime);

                var valueRed = targetRed + (currentColour.r - targetRed) * preValue;
                var valueGreen = targetGreen + (currentColour.g - targetGreen) * preValue;
                var valueBlue = targetBlue + (currentColour.b - targetBlue) * preValue;
                    
                var newEyeColour = new Color(valueRed, valueGreen, valueBlue);

                eyeLeft.Color = newEyeColour;
                eyeRight.Color = newEyeColour;

                yield return null;
            }
        
            while (elapsedTime <= randomTime)
            {
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / randomTime);
                var preValue = EasingFunctions.InOutBack(normalizedTime);

                var valueRed = _originalEyeColor.r + (targetRed - _originalEyeColor.r) * preValue;
                var valueGreen = _originalEyeColor.g + (targetGreen - _originalEyeColor.g) * preValue;
                var valueBlue = _originalEyeColor.b + (targetBlue - _originalEyeColor.b) * preValue;
                    
                var newEyeColour = new Color(valueRed, valueGreen, valueBlue);

                eyeLeft.Color = newEyeColour;
                eyeRight.Color = newEyeColour;

                yield return null;
            }

            eyeLeft.Color = _originalEyeColor;
            eyeRight.Color = _originalEyeColor;

            _eyeFade = false;
        }
    }
}
