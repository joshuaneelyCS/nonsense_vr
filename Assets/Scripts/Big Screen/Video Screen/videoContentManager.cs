using Newtonsoft.Json;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class loadVideoContent : MonoBehaviour
{

    public TMP_FontAsset fontAsset;
    public Material fontMaterial;
    public Sprite circleSpriteHorizontal;
    public Sprite circleSpriteVertical;
    public Sprite circleSpriteCenter;
    public Sprite iconPlay;
    public Sprite iconPause;
    public Color baseColor;
    public Color progressColor;
    public Color handleColor;
    public displaySubtitles subtitles;

    // Loads all content
    private GraphQLFetcher fetchScript;
    private List<VideoPanelData> _newData = new List<VideoPanelData>();
    private List<VideoPanelData> _currentData = new List<VideoPanelData>();

    // This query takes in the id of the previous movie. I tried to adapt it to the sceneconnection so I can directly input the id of the movie chosen, however, the phraseConnection data was null so I couldn't 
    string QUERY_GET_VIDEO_SCREEN_CONTENT;
    
    string GetQueryWithAfterField(string afterField)
    {
        // Escape double quotes and backslashes
        string escapedAfterField = afterField.Replace("\\", "\\\\").Replace("\"", "\\\"");

        // Construct the query string
        return $"{{ \"query\": \"query {{ titleConnection(first: 1, after: \\\"{escapedAfterField}\\\", languageCode: es, licensedOnly: true, hasPublishedScenes: true) {{ edges {{ cursor movie: node {{ ... on TitleInterface {{ title id allScenes(languageCode: es) {{ mediaTitle: title portraitVideoUrl chapter {{ timeStart }} timeStart timeEnd phraseConnection {{ edges {{ words: node {{ text timeStart timeEnd tokens {{ text }} tokenTimings {{ timeStart timeEnd }} }} }} }} }} }} }} }} }} }}\" }}";
    }

    /* DESCRIPTION: This script manages receiving video content, creating the components needed 
     * to display each video, and managing the swiping between them
     */

    // creates a screen for each video
    private void createScreenPanel(VideoPanelData data)
    {

        // CREATE CANVAS
        GameObject canvas = new GameObject("Video");
        setAsChild(canvas);
        RectTransform canvasRect = canvas.AddComponent<RectTransform>();
        setRectSettings(canvasRect, new Vector3(0, 0, 0), new Vector2(390, 392), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector3(1, 1, 1));

        VerticalLayoutGroup canvasLayout = canvas.AddComponent<VerticalLayoutGroup>();
        canvasLayout.childAlignment = TextAnchor.UpperLeft;
        canvasLayout.padding.left = 45;
        canvasLayout.padding.right = 45;
        canvasLayout.childControlHeight = false;
        canvasLayout.childControlWidth = false;
        canvasLayout.childScaleHeight = true;
        canvasLayout.childScaleWidth = true;
        canvasLayout.childForceExpandHeight = false;
        canvasLayout.childForceExpandWidth = false;

        // CREATE HEADER
        GameObject header = new GameObject("Header");
        setAsChild(header, canvas);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        setRectSettings(headerRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(/* will need to be dynamic*/ 300, 40), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),/*SCALE*/ new Vector3(.782f, 1, 1));
        TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = data.mediaTitle;
        headerText.fontSize = 20;
        headerText.color = Color.gray;
        headerText.alignment = TextAlignmentOptions.Left;
        headerText.font = fontAsset;

        /* CREATE VIDEO PLAYER */

        // Screen on which the video is displayed
        GameObject videoScreen = new GameObject("Screen");
        setAsChild(videoScreen, canvas);
        RectTransform videoScreenRect = videoScreen.AddComponent<RectTransform>();
        setRectSettings(videoScreenRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(300, 300), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),/*SCALE*/ new Vector3(1, 1, 1));

        CanvasRenderer videoScreenRenderer = videoScreen.AddComponent<CanvasRenderer>();
        
        // Creates the media player
        DisplayUGUI AVProScreenDisplay = videoScreen.AddComponent<DisplayUGUI>();
        AVProScreenDisplay.ScaleMode = ScaleMode.ScaleAndCrop; // TODO change later when landscape videos come in

        // Play and Pause Button
        GameObject playButton = new GameObject("Play Pause Button");
        setAsChild(playButton, videoScreen);
        RectTransform playButtonRect = playButton.AddComponent<RectTransform>();
        setRectSettings(playButtonRect, /*POS*/new Vector3(0, 0, -15),/*SIZE*/new Vector2(120, 120), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(0.38f, 0.56f, 0.56f));
        
            // Circle sprite and black background color
        Image playButtonImage = playButton.AddComponent<Image>();
        playButtonImage.sprite = circleSpriteVertical;
        Color darkGrayColor = new Color(0.094f, 0.094f, 0.094f);
        darkGrayColor.a = 0.5f;
        playButtonImage.color = darkGrayColor;
        playButtonImage.raycastTarget = false;
        playButtonImage.type = Image.Type.Sliced;
        playButtonImage.fillCenter = true;
        playButtonImage.pixelsPerUnitMultiplier = 1;
            
            // Play Icon in the middle of the button
        GameObject playButtonIcon = new GameObject("Icon");
        setAsChild(playButtonIcon, playButton);
        RectTransform playButtonIconRect = playButtonIcon.AddComponent<RectTransform>();
        setRectSettings(playButtonIconRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(38, 38), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
        Image playButtonIconImage = playButtonIcon.AddComponent<Image>();
        playButtonIconImage.sprite = iconPlay;

        //********************
        // Video player which plays the video
        GameObject myVideoPlayer = new GameObject("Video Player");
        setAsChild(myVideoPlayer, videoScreen);
        RectTransform videoPlayerRect = myVideoPlayer.AddComponent<RectTransform>();
        setRectSettings(videoPlayerRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1),/*SCALE*/ new Vector3(1, 1, 1));
        
        MediaPlayer AVProMediaPlayer = myVideoPlayer.AddComponent<MediaPlayer>();

        //This sets the data's internal media player
        data.mediaPlayer = AVProMediaPlayer;

        AVProMediaPlayer.Loop = true;
        AVProMediaPlayer.AutoStart = false;

        // creates the media path
        MediaPath mediaPath = new MediaPath();
        mediaPath.PathType = MediaPathType.AbsolutePathOrURL;
        mediaPath.Path = $"https://files.wordscenes.com/{data.portriatVideoUrl}";

        // sets the mediapath to the path in the player
        AVProMediaPlayer.MediaPath = mediaPath;
        
        // Prepare and play the video
        AVProScreenDisplay.CurrentMediaPlayer = AVProMediaPlayer;

        // This manages the play/pause, video slider, and time stamp
        VideoTimeScrubControl videoTimeScrub = myVideoPlayer.AddComponent<VideoTimeScrubControl>();

        // When the screen is clicked, play or pause the video
        Button videoPlayerButton = videoScreen.AddComponent<Button>();
        videoPlayerButton.onClick.AddListener(videoTimeScrub.PlayOrPauseVideo);

        // Visual handle of the play pause button
        videoTimeScrub.m_ButtonPlayOrPause = playButton;
        videoTimeScrub.m_ButtonPlayOrPauseIcon = playButtonIconImage;
        videoTimeScrub.m_IconPlay = iconPlay;

        // CREATE SLIDER AND TIMESTAMP
        GameObject videoPlayerUI = new GameObject("Video Player UI");
            setAsChild(videoPlayerUI, canvas);
            RectTransform videoPlayerSliderRect = videoPlayerUI.AddComponent<RectTransform>();
                setRectSettings(videoPlayerSliderRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(300, 52), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),/*SCALE*/ new Vector3(1, 1, 1));
            HorizontalLayoutGroup videoLayoutGroup = videoPlayerUI.AddComponent<HorizontalLayoutGroup>();
                videoLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                videoLayoutGroup.childControlHeight = false;
                videoLayoutGroup.childControlWidth = false;
                videoLayoutGroup.childScaleHeight = false;
                videoLayoutGroup.childScaleWidth = false;
                videoLayoutGroup.childForceExpandHeight = true;
                videoLayoutGroup.childForceExpandWidth = true;

            GameObject timeText = new GameObject("Time Text");
                setAsChild(timeText, videoPlayerUI);
                RectTransform timeTextRect = timeText.AddComponent<RectTransform>();
                    setRectSettings(timeTextRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(98, 52), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
                TextMeshProUGUI timeTextMesh = timeText.AddComponent<TextMeshProUGUI>();
                    timeTextMesh.text = "0:00";
                    timeTextMesh.fontSize = 12;
                    timeTextMesh.color = Color.white;
                    timeTextMesh.alignment = TextAlignmentOptions.Center;
                    timeTextMesh.font = fontAsset;
                    timeTextMesh.material = fontMaterial;

            GameObject videoSlider = new GameObject("Slider");
                setAsChild(videoSlider, videoPlayerUI);
                RectTransform videoSliderRect = videoSlider.AddComponent<RectTransform>();
                    setRectSettings(videoSliderRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(201, 50), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),/*SCALE*/ new Vector3(1, 1, 1));
                Slider slider = videoSlider.AddComponent<Slider>();

                // Add the event triggers that will allow the user to click and drag the slider
                EventTrigger sliderEventTrigger = videoSlider.AddComponent<EventTrigger>();

                    // Sets up triggers
                    EventTrigger.Entry sliderPointerUpEntry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp
                    };
                    EventTrigger.Entry sliderDragEntry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.Drag
                    };
                    EventTrigger.Entry sliderPointerDownEntry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerDown
                    };

                        // Adds a listener to determine when the slider is released
                        sliderPointerUpEntry.callback.AddListener((eventData) => { videoTimeScrub.OnRelease(); });

                        // Adds a listener to determine when the slider is dragged
                        sliderDragEntry.callback.AddListener((eventData) => { videoTimeScrub.OnDrag(); });

                        // Adds a listener to determine when the slider is dragged
                        sliderPointerDownEntry.callback.AddListener((eventData) => { videoTimeScrub.OnPointerDown(); });

                            // Slider Audio (FUTURE)
                            // pointerDownEntry.callback.AddListener((eventData) => { videoTimeScrub.OnPointerDown(); });

                            // Add a pointer enter audio function
    
                    // Add the entries to the EventTrigger's triggers list
                    sliderEventTrigger.triggers.Add(sliderPointerUpEntry);
                    sliderEventTrigger.triggers.Add(sliderDragEntry);
                    sliderEventTrigger.triggers.Add(sliderPointerDownEntry);

        
        GameObject background = new GameObject("Background");
        setAsChild(background, videoSlider);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        setRectSettings(backgroundRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(0, -38), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = circleSpriteHorizontal;
        backgroundImage.color = baseColor;
        backgroundImage.raycastTarget = false;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.fillCenter = true;
        backgroundImage.pixelsPerUnitMultiplier = 30;

        GameObject hitTarget = new GameObject("Hit Target");
        setAsChild(hitTarget, videoSlider);
        RectTransform hitTargetRect = hitTarget.AddComponent<RectTransform>();
        setRectSettings(hitTargetRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));

        GameObject fillArea = new GameObject("Fill Area");
        setAsChild(fillArea, videoSlider);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        setRectSettings(fillAreaRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(0, -38), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
        
        GameObject fill = new GameObject("Fill");
        setAsChild(fill, fillArea);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        setRectSettings(fillRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = circleSpriteCenter;
        fillImage.color = progressColor;
        fillImage.type = Image.Type.Sliced;
        fillImage.fillCenter = true;
        fillImage.pixelsPerUnitMultiplier = 10;

        GameObject handleSlideArea = new GameObject("Handle Slide Area");
        setAsChild(handleSlideArea, videoSlider);
        RectTransform handleSlideAreaRect = handleSlideArea.AddComponent<RectTransform>();
        setRectSettings(handleSlideAreaRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));

        GameObject handle = new GameObject("Handle");
        setAsChild(handle, handleSlideArea);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        setRectSettings(handleRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(40, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
        Image handleImage = handle.AddComponent<Image>();
        Color clear = Color.white;
        clear.a = 0;
        handleImage.color = clear;

        GameObject handleVisual = new GameObject("Handle Visual");
        setAsChild(handleVisual, handle);
        RectTransform handleVisualRect = handleVisual.AddComponent<RectTransform>();
        setRectSettings(handleVisualRect, /*POS*/new Vector3(0, 0, 0),/*SIZE*/new Vector2(18, 27), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),/*SCALE*/ new Vector3(1, 1, 1));
        Image handleVisualImage = handleVisual.AddComponent<Image>();
        handleVisualImage.sprite = circleSpriteVertical;
        handleVisualImage.color = handleColor;
        handleVisualImage.type = Image.Type.Sliced;
        handleVisualImage.fillCenter = true;
        handleVisualImage.pixelsPerUnitMultiplier = 1;

        // Finish configuring settings on the slider
        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;

        // ATTACH SLIDER, TIMESTAMP, AND PLAY AND PAUSE BUTTON TO THE VideoTimeScrubControl Object
        videoTimeScrub.m_Slider = slider;
        videoTimeScrub.m_VideoTimeText = timeTextMesh;
        videoTimeScrub.OnEnable();

    }

    // Start is called before the first frame update
    void Start()
    {

        QUERY_GET_VIDEO_SCREEN_CONTENT = GetQueryWithAfterField(SceneDataToSend.SceneCursor);


        LoadContent();

        content = GetComponent<RectTransform>();
        targetPosition = content.anchoredPosition;
        currentPosition = content.anchoredPosition;

        
    }

    // Loads the Content
    private void LoadContent()
    {
        // Starts fetching data from the API
        fetchScript = FindObjectOfType<GraphQLFetcher>();
        StartCoroutine(FetchDataAndProcess());
    }

    // Makes a request to the API using a Query. It then Parses and Stores the data.
    private IEnumerator FetchDataAndProcess()
    {
        string query = QUERY_GET_VIDEO_SCREEN_CONTENT;

        // Start fetching data
        yield return StartCoroutine(fetchScript.FetchData(query));

        // When the data is fetched, set its contents to a local variable
        string fetchedData = fetchScript.returnFetchedData();

        ParseVideoData(fetchedData);

        // Now that data is fetched, access the list
        if (fetchedData != null)
        {
            // Adds the fetched data to the current data list
            foreach (VideoPanelData data in _newData)
            {
                createScreenPanel(data);
            }

            _currentData = HelperFunctions.JoinLists(_currentData, _newData);

            updateContentSize();

            // Resets the Subtitles index
            ChangeSubtitles();
            }
    }

    // Takes the returned Json and parses it into usable objects
    private void ParseVideoData(string jsonResponse)
    {
        // Takes all the fetched data and puts it into a usable object

        // Parse the JSON response
        var response = JsonConvert.DeserializeObject<GraphQLResponse>(jsonResponse);

        // Access the data
        if (response != null && response.data != null && response.data.titleConnection != null)
        {
            foreach (var edge in response.data.titleConnection.edges)
            {
                if (edge.movie != null)
                {
                    if (edge.movie.allScenes != null)
                    {
                        foreach (var scene in edge.movie.allScenes)
                        {
                            if (scene.portraitVideoUrl != null)
                            {
                                VideoPanelData data = new VideoPanelData();

                                // Add each scene's details to a new array
                                data.mediaTitle = scene.mediaTitle;

                                data.portriatVideoUrl = m3u8_to_mpd(scene.portraitVideoUrl);

                                data.sceneTimeStart = scene.timeStart;
                                data.sceneTimeEnd = scene.timeEnd;

                                if (scene.chapter != null)
                                {
                                    data.chapterTimeStart = scene.chapter.timeStart;
                                }

                                if (scene.phraseConnection != null)
                                {

                                    foreach (var phraseEdge in scene.phraseConnection.edges)
                                    {
                                        if (phraseEdge.words != null)
                                        {
                                            data.phrasesText.Add(phraseEdge.words.text);
                                            data.phrasesTimeStart.Add(float.Parse(phraseEdge.words.timeStart));
                                            data.phrasesTimeEnd.Add(float.Parse(phraseEdge.words.timeEnd));

                                            List<string> tokens = new List<string>();

                                            foreach (var token in phraseEdge.words.tokens)
                                            {
                                                tokens.Add(token.text);
                                            }

                                            data.tokens.Add(tokens);

                                            List<float> timeStart = new List<float>();
                                            List<float> timeEnd = new List<float>();


                                            foreach (var tokenTiming in phraseEdge.words.tokenTimings)
                                            {
                                                timeStart.Add(float.Parse(tokenTiming.timeStart));
                                                timeEnd.Add(float.Parse(tokenTiming.timeStart));
                                            }

                                            data.tokensTimeStart.Add(timeStart);
                                            data.tokensTimeEnd.Add(timeEnd);

                                        }
                                    }
                                }
                                _newData.Add(data);
                            }
                            else
                            {
                                Debug.LogError("Whoops! Seems like we're missing a video url in our database");
                            }
                            
                        }
                    }
                }

            }

        }
        else
        {
            Debug.LogError("Error parsing response data");

        }
    }

    // Converts thte file type of the videos to be compatible with Unity
    private string m3u8_to_mpd(string url)
    {
        if (url.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
        {
            // Replace the ".m3u8" extension with ".mpd"
            return url.Substring(0, url.Length - 5) + ".mpd";
        }
        else
        {
            // Return the URL unchanged if it does not end with ".m3u8"
            return url;
        }
    }

    // This updates the size of the container so the user can scroll farther
    private void updateContentSize()
    {
        
            // Creates a content Area large enough to space all of the video objects received from the server
            RectTransform item = content.GetChild(0) as RectTransform;

            lengthOfOneItem = item.sizeDelta.x;

            float totalLengthOfItems = lengthOfOneItem * content.childCount;

            content.sizeDelta = new Vector2(totalLengthOfItems, 0);
        
    }



    /* THIS MANAGES THE SLIDE SHOW SNAPPING FEATURE OF THE CONTENT */

    // Declare needed variables
    RectTransform content;
    Vector2 currentPosition;
    float lengthOfOneItem;
    int currentPanel = 1;
    public float snapSpeed = 10f;

    int currentVideoFromBeginning = 0;

    private Vector2 targetPosition;
    private bool isSnapping;

    private void Update()
    {

        // If the state is ever isSnapping, then it will play an animation and set its new position
        if (isSnapping)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);
            if (Vector2.Distance(content.anchoredPosition, targetPosition) < 0.1f)
            {
                content.anchoredPosition = targetPosition;
                isSnapping = false;
                currentPosition = content.anchoredPosition;
                currentVideoFromBeginning = (int)Mathf.Abs((currentPosition.x - lengthOfOneItem) / lengthOfOneItem);
                ChangeSubtitles();
                PlayCurrentVideo();
            }
            
        }


    }

    // If the user dragged the slide to the left, move the slide show right, else move it back
    // There is an event trigger on the screen object that detects when the screen is dragged
    public void ChangeSlide()
    {
        //Debug.Log("Snap to Position!");

        // If the position is greater than the current position swipe right (less than end of content)
        // The greater than or less than signs are reversed because when you move left it is negative
        if (content.anchoredPosition.x < currentPosition.x && currentPosition.x > -1 * (lengthOfOneItem * content.childCount) && currentPanel < content.childCount)
        {
            targetPosition.x = currentPosition.x - lengthOfOneItem;
            PauseCurrentVideo();
            currentPanel += 1;

        }
        else if (content.anchoredPosition.x > currentPosition.x && currentPosition.x < lengthOfOneItem && currentPanel > 1)
        {
            targetPosition.x = currentPosition.x + lengthOfOneItem;
            PauseCurrentVideo();
            currentPanel -= 1;
        }

        // If the position is less than the current position swipe left (greater than the beginning of the content)
            

        //Debug.Log($"Target Position is: {targetPosition}");
        isSnapping = true;
        
    }

    private void ChangeSubtitles()
    {
        VideoPanelData data = _currentData[currentPanel - 1];
        subtitles.setData(data.mediaPlayer, data.sceneTimeStart, data.sceneTimeEnd, data.tokens.ToArray(), data.phrasesTimeStart.ToArray(), data.phrasesTimeEnd.ToArray(), data.tokensTimeStart.ToArray(), data.tokensTimeEnd.ToArray(), data.chapterTimeStart);
    }

    RectTransform video;
    RectTransform screenTransform;
    RectTransform videoPlayerTransform;
    VideoTimeScrubControl videoTimeScrubControlScript;
    public void PauseCurrentVideo()
    {
        video = content.GetChild(currentPanel - 1) as RectTransform;
        screenTransform = video.Find("Screen") as RectTransform;
        videoPlayerTransform = screenTransform.Find("Video Player") as RectTransform;
        videoTimeScrubControlScript = videoPlayerTransform.GetComponent<VideoTimeScrubControl>();
        videoTimeScrubControlScript.Initialize();
        videoTimeScrubControlScript.VideoStop();
        videoTimeScrubControlScript.m_ButtonPlayOrPause.SetActive(false);
    }

    public void PlayCurrentVideo()
    {
        video = content.GetChild(currentPanel - 1) as RectTransform;
        screenTransform = video.Find("Screen") as RectTransform;
        videoPlayerTransform = screenTransform.Find("Video Player") as RectTransform;
        videoTimeScrubControlScript = videoPlayerTransform.GetComponent<VideoTimeScrubControl>();
        videoTimeScrubControlScript.Initialize();
        videoTimeScrubControlScript.VideoPlay();

    }

    // Helper Functions
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

