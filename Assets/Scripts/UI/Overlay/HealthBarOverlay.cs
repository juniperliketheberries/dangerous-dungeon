using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;


[RequireComponent(typeof(UIDocument))]
public class HealthBarOverlay : MonoBehaviour
{
    [Header("Configuration")]
    public float BarWidth = 120f;

    public VisualTreeAsset HealthBarResource;

    // Cache fiels
    private UIDocument document;
    private VisualElement healthBarInstance;
    //TODO use pool for multiple bars


    private Vulnerable[] elements;

    private GameObject player;


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

        healthBarInstance = HealthBarResource.Instantiate();

        document.rootVisualElement.Add(healthBarInstance);

        healthBarInstance.style.position = Position.Absolute;

        player = GameObject.FindWithTag("Player");
    }


    private List<VisualElement> used = new();
    private List<IVulnerable> list = new();


    // Update is called once per frame
    void Update()
    {
        if (healthBarInstance == null) { return; }

        if (player == null) { return; }

        foreach (VisualElement element in used)
        {
            ElementPool.Release(element);
        }

        used.Clear();

        // Very expensive, don't do this

        // Also this is bad polymorphism since IVulnerable does not have information about object position. But it could
        foreach (Vulnerable vuln in FindObjectsByType<Vulnerable>(FindObjectsSortMode.None))
        {
            ApplyHealthBar(vuln, vuln.transform.position);
        }

        foreach (Enemy vuln in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            ApplyHealthBar(vuln, vuln.transform.position);
        }
    }

    void ApplyHealthBar(IVulnerable vuln, Vector3 worldPosition)
    {
        if (vuln.IsDamaged) { return; }

        if (vuln.InitialHitPoints == 0
            || vuln.InitialHitPoints == vuln.HitPoints) { return; }

        if (Time.time - vuln.LastHitTime >= 5) { return; }

        VisualElement element = ElementPool.Get();

        used.Add(element);

        VisualElement bar = element.Q<VisualElement>("BarCenter");

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(worldPosition + Vector3.up);

        float width = document.rootVisualElement.layout.width;
        float height = document.rootVisualElement.layout.height;

        element.style.left = width * screenPoint.x - (BarWidth / 2);
        element.style.bottom = height * screenPoint.y;

        bar.style.width = Length.Percent(vuln.HitPoints / vuln.InitialHitPoints * 100f);
    }
}
