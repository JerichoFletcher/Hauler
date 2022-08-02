using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Hauler.Audio;

namespace Hauler.Entity {
    [RequireComponent(typeof(AudioSource))]
    public class FlashlightController : MonoBehaviour {
        public float maxIntensityWhileFlickering = .5f;
        public GameObject flashlightObj;
        public Light2D[] lightComponents;

        bool isFlickering, wasAbleToFlicker;
        float currentStrength, nextFlickerTarget;

        BaseAudioController audioController;
        (Light2D, float)[] lights;

        public bool IsActive { get; private set; }
        public float FlickerStrength { get; set; }
        public bool CanFlicker { get => FlickerStrength > .01f; }

        private void Awake() {
            if(!TryGetComponent(out audioController))
                Debug.LogWarning($"No audio controller added to {gameObject.name}");
            IsActive = flashlightObj.activeInHierarchy;

            lights = new (Light2D, float)[lightComponents.Length];
            for(int i = 0; i < lights.Length; i++)
                lights[i] = (lightComponents[i], lightComponents[i].intensity);
        }

        private void Update() {
            if(!CanFlicker && wasAbleToFlicker) {
                currentStrength = 1f;
                nextFlickerTarget = 1f;
                isFlickering = false;
                SetIntensity(currentStrength);
            }else if(CanFlicker) {
                if(FlickerStrength > .99f) {
                    currentStrength = 0f;
                    nextFlickerTarget = 0f;
                    isFlickering = false;
                    SetIntensity(currentStrength);
                } else {
                    if(!isFlickering) {
                        float newFlickerTarget = Random.value;
                        if(newFlickerTarget < FlickerStrength) {
                            nextFlickerTarget = 1f - newFlickerTarget;
                            isFlickering = true;
                        }
                    } else {
                        currentStrength = Mathf.Lerp(currentStrength, nextFlickerTarget, 60f * Time.deltaTime);
                        SetIntensity(currentStrength);
                        if(Mathf.Abs(currentStrength - nextFlickerTarget) < .01f) {
                            if(nextFlickerTarget < 1f) {
                                nextFlickerTarget = 1f;
                            } else {
                                isFlickering = false;
                            }
                        }
                    }
                }
                //Debug.Log($"currentStrength: {currentStrength}, nextFlickerTarget: {nextFlickerTarget}");
            }
            wasAbleToFlicker = CanFlicker;
        }

        void SetIntensity(float relIntensity) {
            foreach((Light2D, float) light in lights)
                light.Item1.intensity = relIntensity * light.Item2 * Mathf.Lerp(1f, maxIntensityWhileFlickering, FlickerStrength);
        }

        public bool ToggleFlashlight() {
            IsActive = !IsActive;
            flashlightObj.SetActive(IsActive);
            audioController?.PlaySound();
            return IsActive;
        }

        public void SetFlashlight(bool active) {
            if(IsActive != active) {
                flashlightObj.SetActive(active);
                audioController?.PlaySound();
            }
            IsActive = active;
        }
    }
}
