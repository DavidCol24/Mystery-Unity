using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using Ink.Runtime;

public class MysteryStoryController : MonoBehaviour
{
    [Header("Ink Story Assets")]
    public TextAsset inkJSON;
    private Story story;

    [Header("User Interface Elements")]
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI debugText;
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;

    [Header("Visual Environment")]
    public Image backgroundImage;
    public Sprite[] environmentSprites; // Order: Forest, Meadow, Cabin, Grotto
    public Image timeOfDayOverlay;

    [Header("Color Tints")]
    public Color morningTint = Color.white;
    public Color afternoonTint = new Color(1f, 0.9f, 0.7f, 1f);
    public Color eveningTint = new Color(0.8f, 0.6f, 0.4f, 1f);
    public Color nightTint = new Color(0.3f, 0.3f, 0.5f, 1f);

    [Header("Transition Settings")]
    public bool useInstantTransitions = true;
    public float transitionDuration = 0.5f;

    [Header("Debugging")]
    public bool enableDetailedLogging = true;
    public string defaultStartLocation = "forest";
    public string startingStoryPoint = "start";

    // Internal tracking variables
    private string currentLocation = "forest";
    private const int MAX_CONTINUE_ATTEMPTS = 10;
    private string lastProcessedText = string.Empty;

    void Start()
    {
        ValidateAndPrepareComponents();
        InitializeStory();
    }

    void ValidateAndPrepareComponents()
    {
        // Validate critical components
        if (environmentSprites == null || environmentSprites.Length == 0)
        {
            Debug.LogError("Environment sprites are not assigned!");
            return;
        }

        if (backgroundImage != null)
        {
            // Set default sprite if none assigned
            if (backgroundImage.sprite == null && environmentSprites.Length > 0)
            {
                backgroundImage.sprite = environmentSprites[0];
            }

            // Ensure full-screen coverage
            RectTransform bgRect = backgroundImage.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }
    }

    void InitializeStory()
    {
        if (inkJSON == null)
        {
            Debug.LogError("No Ink JSON story file assigned!");
            return;
        }

        try
        {
            story = new Story(inkJSON.text);
            story.ChoosePathString(startingStoryPoint);

            if (enableDetailedLogging)
                Debug.Log("Story initialized successfully");

            RefreshStoryView();
        }
        catch (Exception e)
        {
            Debug.LogError($"Story initialization failed: {e.Message}");
        }
    }

    void RefreshStoryView()
    {
        if (story == null) return;

        ClearUserInterface();

        string textFragment = GetNextStoryFragment();

        if (!string.IsNullOrWhiteSpace(textFragment))
        {
            ProcessStoryText(textFragment);
        }
        else if (!story.canContinue)
        {
            DisplayEndOfStory();
            return;
        }

        UpdateEnvironmentAndStats();
        DisplayAvailableChoices();
    }

    string GetNextStoryFragment()
    {
        string textFragment = string.Empty;
        int continueAttempts = 0;

        while (story.canContinue &&
               string.IsNullOrWhiteSpace(textFragment) &&
               continueAttempts < MAX_CONTINUE_ATTEMPTS)
        {
            textFragment = story.Continue();
            continueAttempts++;
        }

        return textFragment;
    }

    void ProcessStoryText(string textFragment)
    {
        if (textFragment == lastProcessedText)
        {
            Debug.LogWarning("Prevented duplicate text processing");
            return;
        }

        lastProcessedText = textFragment;
        StartCoroutine(AnimateTextAppearance(textFragment));
        DetermineAndUpdateLocation(textFragment);
    }

    void DetermineAndUpdateLocation(string text)
    {
        text = text.ToLower();
        string newLocation = currentLocation;

        if (text.Contains("cabin")) newLocation = "cabin";
        else if (text.Contains("pool") || text.Contains("meadow")) newLocation = "meadow";
        else if (text.Contains("forest")) newLocation = "forest";
        else if (text.Contains("grotto")) newLocation = "grotto";

        if (newLocation != currentLocation)
        {
            currentLocation = newLocation;
            UpdateBackgroundSprite(newLocation);
        }
    }

    void UpdateBackgroundSprite(string location)
    {
        if (backgroundImage == null || environmentSprites == null) return;

        int locationIndex = GetLocationIndex(location);
        if (locationIndex >= 0 && locationIndex < environmentSprites.Length)
        {
            Sprite newSprite = environmentSprites[locationIndex];
            if (newSprite != null)
            {
                backgroundImage.sprite = newSprite;
            }
        }
    }

