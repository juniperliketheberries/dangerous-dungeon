using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


/// <summary>
///     UIElements controller for the top-level "menu" Document.
/// </summary>
/// <remarks>
///     Provides control to the nested <see cref="SettingsUIContent"/> controller
/// </remarks>
public class MenuUIController : MonoBehaviour
{
    /// <summary>
    ///     Inner content controller. Soon will become a tabbed layout
    /// </summary>
    private readonly SettingsUIContent settingsContent = new();

    /// <summary> The root visual element from the <see cref="Document"/> </summary>
    private VisualElement root;

    /// <summary> Visual element displaying the menu buttons </summary>
    private VisualElement menuActions;

    /// <summary> Embedded visual element displaying settings </summary>
    private VisualElement settingsTabbedUI;

    // Menu actions
    private Button continueButton;
    private Button settingsButton;
    private Button restartButton;
    private Button quitButton;


    [Header("Documents")]
    public UIDocument Document;

    [Header("Events")]
    public UnityEvent Closed;
    public UnityEvent Restart;
    public UnityEvent Quit;


    public virtual void OnEnable()
    {
        root = Document.rootVisualElement;

        // Reference buttons
        continueButton = root.Q<Button>("buttonContinue");
        settingsButton = root.Q<Button>("buttonSettings");
        restartButton = root.Q<Button>("buttonRestart");
        quitButton = root.Q<Button>("buttonQuit");

        menuActions = root.Q<VisualElement>("actions");
        settingsTabbedUI = root.Q<VisualElement>("settingsTabbedUI");

        continueButton.clicked += OnContinue;
        settingsButton.clicked += OnSettings;
        restartButton.clicked += OnRestart;
        quitButton.clicked += OnQuit;

        settingsContent.Enabled += HideActions;
        settingsContent.Disabled += ShowActions;
    }

    public virtual void OnDisable()
    {
        continueButton.clicked -= OnContinue;
        settingsButton.clicked -= OnSettings;
        restartButton.clicked -= OnRestart;
        quitButton.clicked -= OnQuit;

        settingsContent.Enabled -= HideActions;
        settingsContent.Disabled -= ShowActions;
    }

    private void OnContinue() => Closed?.Invoke();

    private void OnRestart() => Restart?.Invoke();

    private void OnQuit() => Quit?.Invoke();

    public void Open()
    {
        if (!Document) { return; }
        Document.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (!Document) { return; }
        Document.gameObject.SetActive(false);
    }

    public void OnSettings()
    {
        settingsTabbedUI.style.display = DisplayStyle.Flex;
        settingsContent.Enable(settingsTabbedUI);
    }

    private void ShowActions()
    {
        menuActions.style.display = DisplayStyle.Flex;
    }

    private void HideActions()
    {
        menuActions.style.display = DisplayStyle.None;
    }
}
