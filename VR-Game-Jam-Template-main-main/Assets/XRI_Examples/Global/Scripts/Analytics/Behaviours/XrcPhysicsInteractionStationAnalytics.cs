

namespace UnityEngine.XR.Content.Interaction.Analytics
{
    /// <summary>
    /// Class that connects the Physics Interaction station scene objects with their respective analytics events.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    class XrcPhysicsInteractionStationAnalytics : MonoBehaviour
    {
        const float k_FrequencyToSendPushFlopDoor = 4f;

        static readonly PushFlipDoor k_PushFlipDoorParameter = new PushFlipDoor();

        [Header("Physics Simple Controls Substation")]
        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable[] m_SpringInteractables;

        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable[] m_HingeInteractables;

        [Header("Cabinet Example Substation")]
        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable m_Cabinet1Interactable;

        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable m_Cabinet2Interactable;

        [Header("Doors Example Substation")]
        [SerializeField]
        Rigidbody m_FlipDoorRigidbody;

        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable m_DoorKeyInteractable;

        [SerializeField]
        Door m_DoorLocked;

        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable m_DoorHandleInteractable;

        float m_TimeToSendPushFlopDoor;

        void Awake()
        {
            XrcAnalyticsUtils.Register(m_SpringInteractables, new GrabSpringJoint());
            XrcAnalyticsUtils.Register(m_HingeInteractables, new GrabHingeJoint());

            XrcAnalyticsUtils.Register(m_Cabinet1Interactable, new GrabCabinet1());
            XrcAnalyticsUtils.Register(m_Cabinet2Interactable, new GrabCabinet2());

            if (m_FlipDoorRigidbody != null)
            {
                var onCollision = m_FlipDoorRigidbody.gameObject.AddComponent<OnCollision>();
                onCollision.onEnter.AddListener(OnFlipDoorCollision);
            }
            XrcAnalyticsUtils.Register(m_DoorKeyInteractable, new GrabDoorKey());
            XrcAnalyticsUtils.Register(m_DoorLocked, new DoorLocked(), new DoorUnlocked());
            XrcAnalyticsUtils.Register(m_DoorHandleInteractable, new GrabDoorHandle());
        }

        void OnFlipDoorCollision(Collision collision)
        {
            if (Time.unscaledTime < m_TimeToSendPushFlopDoor)
                return;

            m_TimeToSendPushFlopDoor = Time.unscaledTime + k_FrequencyToSendPushFlopDoor;
            XrcAnalytics.interactionEvent.Send(k_PushFlipDoorParameter);
        }
    }
}
