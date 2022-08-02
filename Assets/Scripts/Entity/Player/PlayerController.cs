using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Hauler.Audio;
using Hauler.World;
using Hauler.Entity.Interactable;

namespace Hauler.Entity.Player {
    [RequireComponent(typeof(Rigidbody2D), typeof(BaseAudioController))]
    public class PlayerController : MonoBehaviour {
        public float speed = 5f, sprintSpeed = 9f, acceleration = 20f, rotationSpeed = 30f;
        [Range(0f, 1f)] public float crawlSpeedMultiplier = .6f;
        [Range(0f, 1f)] public float baseStaminaRegen = .2f, sprintStaminaCost = .3f;

        [Space]
        public FlashlightController flashlight;
        [Range(0f, 1f)] public float baseFlashlightPowerUsage = .1f, baseFlashlightRecharge = .05f, flashlightFlickerThreshold = .2f;

        [Space]
        public float maxInteractDistance = 1.5f;
        public LayerMask interactableMask, obstacleMask;

        [Space]
        public float footstepInterval = .7f;
        [Range(0f, 1f)] public float footstepSprintModifier = .6f;

        [Space]
        public Slider staminaSlider;
        public Slider flashlightSlider;

        public float Stamina { get; private set; }
        public float FlashlightPower { get; private set; }
        public bool FlashlightActive { get; private set; }
        public bool FlashlightShining { get => flashlight.FlickerStrength < .99f; }
        public bool IsMoving { get => move.sqrMagnitude > .1f && Velocity.sqrMagnitude > .1f; }
        public bool IsSprinting { get => sprinting && Stamina > .01f; }
        public bool IsInVent { get => GameController.Instance.PlayerRoom?.IsVent ?? false; }
        public Vector2 Velocity { get => (lastPosition - rb.position) / Time.fixedDeltaTime; }

        Rigidbody2D rb;
        BaseAudioController footstepAudioController;
        Animator flashlightSliderAnim;

        Vector2 lastPosition;
        Vector2 move;
        Vector2 mousePos;
        bool sprinting;
        bool interact;

        float footstepTime, lastFootstepTime;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
            footstepAudioController = GetComponent<BaseAudioController>();
            if(!flashlightSlider.TryGetComponent(out flashlightSliderAnim))
                Debug.LogWarning($"No slider animator found on {flashlightSlider.gameObject.name}");

            Stamina = FlashlightPower = 1f;
            FlashlightActive = flashlight.IsActive;
        }

        private void Update() {
            // Handle interactions
            mousePos = UnityEngine.Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            if(interact) {
                Collider2D collided = Physics2D.OverlapCircle(mousePos, .1f, interactableMask);
                if(collided != null) {
                    BaseInteractable interacted = collided.GetComponent<BaseInteractable>();
                    Vector2 delta = collided.transform.position - transform.position;
                    if(interacted != null && delta.sqrMagnitude <= maxInteractDistance * maxInteractDistance && !Physics2D.Raycast(transform.position, delta, delta.magnitude - .1f, obstacleMask)) {
                        interacted.Act();
                    }
                }
            }
            interact = false;

            // Handle flashlight
            if(FlashlightActive) {
                FlashlightPower -= baseFlashlightPowerUsage * Time.deltaTime;
            } else {
                FlashlightPower += baseFlashlightRecharge * Time.deltaTime;
            }
            FlashlightPower = Mathf.Clamp01(FlashlightPower);
            flashlight.FlickerStrength = Mathf.Clamp01(1f - FlashlightPower * (1f / flashlightFlickerThreshold));

            // Trigger footstep sound
            if(IsMoving) {
                footstepTime += Time.deltaTime;
                if(footstepTime >= lastFootstepTime + footstepInterval * (IsSprinting ? footstepSprintModifier : 1f)) {
                    footstepAudioController.PlaySound();
                    lastFootstepTime = footstepTime;
                }
            }

            // Update UI
            staminaSlider.value = Stamina;
            flashlightSlider.value = FlashlightPower;
            if(flashlightSliderAnim != null)
                flashlightSliderAnim.SetFloat("Power", FlashlightPower);
        }

        private void FixedUpdate() {
            // Handle movement
            float moveSpeed = IsSprinting ? sprintSpeed : speed;
            if(IsInVent) moveSpeed *= crawlSpeedMultiplier;
            rb.velocity = Vector2.Lerp(rb.velocity, move * moveSpeed, acceleration * Time.fixedDeltaTime);
            
            // Handle mouse look
            if(Mouse.current != null) {
                Vector2 targetLook = mousePos - new Vector2(transform.position.x, transform.position.y);
                float targetRot = Mathf.Rad2Deg * Mathf.Atan2(targetLook.y, targetLook.x) - 90f;
                rb.rotation = Mathf.LerpAngle(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            }

            // Deduct stamina while sprinting (or attempting to sprint while moving), regen if standing still
            if(IsMoving && sprinting) {
                Stamina -= sprintStaminaCost * Time.fixedDeltaTime;
            } else if(!IsMoving) {
                Stamina += baseStaminaRegen * Time.fixedDeltaTime;
            }
            Stamina = Mathf.Clamp01(Stamina);

            // Keep track of last position
            lastPosition = rb.position;
        }

        public void Move(InputAction.CallbackContext ctx) {
            move = ctx.ReadValue<Vector2>();
        }

        public void Sprint(InputAction.CallbackContext ctx) {
            sprinting = ctx.performed;
        }

        public void Flashlight(InputAction.CallbackContext ctx) {
            if(ctx.started)
                FlashlightActive = flashlight.ToggleFlashlight();
        }

        public void Interact(InputAction.CallbackContext ctx) {
            if(ctx.started)
                interact = true;
        }
    }
}
