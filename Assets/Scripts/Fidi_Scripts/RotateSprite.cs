using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class RotateSprite : MonoBehaviour
    {
        [Header("Rotation Settings")] public float speed = 1f;
        public bool rotate = true;
        public bool rotateClockwise = true;

        [Header("Size Change Settings")] public bool changeSize = false;
        public float sizeModifier = 1f;
        public float originalSize = 1f;
        public float sizeReduction = 0.01f;

        [Header("Color Change Settings")] public bool changeColor = false;
        public Color trueColor;
        public Color falseColor;
        private SpriteRenderer spriteRenderer;

        [SerializeField] private AudioRec audioRec;


        private void Start()
        {
            originalSize = sizeModifier;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            audioRec.OnRecordingModeChange += ChangeColor;
        }
        
        private void OnDisable()
        {
            audioRec.OnRecordingModeChange -= ChangeColor;
        }

        public void ChangeSizeModifier(float modifier)
        {
            sizeModifier = modifier;
        }

        public void ChangeColor(bool color)
        {
            if (changeColor)
                spriteRenderer.color = !color ? trueColor : falseColor;
        }

        private void Update()
        {
            if (rotate)
            {
                transform.Rotate(rotateClockwise ? Vector3.forward : Vector3.back, speed * Time.deltaTime);
            }

            if (changeSize)
            {
                if (sizeModifier > originalSize)
                {
                    sizeModifier -= sizeReduction;
                }
                else if (sizeModifier < originalSize)
                {
                    sizeModifier += sizeReduction;
                }

                transform.localScale = new Vector3(sizeModifier, sizeModifier, 1);
            }
        }

        public void ChangeDirection()
        {
            rotateClockwise = !rotateClockwise;
        }
    }
}