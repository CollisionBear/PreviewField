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
        public GameObject PreviewFieldGameObjectPrefab;     // Previews all GameObjects
        [PreviewField]
        public TestBlocker PreviewFieldBlockerPrefab;       // Previews only GameObjects with a TestBlocker component

        [Space]
        [PreviewField]
        public Texture Icon;                                 // Previews a Sprite type asset
    }
}