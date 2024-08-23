using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class displaySubtitles : MonoBehaviour
{
    public Sprite cardSprite;
    public Color cardColor;
    public TMP_FontAsset fontAsset;
    public Color highlightColor;

    // Needs to be set in order to work
    public MediaPlayer currentMediaPlayer;

    private const float OFFSET = .3f;

    // These phrases are changed when the panel is changed. There is a function that is called to change them
    float[] currentPhraseStartTimes;
    float[] currentPhraseEndTimes;
    float currentSceneStartTime;
    float currentSceneEndTime;

    List<string>[] currentTokens;
    List<float>[] currentTokenStartTimes;
    List<float>[] currentTokenEndTimes;

    float currentChapterStartTime = 0;
    float currentTime = 0;
    bool slideIsChanging = true;
    int lastPhraseIndex = -1;
    int lastWordIndex = -1;

    // 0 means attach to word behind it, 1 means attach to the word in front
    private Dictionary<string, int> punctuationMap = new Dictionary<string, int>
    {
        {".", 0},
        {",", 0},
        {";", 0},
        {":", 0},
        {"\'", 0},
        {"-", 0},
        {"...", 0},
        {"(", 1},
        {")", 0},
        {"[", 1},
        {"]", 0},
        {"{", 1},
        {"}", 0},
        {"¿", 1},
        {"?", 0},
        {"¡", 1},
        {"!", 0},
        {"*", 1},
        {"\"", 2}
    };

    void makeWordCard(string word)
    {

        // Creates the Word Card
        GameObject card = new GameObject($"{word}");
        setAsChild(card);
        RectTransform cardRect = card.AddComponent<RectTransform>();
        setRectSettings(cardRect, new Vector3(0, 0, 0), /* Set this later */ new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));

        GameObject background = new GameObject("Background");
        setAsChild(background, card);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        setRectSettings(backgroundRect, new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector3(1, 1, 1));
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = cardSprite;
        backgroundImage.color = cardColor;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.fillCenter = true;
        backgroundImage.pixelsPerUnitMultiplier = 1;

        GameObject wordText = new GameObject("Word Text");
        setAsChild(wordText, card);
        RectTransform wordTextRect = wordText.AddComponent<RectTransform>();
        setRectSettings(wordTextRect, new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));
        TextMeshProUGUI wordTextMesh = wordText.AddComponent<TextMeshProUGUI>();

        wordTextMesh.text = word;
        wordTextMesh.fontSize = 50;
        wordTextMesh.font = fontAsset;
        wordTextMesh.fontStyle = FontStyles.Bold;
        wordTextMesh.alignment = TextAlignmentOptions.Center;

        cardRect.sizeDelta = new Vector2(wordTextMesh.preferredWidth + 35, 90);
    }

    private GameObject createRow()
    {
        RectTransform thisRect = GetComponent<RectTransform>();
        GameObject row = new GameObject("Row");
        setAsChild(row, thisRect.gameObject);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        setRectSettings(rowRect, new Vector3(0, 0, 0), new Vector2(0, 100), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector3(1, 1, 1));
        row.AddComponent<Canvas>();
        row.AddComponent<GraphicRaycaster>();
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.padding.top = 10;
        hlg.padding.left = 10;
        hlg.padding.right = 10;
        hlg.padding.bottom = 10;
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;
        hlg.childScaleHeight = true;
        hlg.childScaleWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;

        return row;
    }

    
    private List<string> combinePunctuation(List<string> tokens)
    {
        string currentWord;
        
        List<string> newList = new List<string>();

        for (int i = 0; i < tokens.Count; i++)
        {
            currentWord = tokens[i];

            // if the current word is not punctuation
            if (!punctuationMap.ContainsKey(tokens[i])) {

                // Check to see if there is punctuation before
                if (i > 0)
                {
                    // if there is punctuation before
                    if (punctuationMap.ContainsKey(tokens[i - 1]) && punctuationMap[tokens[i - 1]] == 1 || tokens[i - 1] == "\"")
                    {
                        currentWord = tokens[i - 1] + currentWord;
                    }
                    if (i > 1)
                    {
                        if (punctuationMap.ContainsKey(tokens[i - 1]) && tokens[i - 2] == "\"")
                        {
                            currentWord = tokens[i - 2] + currentWord;
                        }
                    }
                }

                // Check to see if there is punctuation after
                if (i < tokens.Count - 1)
                {
                    // if there is punctuation before
                    if (punctuationMap.ContainsKey(tokens[i + 1]) && punctuationMap[tokens[i + 1]] == 0 )
                    {
                        currentWord = currentWord + tokens[i + 1];
                        i++;
                    }

                    if (i < tokens.Count - 1)
                    {
                        if (tokens[i + 1] == "\"")
                        {
                            currentWord = currentWord + tokens[i + 1];
                            i++;
                        }
                    } 
                }
                newList.Add(currentWord);
            }
            
        }
        return newList;
    }

    void generatePhraseWithTokens(List<string> tokens)
    {
        
        ClearCurrentSubtitles();

        if (tokens == null) { return; }

        RectTransform thisRect = GetComponent<RectTransform>();
        GameObject row = createRow();

        float totalTextWidth = 10;

        tokens = combinePunctuation(tokens);

        foreach (string word in tokens)
        {
            
            // This creates the word card
            string trimmedWord = word.Trim();
            makeWordCard(word);

            // This is to access the width of this card
            RectTransform wordRect = thisRect.Find($"{word}") as RectTransform;

            // If adding to the line will cause overflow, it will move it all to the top row and continue writing
            if (totalTextWidth + wordRect.sizeDelta.x + 10 > thisRect.sizeDelta.x)
            {
                row = createRow();
                totalTextWidth = 10;
            }

            // Puts it into the bottom row
            setAsChild(wordRect.gameObject, row);

            // The width of the bottom row grows
            totalTextWidth += wordRect.sizeDelta.x + 10;
        }
    }
    
    void generatePhrase(string phrase)
    {
        ClearCurrentSubtitles();

        if (phrase == null){ return; }

        RectTransform thisRect = GetComponent<RectTransform>();
        GameObject row = createRow();

        float totalTextWidth = 10;

        string[] words = phrase.Split();
        foreach (string word in words)
        {
            // This creates the word card
            string trimmedWord = word.Trim();
            makeWordCard(word);

            // This is to access the width of this card
            RectTransform wordRect = thisRect.Find($"{word}") as RectTransform;
            
            // If adding to the line will cause overflow, it will move it all to the top row and continue writing
            if (totalTextWidth + wordRect.sizeDelta.x + 10 > thisRect.sizeDelta.x)
            {
                row = createRow();
                totalTextWidth = 10;
            }
            
            // Puts it into the bottom row
            setAsChild(wordRect.gameObject, row);

            // The width of the bottom row grows
            totalTextWidth += wordRect.sizeDelta.x + 10;
        }
    }

    bool tokenErrorLogged;
    // Main function that updates the subtitles
    void updateSubtitles(List<string>[] tokens, float[] startTimes, float[] endTimes, float currentTime, List<float>[] tokenTimings)
    {
        if (tokens != null)
        {
            // Checks how much time has elapsed since the start of the video to find which subtitle should be displayed
            int currentPhraseIndex = findCurrentPhrase(startTimes, currentTime);

            // If the phrase is the same continue highlighting the cards
            if (currentPhraseIndex == lastPhraseIndex)
            {
                if (tokenTimings != null && currentPhraseIndex > 0 && currentPhraseIndex < tokenTimings.Length)
                {
                    highlightCurrentWord(tokenTimings[currentPhraseIndex], currentTime);
                }
            }
            else
            {
                // Sets the new state
                lastPhraseIndex = currentPhraseIndex;

                // If no phrase is supposed to be shown, clear all subtitles
                if (currentPhraseIndex == -1)
                {
                    // Generates a blank subtitles
                    generatePhraseWithTokens(null);
                    return;
                }
                if (currentPhraseIndex >= 0 && currentPhraseIndex < tokens.Length)
                {
                    // generates the phrase in the phrase index
                    generatePhraseWithTokens(tokens[currentPhraseIndex]);
                }
            }

        }
        else
        {
            if (!tokenErrorLogged)
            {
                Debug.Log("Whoops, there were no tokens!");
                tokenErrorLogged = true;
            }
        }
    }

    void highlightCurrentWord(List<float> tokenStartTimes, float currentTime)
    {
        if (tokenStartTimes != null)
        {
            int currentWordIndex = findCurrentWord(tokenStartTimes.ToArray(), currentTime);
            // Debug.Log($"Current Phrase to Highlight: {currentWordIndex}");

            // If the phrase is not supposed to change, don't do anything.
            if (currentWordIndex == lastWordIndex)
            {
                return;
            }
            else
            {
                int i = 0;

                RectTransform myRect = GetComponent<RectTransform>();

                // Go through each word in the row and find which needs to be highlighted and de highlighted.
                foreach (RectTransform row in myRect)
                {
                    // Debug.Log($"Searching Row");
                    foreach (RectTransform word in row)
                    {
                        // Debug.Log($"Searching Word");
                        if (i == lastWordIndex)
                        {
                            // Debug.Log($"Last Word Found! {lastWordIndex}");
                            HighlightWord(word, Color.white);
                        }
                        else if (i == currentWordIndex)
                        {
                            // Debug.Log($"Current Word Found! {currentWordIndex}");
                            HighlightWord(word, highlightColor);

                        }
                        i++;
                    }
                }

                lastWordIndex = currentWordIndex;
                // Debug.Log($"LastwordIndex: {lastWordIndex}");
            }
        }
        
    }

    void HighlightWord(RectTransform word, Color color)
    {
        // Find the child RectTransform named "Word Text"
        RectTransform wordText = word.Find("Word Text") as RectTransform;

        // Check if the child RectTransform is found
        if (wordText == null)
        {
            Debug.LogWarning("Child 'Word Text' not found.");
            return; // Exit if the child is not found
        }

        // Get the TextMeshProUGUI component from the child RectTransform
        TextMeshProUGUI wordTextMesh = wordText.GetComponent<TextMeshProUGUI>();

        // Check if the TextMeshProUGUI component is found
        if (wordTextMesh != null)
        {
            wordTextMesh.color = color;
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component not found on 'Word Text'.");
        }
    }

    private void ClearCurrentSubtitles() {
        // Destroy rows
        foreach (Transform row in transform)
        {
            Destroy(row.gameObject);
        }
    }

    private int findCurrentPhrase(float[] startTimes, float currentTime)
    {
        if (startTimes != null)
        {
            for (int i = 0; i < startTimes.Length; i++)
            {
                // If the next time doesn't exist or the the current time is before the next phrase time
                if (i + 1 == startTimes.Length || currentTime < convertPhraseTime(startTimes[i + 1] + OFFSET))
                {
                     return i;
                }
            }
        }
        return 0;
    }

    private int findCurrentWord(float[] startTimes, float currentTime)
    {
        if (startTimes != null)
        {
            for (int i = 0; i < startTimes.Length; i++)
            {
                if (currentTime < convertPhraseTime(startTimes[i] + OFFSET))
                {
                    return -1;
                }
                // If the next time doesn't exist or the the current time is before the next phrase time
                if (i + 1 == startTimes.Length || currentTime < convertPhraseTime(startTimes[i + 1] + OFFSET))
                {
                    return i;
                }
            }
        }
        return 0;
    }

    // This function handles the alignment between chapter start time and when the scene starts
    private float convertPhraseTime(float time)
    {
        return time + currentChapterStartTime - currentSceneStartTime;
    }

    // A slide is changed and new data is passed in to display
    public void setData(
        MediaPlayer mediaPlayer,
        float sceneStartTime,
        float sceneEndTime,
        List<string>[] tokens, 
        float[] phraseStartTimes, 
        float[] phraseEndTimes, 
        List<float>[] tokenStartTimes, 
        List<float>[] tokenEndTimes,
        float chapterStartTime)
    {
        slideIsChanging = true;

        currentSceneStartTime = sceneStartTime;
        currentSceneEndTime = sceneEndTime;
        currentPhraseEndTimes = phraseEndTimes;
        currentMediaPlayer = mediaPlayer;
        currentChapterStartTime = chapterStartTime;

        // These sometimes come in mixed up, this function will sort and delete any null tokens and their timings
        InsertionSort(ref tokens, ref tokenStartTimes, ref tokenEndTimes, ref phraseStartTimes);
        currentTokens = tokens;
        currentTokenStartTimes = tokenStartTimes;
        currentTokenEndTimes = tokenEndTimes;
        currentPhraseStartTimes = phraseStartTimes;

        // These reset the phrase and word indexes
        lastPhraseIndex = -1;
        lastWordIndex = -1;
        tokenErrorLogged = false;
        slideIsChanging = false;
    }

    // In some cases phrases are out of order. This function looks at the first start time of each and sorts the needed info
    private void InsertionSort(
        ref List<string>[] tokens, 
        ref List<float>[] tokenStartTimes, 
        ref List<float>[] tokenEndTimes,
        ref float[] startTimes) {
        if (tokens != null && tokenStartTimes != null && tokenEndTimes != null && startTimes != null)
        {
            for (int i = 1; i < tokenStartTimes.Length; i++)
            {
                if (tokenStartTimes[i] != null)
                {

                    float key = tokenStartTimes[i][0];

                    List<float> temp1 = tokenStartTimes[i];
                    List<string> temp2 = tokens[i];
                    List<float> temp3 = tokenEndTimes[i];
                    float temp4 = startTimes[i];

                    int j = i - 1;

                    // Move elements of arr[0..i-1], that are greater than key,
                    // to one position ahead of their current position
                    while (j >= 0 && tokenStartTimes[j][0] > key)
                    {
                        tokenStartTimes[j + 1] = tokenStartTimes[j];
                        tokens[j + 1] = tokens[j];
                        tokenEndTimes[j + 1] = tokenEndTimes[j];
                        startTimes[j + 1] = startTimes[j];
                        j--;
                    }

                    tokenStartTimes[j + 1] = temp1;
                    tokens[j + 1] = temp2;
                    tokenEndTimes[j + 1] = temp3;
                    startTimes[j + 1] = temp4;

                }
            }

            CleanUpList(ref tokens, ref tokenStartTimes, ref tokenEndTimes, ref startTimes);

        }
    }

    // Sometimes there are phrases without tokens, but contiain timings, this removes any blank tokens and there corresponding timing
    private void CleanUpList(
        ref List<string>[] tokens, 
        ref List<float>[] tokenStartTimes, 
        ref List<float>[] tokenEndTimes,
        ref float[] startTimes)
    {
        List<List<string>> newList = new List<List<string>>();
        List<List<float>> newList2 = new List<List<float>>();
        List<List<float>> newList3 = new List<List<float>>();
        List<float> newList4 = new List<float>();

        for (int i = 0; i < tokens.Length; i ++) {

            // If the token is not empty, add it to the temporary list
            if (tokens[i].Count != 0)
            {
                newList.Add(tokens[i]);
                newList2.Add(tokenStartTimes[i]);
                newList3.Add(tokenEndTimes[i]);
                newList4.Add(startTimes[i]);
            }

        }

        // here it says the assignment is unecessary
        tokens = newList.ToArray();
        tokenStartTimes = newList2.ToArray();
        tokenEndTimes = newList3.ToArray();
        startTimes = newList4.ToArray();

    }

    // Update is called once per frame
    void Update()
    {
        if (DataIsLoaded())
        {
            currentTime = (float)currentMediaPlayer.Control.GetCurrentTime();

            if (slideIsChanging == false)
            {
                updateSubtitles(currentTokens, currentPhraseStartTimes, currentPhraseEndTimes, currentTime, currentTokenStartTimes);
            }
            else
            {
                ClearCurrentSubtitles();
            }
        }
    }

    // Checks to see if all data is loaded before displaying
    private bool DataIsLoaded()
    {
        if (currentTokens != null &&
        currentPhraseStartTimes != null &&
        currentPhraseEndTimes != null && currentMediaPlayer != null)
        {
            return true;
        }
        return false;
    }
    private void PrintTiming(List<float> list)
    {
        string timings = " ";

        for (int i = 0; i < list.Count; i++)
        {
            timings += convertPhraseTime(list[i]).ToString() + " ";
        }

        Debug.Log(timings);
    }

    private void PrintTokens(List<string>[] list)
    {
        string timings = " ";

        for (int i = 0; i < list.Length; i++)
        {
            for (int j = 0; j < list[i].Count; j++)
            {
                timings += list[i][j] + " ";
            }
            timings += "\n";
        }

        Debug.Log(timings);
    }
    private void setAsChild(GameObject child, GameObject parent = null)
    {
        if (parent != null)
        {
            child.transform.SetParent(parent.transform);
            return;
        }
        child.transform.SetParent(transform);


    }
    private void setRectSettings(RectTransform rect, Vector3 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector3 scale)
    {
        rect.anchoredPosition3D = position;
        rect.sizeDelta = size;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.localScale = scale;

    }
}
