using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class UpdateDescriptionScrollView : MonoBehaviour
{

    public Sprite roundedBorderSprite;
    public TMP_FontAsset panelFont;
    public Material blurMaterial;
    public Color darkPanelColor;
    const string NONSENSE_YELLOW = "#ffc702";
    const string DARK_GRAY = "#605c5c";
    const string DARK_YELLOW = "#c89c04";
    public Quaternion Rotation;
    

    // Start is called before the first frame update
    void Start()
    {
        //hideObjects();
        //test();
        displayInformation();
    }

    // Update is called once per frame
    void Update()
    {

    }

    List<List<string>> retrievedContent = new List<List<string>>{

    new List<string> {"<b>Context</b>", "<b>Empezaba</b>", "The Spanish word for <b>beginning</b> or <b>starting</b>. In this context, it means that something was starting to happen"},
    new List<string> { "<b>Word</b>", "<b>empezar</b>", "to start, begin, to get started\nto start to\nto start by;to start with\nto lead with, to open with\nto be started\n" },
    new List<string> { "<b>Grammar<b>", "Why does <b>empezar</b> change to <b>Empezaba</b>?", "Because the speaker is saying that someone (or something) <b>was doing</b> this or <b>used to do</b> it. They could also be saying <b>I</b> was doing it, depending on the context." }

    };

    public void displayInformation(/*This is will take in some argument in the future*/) 
    {
        RectTransform myTransform = GetComponent<RectTransform>();
        myTransform.sizeDelta = new Vector2(190 * retrievedContent.Count, 1320);
        
        // creates the first screen which is black
        createNewPanel(retrievedContent[0], "Gray");

        for (int i = 1; i < retrievedContent.Count; i++) {

            // Creates the rest of the panels
            createNewPanel(retrievedContent[i], "Yellow");

        }
    }

    void setAsChild(GameObject child, GameObject parent = null)
    {
        if (parent != null)
        {
            child.transform.SetParent(parent.transform);
            return;
        }
        child.transform.SetParent(transform);

    }
    void createNewPanel(List<string> content, string panelColor)
    {
        // COLOR SETTINGS

        Color textColor = Color.white;
        Color UIColor = Color.black;
        Color backgroundColor = Color.black;

        // Settings for gray panel
        if (panelColor == "Gray")
        {
            
            textColor = Color.white;
            /*
            backgroundColor = Color.black;
            backgroundColor.a = 200 / 255f; */
            backgroundColor = darkPanelColor;
            UnityEngine.ColorUtility.TryParseHtmlString(DARK_GRAY, out UIColor);
            UIColor.a = 169 / 255f;
            

        } else if (panelColor == "Yellow")
        {
            textColor = Color.black;
            UnityEngine.ColorUtility.TryParseHtmlString(NONSENSE_YELLOW, out backgroundColor);
            UnityEngine.ColorUtility.TryParseHtmlString(DARK_YELLOW, out UIColor);

        }

        // Creates a new Canvas GameObject
        GameObject canvasObject = new GameObject("NewCanvas");

        // Add a Canvas component to the GameObject and changes rendering to World Space
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Add a GraphicRaycaster component to the Canvas GameObject
        canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Set the Canvas as a child of the current GameObject (self)
        setAsChild(canvasObject);

        // Set the Canvas GameObject's local position, rotation, and scale
        canvasObject.transform.localPosition = Vector3.zero; // Centered
        canvasObject.transform.localRotation = Quaternion.identity; // No rotation
        canvasObject.transform.localScale = new Vector3(1, 1, 1); // Adjust scale as needed

        // Set the Canvas size to 1000x1320 pixels
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000, 1320); // Width and Height of the Canvas

        GameObject canvasBackground = new GameObject("Color and Style");
        setAsChild(canvasBackground, canvasObject);
        RectTransform canvasBackgroundRect = canvasBackground.AddComponent<RectTransform>();
        canvasBackgroundRect.anchorMin = Vector2.zero;
        canvasBackgroundRect.anchorMax = new Vector2(1,1);
        canvasBackgroundRect.offsetMin = new Vector2(0, 0); // Bottom-left
        canvasBackgroundRect.offsetMax = new Vector2(1, 1); // Top-Right
        canvasBackgroundRect.anchoredPosition3D = new Vector3(0, 0, 0);
        canvasBackgroundRect.rotation = Rotation;
        canvasBackgroundRect.localScale = new Vector3(1, 1, 1);

        // Add an Image component to the Image GameObject
        Image panelStyle = canvasBackground.AddComponent<Image>();

        // Assign the source sprite to the Image component
        if (roundedBorderSprite != null)
        {
            panelStyle.sprite = roundedBorderSprite;
        }
        /*
        if (blurMaterial != null && panelColor == "Gray")
        {
            panelStyle.material = blurMaterial;
            panelStyle.useSpriteMesh = true;
        }
        */
        // Sets the panel's Color
        panelStyle.color = backgroundColor;

        canvasBackground.AddComponent<Canvas>();

        /* CREATE THE VIEW TO DETERMINE WHAT WILL BE VISIBLE */

        GameObject canvasView = new GameObject("View");
        setAsChild(canvasView, canvasObject);
        RectTransform canvasViewRect = canvasView.AddComponent<RectTransform>();
        canvasViewRect.anchorMin = new Vector2(0, 0);
        canvasViewRect.anchorMax = new Vector2(1, 1);
        canvasViewRect.pivot = new Vector2(0.5f, .5f);
        canvasViewRect.offsetMin = new Vector2(0, 0); // Bottom-left
        canvasViewRect.offsetMax = new Vector2(0, -150);
        canvasViewRect.anchoredPosition3D = new Vector3(0, 0, 0);
        canvasViewRect.rotation = Rotation;
        canvasViewRect.localScale = new Vector3(1, 1, 1);
        
        // This is to counteract the offset of the view being 150 pixels shorter
        canvasViewRect.anchoredPosition = new Vector3(0, 75);

        // This mask hides the objects outside of its view
        canvasView.AddComponent<RectMask2D>();


        /* CREATE VERTICAL LAYOUT GROUP FOR UI ITEMS */

        GameObject UIcontent = new GameObject("Content");
        setAsChild(UIcontent, canvasView);
        RectTransform UIcontentRect = UIcontent.AddComponent<RectTransform>(); 
        UIcontentRect.offsetMin = new Vector2(0, 0); // Bottom-left
        UIcontentRect.offsetMax = new Vector2(0, 1300); // Top-Right
        UIcontentRect.anchorMin = new Vector2(0, 1);
        UIcontentRect.anchorMax = new Vector2(1, 1);
        UIcontentRect.pivot = new Vector2(0, 1);
        UIcontentRect.rotation = Rotation;
        UIcontentRect.anchoredPosition3D = new Vector3(0, 0, 0);
        UIcontentRect.localScale = new Vector3 (1, 1, 1);

        VerticalLayoutGroup verticalLayout = UIcontent.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding.left = 60;
        verticalLayout.padding.right = 60;
        verticalLayout.padding.top = 80;
        verticalLayout.padding.bottom = 80;
        verticalLayout.spacing = 40;
        verticalLayout.childAlignment = TextAnchor.UpperLeft;
        verticalLayout.childControlHeight = false; verticalLayout.childControlWidth = false;
        verticalLayout.childScaleWidth = true; verticalLayout.childScaleHeight = true;
        verticalLayout.childForceExpandWidth = false; verticalLayout.childForceExpandHeight = false;

        /* CREATE THE HEADER */


        // Create the content that will be on each screen
        GameObject header = new GameObject("Header");
        setAsChild(header, UIcontent);

        // Sets up the boundry of the header object
        RectTransform headerBoundry = header.AddComponent<RectTransform>();
        headerBoundry.anchorMin = new Vector2(0, 1);
        headerBoundry.anchorMax = new Vector2(0, 1);
        headerBoundry.pivot = new Vector2(0, 1);
        headerBoundry.rotation = Rotation;
        headerBoundry.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        headerBoundry.anchoredPosition3D = new Vector3(60, -85, 0);

        // -- TODO -- more dynamic sizing that will pad it to word instead of hard coding
        int headerSize = 240;
        if (content[0] == "Word")
        {
            headerSize = 190;
        }

        // Sets the size depending on how large the word is
        headerBoundry.sizeDelta = new Vector2(headerSize, 100);

        // Create the background of the header
        GameObject headerBackground = new GameObject("Color and Style");
        setAsChild(headerBackground, header);
        RectTransform headerBackgroundRect = headerBackground.AddComponent<RectTransform>();

        // This background will fill to the size of the headerBoundry
        headerBackgroundRect.anchorMin = new Vector2(0, 0);
        headerBackgroundRect.anchorMax = new Vector2(1, 1);
        headerBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
        headerBackgroundRect.localScale = new Vector3(1, 1, 1);
        headerBackgroundRect.offsetMin = new Vector2(0, 0); // Bottom-left
        headerBackgroundRect.offsetMax = new Vector2(0, 0); // Top-right
        headerBackgroundRect.rotation = Rotation;
        headerBackgroundRect.anchoredPosition3D = new Vector3(0, 0, 0);

        // This is the background color of the header
        Image headerStyleImage = headerBackground.AddComponent<Image>();
        headerStyleImage.type = Image.Type.Sliced;
        headerStyleImage.fillCenter = true;

        // Gives the header a boarder if there is one
        if (roundedBorderSprite != null)
        {
            headerStyleImage.sprite = roundedBorderSprite;
        }

        headerStyleImage.color = UIColor;

        // Configuring header dimensions
        GameObject headerText = new GameObject("Text");
        setAsChild(headerText, header);
        RectTransform headerTextRect = headerText.AddComponent<RectTransform>();
        headerTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        headerTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        headerTextRect.pivot = new Vector2(0.5f, 0.5f);
        headerTextRect.rotation = Rotation;
        headerTextRect.localScale = new Vector3(0.867f, 1.114f, 1);
        headerTextRect.anchoredPosition3D = new Vector3(0, 0, 0);

        headerTextRect.sizeDelta = new Vector2((content[0].Length * 34f), 40);

        // Adding the header text
        TextMeshProUGUI headerTextMesh = headerText.AddComponent<TextMeshProUGUI>();

        // The header text is what is recieved from the server
        headerTextMesh.text = $"{content[0].ToUpper()}";
        headerTextMesh.font = panelFont;
        headerTextMesh.fontSize = 36;
        headerTextMesh.alignment = TextAlignmentOptions.Center;
        headerTextMesh.color = textColor;



        /* BIG FONT TEXT */

        // Configuring dimensions
        GameObject bigText = new GameObject("bigText");
        setAsChild(bigText, UIcontent);
        RectTransform bigTextRect = bigText.AddComponent<RectTransform>();
        bigTextRect.anchorMin = new Vector2(0,1);
        bigTextRect.anchorMax = new Vector2(0, 1);
        bigTextRect.pivot = new Vector2(0, 1);
        bigTextRect.rotation = Rotation;
        bigTextRect.localScale = new Vector3(0.867f, 1.114f, 1);
        bigTextRect.anchoredPosition3D = new Vector3(60, -300, 0);

        // Adjusts the size of the rect depending on the length of the word
        bigTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 975);


        // Adding the text 
        TextMeshProUGUI bigTextMesh = bigText.AddComponent<TextMeshProUGUI>();

        // The text is received from the server // Needs to be given a bold letter
        bigTextMesh.text = content[1];
        bigTextMesh.font = panelFont;
        bigTextMesh.fontSize = 120;
        bigTextMesh.color = textColor;
        bigTextMesh.ForceMeshUpdate();
        float bigTextpreferredHeight = bigTextMesh.preferredHeight;

        bigTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bigTextpreferredHeight);


        /* DESCRIPTION TEXT (MAIN TEXT) */

        // Configuring dimensions
        GameObject mainText = new GameObject("mainText");
        setAsChild(mainText, UIcontent);
        RectTransform mainTextRect = mainText.AddComponent<RectTransform>();
        mainTextRect.anchorMin = new Vector2(0, 1);
        mainTextRect.anchorMax = new Vector2(0, 1);
        mainTextRect.pivot = new Vector2(0, 1);
        mainTextRect.rotation = Rotation;
        mainTextRect.localScale = new Vector3(0.867f, 1.114f, 1);
        mainTextRect.anchoredPosition3D = new Vector3(60, -510, 0);

        // Adjusts the size of the rect depending on the length of the text
        mainTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 975);

        // Adding the text 
        TextMeshProUGUI mainTextMesh = mainText.AddComponent<TextMeshProUGUI>();

        // The text is received from the server
        mainTextMesh.text = content[2];
        mainTextMesh.font = panelFont;
        mainTextMesh.fontSize = 80;
        mainTextMesh.color = textColor;
        mainTextMesh.ForceMeshUpdate();
        float mainTextpreferredHeight = mainTextMesh.preferredHeight;

        mainTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mainTextpreferredHeight);



        /* INTERACTION between user and panel */

        float totalHeight = 0f;

        // This padding corrisponds with the padding in main contents vertical layout
        float verticalLayoutPadding = 40;

        totalHeight += 160;
        
        // Gets the total height of all of the contents
        foreach (RectTransform child in UIcontentRect)
        {
            totalHeight += child.rect.height + verticalLayoutPadding;
        }

        if (totalHeight >= (UIcontentRect.sizeDelta.y - 150))
        {
            interactionWithPanels animateOnHover = canvas.AddComponent<interactionWithPanels>();
            animateOnHover.targetHeight = totalHeight + 60 + 150;
            animateOnHover.duration = 0.1f;
        }

    }

    void hideObjects()
    {
        // Get the renderer component
        Renderer rend = GetComponent<Renderer>();

        if (rend != null)
        {
            rend.enabled = false;
        }
        else
        {
            Debug.LogWarning("No Renderer component found on this GameObject");
        }
    }
}


