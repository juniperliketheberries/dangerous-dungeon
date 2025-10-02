using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;


[RequireComponent(typeof(UIDocument))]
public class HealthBarOverlay : MonoBehaviour
{
    private const string BarElementName = "BarCenter";

    private readonly List<VisualElement> visualElementCache = new();

    private UIDocument document;

    [Header("Configuration")]
    public float BarWidth = 120f;

    public VisualTreeAsset HealthBarResource;


    private IObjectPool<VisualElement> m_ElementPool = null;

    public IObjectPool<VisualElement> ElementPool
    {
        get
        {
            m_ElementPool ??= new ObjectPool<VisualElement>(Create, Get, Release, Destroy,
                collectionCheck: true, defaultCapacity: 10, maxSize: 24);

            return m_ElementPool;
        }
    }

    public VisualElement Create()
    {
        VisualElement instance = HealthBarResource.Instantiate();

        document.rootVisualElement.Add(instance);

        instance.style.position = Position.Absolute;
        instance.style.display = DisplayStyle.None;

        return instance;
    }

    public void Get(VisualElement element)
    {
        element.style.display = DisplayStyle.Flex;

    }

    public void Release(VisualElement element)
    {
        element.style.display = DisplayStyle.None;
    }

    public void Destroy(VisualElement element)
    {
        document.rootVisualElement.Remove(element);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        document = GetComponent<UIDocument>();
    }


    // Update is called once per frame
    void Update()
    {
        // Release each element and then clear the cache.
        // A 'health bar' is not bound to an entity, so it can be used for
        // different entities during each frame. This is not great as it
        // prevents health bar animations from working properly.
        visualElementCache.ForEach(element => ElementPool.Release(element));
        visualElementCache.Clear();

        // Very expensive, don't do this
        foreach (Vulnerable vuln in FindObjectsByType<Vulnerable>(FindObjectsSortMode.None))
        {
            ApplyHealthBar(vuln);
        }
    }

    void ApplyHealthBar(Vulnerable vuln)
    {
        if (vuln.IsDamaged ||
            vuln.InitialPoints == 0 ||
            vuln.InitialPoints == vuln.CurrentPoints ||
            Time.time - vuln.LastHitTime >= 5)
        { return; }


        VisualElement element = ElementPool.Get();

        visualElementCache.Add(element);

        VisualElement bar = element.Q<VisualElement>(BarElementName);

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(vuln.transform.position + Vector3.up);

        float width = document.rootVisualElement.layout.width;
        float height = document.rootVisualElement.layout.height;

        element.style.left = width * screenPoint.x - (BarWidth / 2);
        element.style.bottom = height * screenPoint.y;

        bar.style.width = Length.Percent(vuln.CurrentPoints / vuln.InitialPoints * 100f);
    }
}
