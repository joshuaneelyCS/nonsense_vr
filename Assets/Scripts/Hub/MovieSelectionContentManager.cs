using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MovieSelectionContentManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_FontAsset titleFontAsset;

    private const string QUERY_GET_VIDEO_SCREEN_CONTENT = "{\"query\" : \"query { titleConnection(languageCode: es, licensedOnly: true, hasPublishedScenes: true) { edges { cursor movie: node { ... on TitleInterface { title posterImageUrl id summary } } } } }\" }";
    private Dictionary <string, List<MovieHubPoster>> _movieSections = new Dictionary<string, List<MovieHubPoster>> { 
        {"New Releases", new List<MovieHubPoster>()},
        {"Recently Watched", new List<MovieHubPoster>()},
        {"Most Popular", new List<MovieHubPoster>()}
    };

    private void BuildRows()
    {
        // Start building the rows
        foreach (var key in _movieSections.Keys)
        {
            BuildRow(key);
        }
    }
    private void BuildRow(string header)
    {
        GameObject row = BuildRowContainer();
        BuildRowTitle(row, header);
        BuildRowHorizontalScrollView(row, header);
    }

    private GameObject BuildRowContainer()
    {
        // Creates the object row
        GameObject row = new GameObject("Row");
        setAsChild(row);

        // Adds a Rect Transform object
        RectTransform rect = row.AddComponent<RectTransform>();
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(366, 240), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));
        
        // Adds a vertical layout group to organize the content
        VerticalLayoutGroup rowVLG = row.AddComponent<VerticalLayoutGroup>();
        rowVLG.childAlignment = TextAnchor.UpperLeft;
        rowVLG.childControlHeight = false; rowVLG.childControlWidth = false;
        rowVLG.childScaleWidth = true; rowVLG.childScaleHeight = true;
        rowVLG.childForceExpandWidth = false; rowVLG.childForceExpandHeight = false;

        return row;
    }

    private void BuildRowTitle(GameObject parent, string header)
    {
        // Adds the title game object
        GameObject rowTitle = new GameObject("Title");
        setAsChild(rowTitle, parent);

        // Adds a Rect Transform object
        RectTransform rect = rowTitle.AddComponent<RectTransform>();
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(300, 60), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector3(0.6f, 1, 1));

        // Adds the text component for the title
        TextMeshProUGUI rowTitleTextMesh = rowTitle.AddComponent<TextMeshProUGUI>();
        rowTitleTextMesh.font = titleFontAsset;
        rowTitleTextMesh.fontStyle = FontStyles.Bold;
        rowTitleTextMesh.text = header;
        rowTitleTextMesh.fontSize = 26;
        rowTitleTextMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
    }

    private void BuildRowHorizontalScrollView(GameObject parent, string key)
    {
        // Creates the object row
        GameObject scrollView = new GameObject("Scroll View");
        setAsChild(scrollView, parent);

        // Adds a Rect Transform object
        RectTransform rect = scrollView.AddComponent<RectTransform>();
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(366, 180), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));

        // Creates the Viewport Object
        var (viewport, content) = BuildViewport(scrollView, key);

        // Adds the Scroll Rect Component
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewport;
        scrollRect.vertical = false;


    }
    private (RectTransform viewport, RectTransform content) BuildViewport(GameObject parent, string key)
    {
        // Creates the object row
        GameObject viewport = new GameObject("Viewport");
        setAsChild(viewport, parent);

        // Adds a Rect Transform object
        RectTransform rect = viewport.AddComponent<RectTransform>();
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector3(1, 1, 1));

        RectTransform content = buildContent(viewport, key);

        return (rect, content);
    }

    private RectTransform buildContent(GameObject parent, string key)
    {
        // Creates the object row
        GameObject content = new GameObject("Content");
        setAsChild(content, parent);

        // Adds a Rect Transform object
        RectTransform rect = content.AddComponent<RectTransform>();
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(546, 180), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector3(1, 1, 1));

        // Create horizontal Layout Group
        HorizontalLayoutGroup contentHZL = content.AddComponent<HorizontalLayoutGroup>();
        contentHZL.padding.right = 10;
        contentHZL.spacing = 10;
        contentHZL.childAlignment = TextAnchor.MiddleLeft;
        contentHZL.childControlHeight = false; contentHZL.childControlWidth = false;
        contentHZL.childScaleWidth = true; contentHZL.childScaleHeight = true;
        contentHZL.childForceExpandWidth = false; contentHZL.childForceExpandHeight = false;

        // For each movie, make a card
        foreach (var movie in _movieSections[key])
        {
            makePortraitCard(content, movie);
        }

        // Create the content container's size
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(findContentWidthToFit(rect, contentHZL), 180), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector3(1, 1, 1));

        return rect;
    }

    private float findContentWidthToFit(RectTransform content, HorizontalLayoutGroup horizontalLayoutGroup)
    {
        if (content != null && content.GetChild(0) != null)
        {
            // Creates a content Area large enough to space all of the video objects received from the server
            RectTransform item = content.GetChild(0) as RectTransform;

            // Calculates padding from the horizontal layout (how far apart are they)
            float lengthOfOneItem = item.sizeDelta.x + horizontalLayoutGroup.spacing;

            float totalLengthOfItems = horizontalLayoutGroup.padding.left + (lengthOfOneItem * content.childCount);

            return totalLengthOfItems;
        }
        return 0;
    }

    private void makePortraitCard(GameObject parent, MovieHubPoster movie)
    {
        // Creates the object row
        GameObject content = new GameObject($"{movie.title}");
        setAsChild(content, parent);

        RectTransform rect = content.AddComponent<RectTransform>();
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(75, 180), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));

        RawImage rawImage = content.AddComponent<RawImage>();
        rawImage.color = Color.white;

        LoadImageFromUrl LoadImageScript = FindObjectOfType<LoadImageFromUrl>();

        // NOTICE: The image is resized in the ImageFromUrl script
        if (LoadImageScript != null)
        {
            StartCoroutine(LoadImageScript.LoadImage($"https://files.wordscenes.com/{movie.posterImageUrl}", rawImage, () => Continue(rect, content, movie)));
        }
        else
        {
            Debug.LogError("RawImageManager not found in the scene.");
        }

    }

    private void Continue(RectTransform rect, GameObject poster, MovieHubPoster movie)
    {
        GameObject hitBox = HandleImageAndDetection(rect, poster);

        // Stores the image texture in the movie object
        movie.renderedPosterTexture = poster.GetComponent<RawImage>().texture;

        // Stores the movie in the object for when it is clicked
        MovieComponent movieComponent = poster.AddComponent<MovieComponent>();
        movieComponent.myMovie = movie;

        CreateEventClicked(hitBox, movieComponent);
    }

    private GameObject HandleImageAndDetection(RectTransform rect, GameObject content)
    {
        
        setRectSettings(rect, new Vector3(0, 0, 0), new Vector2(75, 180), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));
        rect.rotation = Quaternion.Euler(180, 0, 0);

        // This deals with hit detection. Without it, the scroll won't scroll
        GameObject hitBox = new GameObject("Hit Box");
        setAsChild(hitBox, content);

        RectTransform hitBoxRect = hitBox.AddComponent<RectTransform>();
        setRectSettings(hitBoxRect, new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector3(1, 1, 1));
        rect.rotation = Quaternion.Euler(180, 0, 0);

        // Create the image
        Image hitBoxImage = hitBox.AddComponent<Image>();
        Color transparent = Color.white;
        transparent.a = 0;
        hitBoxImage.color = transparent;

        return hitBox;
    }

    // When this card is clicked, it will call the movieComponent script to show the description screen with the proper data
    private void CreateEventClicked(GameObject hitBox, MovieComponent movieComponent)
    {
        PosterClicked clickScript = hitBox.AddComponent<PosterClicked>();
        clickScript.movieComponent = movieComponent;
    }

    private void LoadContent()
    {
        GraphQLFetcher fetchScript = FindObjectOfType<GraphQLFetcher>();
        StartCoroutine(FetchAndSortData(fetchScript));
    }

    private IEnumerator FetchAndSortData(GraphQLFetcher fetchScript)
    {
        string query = QUERY_GET_VIDEO_SCREEN_CONTENT;

        // Start fetching data
        yield return StartCoroutine(fetchScript.FetchData(query));

        // When the data is fetched, set its contents to a local variable
        string fetchedData = fetchScript.returnFetchedData();

        ParseVideoDataAndBuild(fetchedData);

    }

    private void ParseVideoDataAndBuild(string jsonResponse)
    {
        // Takes all the fetched data and puts it into a usable object

        // Parse the JSON response
        var response = JsonConvert.DeserializeObject<GraphQLResponse>(jsonResponse);

        string prevCursor = "";
        // Access the data
        if (response != null && response.data != null && response.data.titleConnection != null)
        {
            foreach (var edge in response.data.titleConnection.edges)
            {
                if (edge.movie != null)
                {
                    // Parses the data into movie objects
                    MovieHubPoster movie = new MovieHubPoster();
                    movie.title = edge.movie.title;
                    movie.posterImageUrl = edge.movie.posterImageUrl;
                    movie.id = edge.movie.id;
                    if (edge.movie.summary != null)
                    {
                        movie.summary = edge.movie.summary.Trim(' ', '\n', '\r', '\t');
                    }
                    movie.cursor = prevCursor;
                    prevCursor = edge.cursor;
                    SortMovie(movie);
                }
            }

            // Starts building the rows when all of the data is parsed
            BuildRows();
        }
        else
        {
            Debug.LogError("Could not parse response");
        }
    }

    // This is a demo sorting function. It randomly sorts them. This can be tailored to the user
    int counter = 0;
    private void SortMovie(MovieHubPoster movie)
    {
        if (counter == 0)
        {
            _movieSections["New Releases"].Add(movie);
        } 
        else if (counter == 1)
        {
            _movieSections["Recently Watched"].Add(movie);
        }
        else
        {
            _movieSections["Most Popular"].Add(movie);
            counter = -1;
        }
        counter++;
    }

    void Start()
    {
       LoadContent();
    }

    // Update is called once per frame
    void Update()
    {
        
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
