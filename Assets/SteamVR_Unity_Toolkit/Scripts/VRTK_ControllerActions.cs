namespace VRTK
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    public class VRTK_ControllerActions : MonoBehaviour
    {
        private bool controllerVisible = true;
        private ushort hapticPulseStrength;

        private uint controllerIndex;
        private SteamVR_TrackedObject trackedController;
        private SteamVR_Controller.Device device;
        private ushort maxHapticVibration = 3999;

        private Dictionary<GameObject, Material> storedMaterials;

        public bool IsControllerVisible()
        {
            return controllerVisible;
        }

        public void ToggleControllerModel(bool on, GameObject grabbedChildObject)
        {
            foreach (MeshRenderer renderer in this.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject != grabbedChildObject && (grabbedChildObject == null || !renderer.transform.IsChildOf(grabbedChildObject.transform)))
                {
                    renderer.enabled = on;
                }
            }

            foreach (SkinnedMeshRenderer renderer in this.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer.gameObject != grabbedChildObject && (grabbedChildObject == null || !renderer.transform.IsChildOf(grabbedChildObject.transform)))
                {
                    renderer.enabled = on;
                }
            }
            controllerVisible = on;
        }

        public void SetControllerOpacity(float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0f, 1f);
            foreach (var renderer in this.gameObject.GetComponentsInChildren<Renderer>())
            {
                if (alpha < 1f)
                {
                    renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    renderer.material.SetInt("_ZWrite", 0);
                    renderer.material.DisableKeyword("_ALPHATEST_ON");
                    renderer.material.DisableKeyword("_ALPHABLEND_ON");
                    renderer.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    renderer.material.renderQueue = 3000;
                }
                else
                {
                    renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    renderer.material.SetInt("_ZWrite", 1);
                    renderer.material.DisableKeyword("_ALPHATEST_ON");
                    renderer.material.DisableKeyword("_ALPHABLEND_ON");
                    renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    renderer.material.renderQueue = -1;
                }

                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, alpha);
            }
        }

        public void HighlightControllerElement(GameObject element, Color highlight)
        {
            var renderer = element.GetComponent<Renderer>();
            if (renderer && renderer.material)
            {
                storedMaterials.Add(element, new Material(renderer.material));
                renderer.material.color = highlight;
                renderer.material.shader = Shader.Find("Unlit/Color");
            }
        }

        public void UnhighlightControllerElement(GameObject element)
        {
            var renderer = element.GetComponent<Renderer>();
            if (renderer && renderer.material)
            {
                renderer.material = new Material(storedMaterials[element]);
                storedMaterials.Remove(element);
            }
        }

        public void ToggleHighlightControllerElement(bool state, GameObject element, Color highlight)
        {
            if (element)
            {
                if (state)
                {
                    HighlightControllerElement(element.gameObject, highlight);
                }
                else
                {
                    UnhighlightControllerElement(element.gameObject);
                }
            }
        }

        public void ToggleHighlightTrigger(bool state, Color highlight)
        {
            ToggleHighlightAlias(state, "Model/trigger", highlight);
        }

        public void ToggleHighlightGrip(bool state, Color highlight)
        {
            ToggleHighlightAlias(state, "Model/lgrip", highlight);
            ToggleHighlightAlias(state, "Model/rgrip", highlight);
        }

        public void ToggleHighlightTouchpad(bool state, Color highlight)
        {
            ToggleHighlightAlias(state, "Model/trackpad", highlight);
        }

        public void ToggleHighlightApplicationMenu(bool state, Color highlight)
        {
            ToggleHighlightAlias(state, "Model/button", highlight);
        }

        public void TriggerHapticPulse(ushort strength)
        {
            hapticPulseStrength = (strength <= maxHapticVibration ? strength : maxHapticVibration);
            device.TriggerHapticPulse(hapticPulseStrength);
        }

        public void TriggerHapticPulse(ushort strength, float duration, float pulseInterval)
        {
            hapticPulseStrength = (strength <= maxHapticVibration ? strength : maxHapticVibration);
            StartCoroutine(Pulse(duration, hapticPulseStrength, pulseInterval));
        }

        private void Awake()
        {
            trackedController = GetComponent<SteamVR_TrackedObject>();
            this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            storedMaterials = new Dictionary<GameObject, Material>();
        }

        private void Update()
        {
            controllerIndex = (uint)trackedController.index;
            device = SteamVR_Controller.Input((int)controllerIndex);
        }

        private IEnumerator Pulse(float duration, int hapticPulseStrength, float pulseInterval)
        {
            if (pulseInterval <= 0)
            {
                yield break;
            }

            while (duration > 0)
            {
                device.TriggerHapticPulse((ushort)hapticPulseStrength);
                yield return new WaitForSeconds(pulseInterval);
                duration -= pulseInterval;
            }
        }

        private  void ToggleHighlightAlias(bool state, string transformPath, Color highlight)
        {
            var element = transform.Find(transformPath);
            if (element)
            {
                ToggleHighlightControllerElement(state, element.gameObject, highlight);
            }
        }
    }
}