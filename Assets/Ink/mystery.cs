using Ink.Runtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class Mystery : MonoBehaviour
{
    [Header("Ink Assets")]
    public TextAsset inkJSON;
    private Story story;

    [Header("UI Elements")]
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI debugText; // Added for debugging
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;

    [Header("Visual Elements")]
    public Image backgroundImage;
    public Sprite[] environmentSprites; // Array containing forest, meadow, cabin, grotto sprites
    public Image timeOfDayOverlay;
    public Color morningTint = Color.white;
    public Color afternoonTint = new Color(1f, 0.9f, 0.7f, 1f);
    public Color eveningTint = new Color(0.8f, 0.6f, 0.4f, 1f);
    public Color nightTint = new Color(0.3f, 0.3f, 0.5f, 1f);

    [Header("Choice Elements")]
    public Sprite[] choiceImages; // Array for choice-specific images

    [Header("Debug Settings")]
    public bool enableDebugMode = true;
    public string defaultLocation = "forest";
    public string startingKnot = "start";

    void Start()
    {
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON file not assigned!");
            return;
        }

        try
        {
            story = new Story(inkJSON.text);
            if (enableDebugMode)
                Debug.Log("Story initialized successfully");

            // IMPORTANT: Explicitly choose the starting path
            story.ChoosePathString(startingKnot);
            if (enableDebugMode)
                Debug.Log("Starting story at: " + startingKnot);

            RefreshView();
        }
        catch (Exception e)
        {
            Debug.LogError("Error initializing story: " + e.Message);
        }
    }

    void Update()
    {
        // Added debug overlay
        if (debugText != null && enableDebugMode)
        {
            debugText.text = $"Story null? {story == null}\n" +
                $"Can continue? {story?.canContinue}\n" +
                $"Choices: {story?.currentChoices.Count}\n" +
                $"StoryText null? {storyText == null}\n" +
                $"Button Container null? {choiceButtonContainer == null}\n" +
                $"Button Prefab null? {choiceButtonPrefab == null}\n" +
                $"Choice Images: {choiceImages?.Length ?? 0}";

            // Check specific buttons with images
            int buttonsWithImages = 0;
            if (choiceButtonContainer != null)
            {
                foreach (Transform child in choiceButtonContainer)
                {
                    Image img = child.Find("ChoiceImage")?.GetComponent<Image>();
                    if (img != null && img.sprite != null && img.gameObject.activeInHierarchy)
                        buttonsWithImages++;
                }
                debugText.text += $"\nButtons with active images: {buttonsWithImages}";
            }
        }
    }

    void RefreshView()
    {
        if (story == null)
        {
            Debug.LogError("Story is null in RefreshView!");
            return;
        }

        if (enableDebugMode)
            Debug.Log("Refreshing view...");

        // Reset UI
        if (storyText != null)
            storyText.text = "";
        else
            Debug.LogError("Story Text component not assigned!");

        // Clear existing choice buttons
        if (choiceButtonContainer != null)
        {
            foreach (Transform child in choiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("Choice Button Container is not assigned!");
            return;
        }

        // Continue the story until we hit a choice
        if (story.canContinue)
        {
            string textFragment = story.Continue();
            if (storyText != null)
            {
                // Enhanced text update with forced mesh update
                storyText.text = textFragment;
                storyText.ForceMeshUpdate(true, true); // Force complete update

                // Enhance text visibility
                storyText.fontSize = 36;
                storyText.color = Color.white;
                storyText.fontStyle = FontStyles.Bold;
                storyText.alignment = TextAlignmentOptions.Center;
                storyText.textWrappingMode = TextWrappingModes.Normal; // Corrected line
                storyText.paragraphSpacing = 15;
                storyText.margin = new Vector4(20, 20, 20, 20);

                if (enableDebugMode)
                {
                    Debug.Log($"Text updated to: '{textFragment.Substring(0, Mathf.Min(50, textFragment.Length))}...'");
                    Debug.Log($"StoryText component null? {storyText == null}");
                    Debug.Log($"Setting text to: {textFragment}");
                    Debug.Log($"After setting, text is: {storyText.text}");
                }
            }
            else
            {
                Debug.LogError("Story Text object is null!");
            }
        }
        else if (enableDebugMode)
        {
            Debug.Log("Story cannot continue");
        }

        // Update visual environment based on story tags
        UpdateEnvironment();

        // Update stats UI
        UpdateStats();

        // Create choice buttons
        DisplayChoices();
    }

    void DisplayChoices()
    {
        if (story == null || choiceButtonContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError($"Missing references! Story null? {story == null}, Container null? {choiceButtonContainer == null}, Prefab null? {choiceButtonPrefab == null}");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        List<Choice> currentChoices = story.currentChoices;

        if (enableDebugMode)
        {
            Debug.Log($"Displaying {currentChoices.Count} choices");

            // Debug each available choice
            for (int i = 0; i < currentChoices.Count; i++)
            {
                Debug.Log($"Choice {i}: {currentChoices[i].text}");
            }
        }

        // Handle case with no choices
        if (currentChoices.Count == 0)
        {
            // No choices found - check if story can continue
            if (story.canContinue)
            {
                // Still has content, add a "Continue" button
                Button continueButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();

                // Enhance button visibility
                Image buttonImage = continueButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = Color.white;
                }

                // Style button text
                if (buttonText != null)
                {
                    buttonText.text = "Continue...";
                    buttonText.fontSize = 24;
                    buttonText.color = new Color(0.1f, 0.1f, 0.2f); // Dark blue-black
                    buttonText.fontStyle = FontStyles.Bold;
                    buttonText.alignment = TextAlignmentOptions.Center;
                    buttonText.ForceMeshUpdate();
                }

                // Add continue image if available
                Transform choiceImageTransform = continueButton.transform.Find("ChoiceImage");
                if (choiceImageTransform != null && choiceImages != null && choiceImages.Length > 0)
                {
                    Image choiceImageComponent = choiceImageTransform.GetComponent<Image>();
                    if (choiceImageComponent != null && choiceImages[0] != null)
                    {
                        choiceImageComponent.sprite = choiceImages[0]; // Use first image for continue
                        choiceImageComponent.gameObject.SetActive(true);

                        if (enableDebugMode)
                            Debug.Log("Set continue button image");
                    }
                }

                // Size the button properly
                RectTransform buttonRect = continueButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.sizeDelta = new Vector2(600, 60);
                }

                // Add click listener
                continueButton.onClick.AddListener(() => {
                    RefreshView(); // Continue the story
                    if (enableDebugMode)
                        Debug.Log("Continue button clicked");
                });

                // Force layout rebuild
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceButtonContainer.GetComponent<RectTransform>());

                return;
            }
            else if (!story.canContinue)
            {
                // End of story - add restart button
                Button restartButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                TextMeshProUGUI buttonText = restartButton.GetComponentInChildren<TextMeshProUGUI>();

                // Enhance button visibility
                Image buttonImage = restartButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = Color.white;
                }

                // Style button text
                if (buttonText != null)
                {
                    buttonText.text = "Restart Story";
                    buttonText.fontSize = 24;
                    buttonText.color = new Color(0.1f, 0.1f, 0.2f); // Dark blue-black
                    buttonText.fontStyle = FontStyles.Bold;
                    buttonText.alignment = TextAlignmentOptions.Center;
                    buttonText.ForceMeshUpdate();
                }

                // Size the button properly
                RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.sizeDelta = new Vector2(600, 60);
                }

                restartButton.onClick.AddListener(() => {
                    RestartStory();
                    if (enableDebugMode)
                        Debug.Log("Restart button clicked");
                });

                // Force layout rebuild
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceButtonContainer.GetComponent<RectTransform>());

                return;
            }
        }

        // Create choice buttons
        for (int i = 0; i < currentChoices.Count; i++)
        {
            Choice choice = currentChoices[i];
            Button choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);

            // Ensure button is active
            choiceButton.gameObject.SetActive(true);

            // Enhance button visibility
            Image buttonImage = choiceButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white;
            }

            TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = choice.text;
                // Style button text
                buttonText.fontSize = 24;
                buttonText.color = new Color(0.1f, 0.1f, 0.2f); // Dark blue-black
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.ForceMeshUpdate();

                if (enableDebugMode)
                    Debug.Log($"Created button with text: '{choice.text}'");
            }

            // Add choice image
            Transform choiceImageTransform = choiceButton.transform.Find("ChoiceImage");
            if (choiceImageTransform != null)
            {
                Image choiceImageComponent = choiceImageTransform.GetComponent<Image>();
                if (choiceImageComponent != null && choiceImages != null && choiceImages.Length > 0)
                {
                    // Determine which image to use based on choice content
                    int imageIndex = DetermineImageIndex(choice.text);

                    if (imageIndex >= 0 && imageIndex < choiceImages.Length &&
                        choiceImages[imageIndex] != null)
                    {
                        choiceImageComponent.sprite = choiceImages[imageIndex];
                        choiceImageComponent.gameObject.SetActive(true);

                        if (enableDebugMode)
                            Debug.Log($"Set choice image {imageIndex} for choice: '{choice.text}'");
                    }
                    else
                    {
                        // Hide the image if no valid sprite
                        choiceImageComponent.gameObject.SetActive(false);

                        if (enableDebugMode)
                            Debug.Log($"No valid image found for choice: '{choice.text}'");
                    }
                }
            }

            // Size the button properly
            RectTransform buttonRect = choiceButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(600, 60);
            }

            int choiceIndex = i;
            choiceButton.onClick.RemoveAllListeners();
            choiceButton.onClick.AddListener(() => {
                if (enableDebugMode)
                    Debug.Log($"Button clicked: {choiceIndex}");
                MakeChoice(choiceIndex);
            });

            // Handle vitality-based button state
            try
            {
                object vitalityObj = story.variablesState["player_vitality"];
                if (vitalityObj != null)
                {
                    int vitality = (int)vitalityObj;
                    if (vitality < 20 && choice.text.Contains("Continue"))
                    {
                        choiceButton.interactable = false;
                        if (buttonText != null)
                        {
                            buttonText.text += " (Too exhausted)";
                            buttonText.color = new Color(0.5f, 0.5f, 0.5f);
                        }
                    }
                }
            }
            catch { }
        }

        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(choiceButtonContainer.GetComponent<RectTransform>());
    }

    // Helper method to determine image index from choice text
    private int DetermineImageIndex(string choiceText)
    {
        // Simple approach - look for keywords in the choice text
        choiceText = choiceText.ToLower();

        if (choiceText.Contains("forest") || choiceText.Contains("walk") ||
            choiceText.Contains("tree"))
            return 0;
        else if (choiceText.Contains("meadow") || choiceText.Contains("field") ||
                choiceText.Contains("flower"))
            return 1;
        else if (choiceText.Contains("cabin") || choiceText.Contains("house") ||
                choiceText.Contains("shelter"))
            return 2;
        else if (choiceText.Contains("grotto") || choiceText.Contains("cave") ||
                choiceText.Contains("water"))
            return 3;

        // Default - return first image or -1 if you want no image
        return 0;
    }

    void MakeChoice(int choiceIndex)
    {
        if (story == null)
            return;

        if (enableDebugMode)
            Debug.Log($"Selected choice: {choiceIndex}");

        story.ChooseChoiceIndex(choiceIndex);
        RefreshView();
    }

    void UpdateStats()
    {
        if (story == null || statsText == null)
            return;

        try
        {
            // Safely get variables using try/catch
            int vitality = 100;
            bool hasMagicWater = false;
            string timeOfDay = "morning";
            int gnomeFriendship = 0;

            try { vitality = (int)story.variablesState["player_vitality"]; } catch { }
            try { hasMagicWater = (bool)story.variablesState["has_magic_water"]; } catch { }
            try { timeOfDay = (string)story.variablesState["time_of_day"]; } catch { }
            try { gnomeFriendship = (int)story.variablesState["gnome_friendship"]; } catch { }

            // Enhanced stats display with rich text formatting
            statsText.text = $"<b>Vitality:</b> {vitality}%\n" +
                            $"<b>Time:</b> {timeOfDay}\n" +
                            (hasMagicWater ? "<b>Magic Water Active</b>\n" : "") +
                            (gnomeFriendship > 0 ? $"<b>Gnome Trust:</b> {gnomeFriendship}" : "");

            // Style the stats text
            statsText.fontSize = 18;
            statsText.color = Color.white;
            statsText.ForceMeshUpdate();

            // Style the stats panel background if needed
            Image statsPanel = statsText.transform.parent.GetComponent<Image>();
            if (statsPanel != null)
            {
                statsPanel.color = new Color(0f, 0f, 0f, 0.7f); // Semi-transparent black
            }

            if (enableDebugMode)
                Debug.Log($"Stats updated: Vitality={vitality}, Time={timeOfDay}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error updating stats: " + e.Message);
            statsText.text = "Stats unavailable";
            statsText.ForceMeshUpdate();
        }
    }

    void UpdateEnvironment()
    {
        if (story == null)
            return;

        try
        {
            // Get environment from tags
            List<string> tags = story.currentTags;
            string location = defaultLocation;

            if (tags != null && tags.Count > 0)
            {
                location = tags[0];
                if (enableDebugMode)
                    Debug.Log($"Found location tag: {location}");
            }
            else
            {
                if (enableDebugMode)
                    Debug.Log($"No location tag found, using default: {defaultLocation}");
            }

            // Get time of day safely
            string timeOfDay = "morning";
            try
            {
                object timeObj = story.variablesState["time_of_day"];
                if (timeObj != null)
                    timeOfDay = (string)timeObj;
            }
            catch { }

            // Update background based on location
            if (backgroundImage != null && environmentSprites != null && environmentSprites.Length > 0)
            {
                int locationIndex = -1;

                switch (location.ToLower())
                {
                    case "forest":
                        locationIndex = 0;
                        break;
                    case "meadow":
                        locationIndex = 1;
                        break;
                    case "cabin":
                        locationIndex = 2;
                        break;
                    case "grotto":
                        locationIndex = 3;
                        break;
                    default:
                        if (enableDebugMode)
                            Debug.Log($"Unknown location tag: {location}, defaulting to forest");
                        locationIndex = 0; // Default to forest
                        break;
                }

                if (locationIndex >= 0 && locationIndex < environmentSprites.Length &&
                    environmentSprites[locationIndex] != null)
                {
                    backgroundImage.sprite = environmentSprites[locationIndex];
                    if (enableDebugMode)
                        Debug.Log($"Set background to {location} (sprite index: {locationIndex})");
                }
                else
                {
                    Debug.LogWarning($"Invalid sprite index: {locationIndex} or missing sprite");
                }
            }
            else
            {
                Debug.LogWarning("Background image or environment sprites not set");
            }

            // Update lighting based on time of day
            if (timeOfDayOverlay != null)
            {
                switch (timeOfDay.ToLower())
                {
                    case "morning":
                        timeOfDayOverlay.color = morningTint;
                        break;
                    case "afternoon":
                        timeOfDayOverlay.color = afternoonTint;
                        break;
                    case "evening":
                        timeOfDayOverlay.color = eveningTint;
                        break;
                    case "night":
                        timeOfDayOverlay.color = nightTint;
                        break;
                    default:
                        if (enableDebugMode)
                            Debug.Log($"Unknown time of day: {timeOfDay}, defaulting to morning");
                        timeOfDayOverlay.color = morningTint; // Default
                        break;
                }

                if (enableDebugMode)
                    Debug.Log($"Set time of day overlay for: {timeOfDay}");
            }
            else
            {
                Debug.LogWarning("Time of day overlay not set");
            }

            // Add a semi-transparent darkening layer to StoryPanel for better text visibility
            Image storyPanel = storyText?.transform.parent?.GetComponent<Image>();
            if (storyPanel != null)
            {
                storyPanel.color = new Color(0f, 0f, 0f, 0.6f); // Semi-transparent black
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error in UpdateEnvironment: " + e.Message + "\n" + e.StackTrace);
        }
    }

    // Method to restart the story
    public void RestartStory()
    {
        if (inkJSON != null)
        {
            try
            {
                story = new Story(inkJSON.text);
                if (enableDebugMode)
                    Debug.Log("Story restarted");

                // Always choose the starting path when restarting
                story.ChoosePathString(startingKnot);
                if (enableDebugMode)
                    Debug.Log("Restarting story at: " + startingKnot);

                RefreshView();
            }
            catch (Exception e)
            {
                Debug.LogError("Error restarting story: " + e.Message);
            }
        }
    }
}