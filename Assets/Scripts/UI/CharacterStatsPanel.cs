using DangerousDungeon.Character;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DangerousDungeon.UI.Character
{

    [RequireComponent(typeof(UIDocument))]
    public class CharacterStatsPanel : MonoBehaviour
    {

        private UIDocument document;

        private VisualElement fieldList;

        public VisualTreeAsset KeyValuePairResource;

        public Player PlayerController;

        protected CharacterStats stats;

        protected Dictionary<Stat, VisualElement> keyValuePairs = new(); 

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            document = GetComponent<UIDocument>();
            fieldList = document.rootVisualElement.Q<VisualElement>("fields");

            fieldList.Clear();

            stats = PlayerController.Stats;

            keyValuePairs.Clear();
            foreach (Stat stat in stats.ActiveFields)
            {
                VisualElement element = GenerateStatField(stat, stats.Get(stat));
                keyValuePairs.Add(stat, element);
                fieldList.Add(element);
            }

            if (stats != null)
            {
                stats.ValueChanged += HandleStatChange;
            }
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.ValueChanged -= HandleStatChange;
            }
        }

        protected VisualElement GenerateStatField(Stat stat, int value)
        {
            VisualElement container = KeyValuePairResource.Instantiate();

            container.style.alignSelf = Align.Stretch;

            Label key = container.Q<Label>("labelKey");
            Label val = container.Q<Label>("labelValue");

            key.text = stat.ToString();
            val.text = value.ToString();

            return container;
        }

        protected void UpdateStatField(VisualElement container, int value)
        {
            Label val = container.Q<Label>("labelValue");

            val.text = value.ToString();
        }

        protected void HandleStatChange(Stat stat)
        {
            UpdateStatField(keyValuePairs[stat], stats.Get(stat));
        }
    }
}
