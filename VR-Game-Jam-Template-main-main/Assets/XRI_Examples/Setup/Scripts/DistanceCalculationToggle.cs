using UnityEngine.UI;


namespace UnityEngine.XR.Content.Interaction
{
    public class DistanceCalculationToggle : MonoBehaviour
    {
        [SerializeField]
        Toggle m_Toggle;

        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable[] m_Interactables;

        void Start()
        {
            if (m_Toggle == null)
                return;

            OnToggleValueChanged(m_Toggle.isOn);
            m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void OnToggleValueChanged(bool value)
        {
            var distanceCalculationMode = value
                ? UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.DistanceCalculationMode.ColliderVolume
                : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.DistanceCalculationMode.ColliderPosition;

            foreach (var interactable in m_Interactables)
            {
                if (interactable != null)
                    interactable.distanceCalculationMode = distanceCalculationMode;
            }
        }
    }
}
