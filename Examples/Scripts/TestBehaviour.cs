using UnityEngine;

namespace CollisionBear.PreviewObjectPicker.Examples {
    public class TestBehaviour : MonoBehaviour
    {
        [Header("Test Behaviour to show how PreviewField can show Previews")]
        [Header("Unity Standard drawers")]
        public GameObject NormalGameObjectPrefab;
        public TestBlocker NormalBlockerPrefab;

        [Space]
        [Header("Preview Field drawers")]
        [PreviewField]
        public GameObject PreviewFieldGameObjectPrefab;
        [PreviewField]
        public GameObject PreviewFieldBlockerPrefab;
    }
}