    int GetLocationIndex(string location)
    {
        switch (location.ToLower())
        {
            case "forest": return 0;
            case "meadow": return 1;
            case "cabin": return 2;
            case "grotto": return 3;
            default: return 0;
        }
    }

    void ClearUserInterface()
    {
        if (storyText != null) storyText.text = string.Empty;

        if (choiceButtonContainer != null)
        {
            foreach (Transform child in choiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    IEnumerator AnimateTextAppearance(string textFragment)
    {
        if (storyText == null) yield break;

        storyText.alpha = 0;
        storyText.text = textFragment;
        storyText.fontSize = 36;
        storyText.color = Color.white;
        storyText.fontStyle = FontStyles.Bold;
        storyText.alignment = TextAlignmentOptions.Center;
        storyText.ForceMeshUpdate();

        float animationDuration = 0.5f;
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            storyText.alpha = Mathf.Lerp(0, 1, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        storyText.alpha = 1;
    }

    void DisplayAvailableChoices()
    {
        List<Choice> currentChoices = story.currentChoices;

        if (currentChoices.Count == 0)
        {
            CreateContinueOrRestartButton();
            return;
        }

        foreach (Choice choice in currentChoices)
        {
            CreateChoiceButton(choice);
        }
    }

    void CreateChoiceButton(Choice choice)
    {
        Button choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = choice.text;
            buttonText.fontSize = 24;
            buttonText.color = Color.black;
        }

        choiceButton.onClick.AddListener(() => ProcessChoice(choice));
    }

    void ProcessChoice(Choice choice)
    {
        int choiceIndex = story.currentChoices.IndexOf(choice);
        story.ChooseChoiceIndex(choiceIndex);
        RefreshStoryView();
    }

    void CreateContinueOrRestartButton()
    {
        Button actionButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();

        if (story.canContinue)
        {
            buttonText.text = "Continue...";
            actionButton.onClick.AddListener(RefreshStoryView);
        }
        else
        {
            buttonText.text = "Restart Story";
            actionButton.onClick.AddListener(RestartStory);
        }
    }

    void UpdateEnvironmentAndStats()
    {
        UpdateTimeOfDay();
        UpdateGameStats();
    }

    void UpdateTimeOfDay()
    {
        if (story == null || timeOfDayOverlay == null) return;

        try
        {
            string timeOfDay = GetStoryVariable<string>("time_of_day", "morning");
            Color targetColor = GetTimeOfDayColor(timeOfDay);
            StartCoroutine(FadeTimeOfDay(targetColor));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error updating time of day: {e.Message}");
        }
    }

    Color GetTimeOfDayColor(string timeOfDay)
    {
        switch (timeOfDay.ToLower())
        {
            case "morning": return morningTint;
            case "afternoon": return afternoonTint;
            case "evening": return eveningTint;
            case "night": return nightTint;
            default: return morningTint;
        }
    }

    IEnumerator FadeTimeOfDay(Color targetColor)
    {
        Color startColor = timeOfDayOverlay.color;
        float duration = 0.75f;
        float time = 0;

        while (time < duration)
        {
            timeOfDayOverlay.color = Color.Lerp(startColor, targetColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        timeOfDayOverlay.color = targetColor;
    }

    void UpdateGameStats()
    {
        if (story == null || statsText == null) return;

        try
        {
            int vitality = GetStoryVariable<int>("player_vitality", 100);
            bool hasMagicWater = GetStoryVariable<bool>("has_magic_water", false);
            string timeOfDay = GetStoryVariable<string>("time_of_day", "morning");
            int gnomeFriendship = GetStoryVariable<int>("gnome_friendship", 0);

            statsText.text = $"<b>Vitality:</b> {vitality}%\n" +
                            $"<b>Time:</b> {timeOfDay}\n" +
                            (hasMagicWater ? "<b>Magic Water Active</b>\n" : "") +
                            (gnomeFriendship > 0 ? $"<b>Gnome Trust:</b> {gnomeFriendship}" : "");

            statsText.fontSize = 18;
            statsText.color = Color.white;
            statsText.ForceMeshUpdate();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error updating game stats: {e.Message}");
            statsText.text = "Stats unavailable";
        }
    }

    T GetStoryVariable<T>(string variableName, T defaultValue)
    {
        try
        {
            object value = story.variablesState[variableName];
            return value != null ? (T)value : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    void DisplayEndOfStory()
    {
        if (storyText != null)
        {
            storyText.text = "The story has concluded.";
        }
        CreateContinueOrRestartButton();
    }

    void RestartStory()
    {
        InitializeStory();
    }
}