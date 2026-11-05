using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Makes sure the report card always has an Image, since that Image shows the card art
[RequireComponent(typeof(Image))]
public class ReportCard : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     * ~ Every position is in art pixels on ReportCard.png (50 x 70), measured from the top left corner
     */
    [Header("Faces")]
    // Shown for a customer who got their package while Happy
    [SerializeField] private Sprite happyFace;
    // Shown for a customer who got their package while Impatient
    [SerializeField] private Sprite impatientFace;
    // Shown for a customer who got their package while Angry
    [SerializeField] private Sprite angryFace;
    // Shown for a customer who left, or never got their package (if empty, the Angry face is shown faded instead)
    [SerializeField] private Sprite goneFace;
    // How the Angry face is faded when it stands in for the Gone face
    [SerializeField] private Color goneFadedColor = new Color(1f, 1f, 1f, 0.35f);
    // About how tall each face is drawn, in card pixels (the face is drawn at the nearest whole number size to this)
    [SerializeField] private float faceHeight = 5f;

    [Header("Text")]
    // The words in the header box at the top of the card
    [SerializeField] private string headerText = "Report Card";
    // The color of the writing on the card (matches the card's outline)
    [SerializeField] private Color inkColor = new Color32(91, 80, 74, 255);

    [Header("Grade Stamp Colors")]
    // Each grade is stamped in bold in its own color
    [SerializeField] private Color gradeS = new Color32(176, 84, 214, 255);
    [SerializeField] private Color gradeA = new Color32(52, 168, 83, 255);
    [SerializeField] private Color gradeB = new Color32(66, 133, 214, 255);
    [SerializeField] private Color gradeC = new Color32(222, 170, 32, 255);
    [SerializeField] private Color gradeD = new Color32(230, 112, 40, 255);
    [SerializeField] private Color gradeF = new Color32(204, 48, 48, 255);

    [Header("Layout (art pixels)")]
    // The y of each writing line, top to bottom (one customer sits on each)
    [SerializeField] private int[] lineRows = { 19, 25, 31, 37, 43 };
    // How tall the space above each line is
    [SerializeField] private int rowHeight = 5;
    // Where the face starts on each line
    [SerializeField] private int faceX = 6;
    // Where the customer's name starts and ends on each line
    [SerializeField] private int nameLeft = 12;
    [SerializeField] private int nameRight = 43;
    // The inside of the header box (left, top, width, height)
    [SerializeField] private RectInt headerBox = new RectInt(6, 5, 38, 6);
    // The inside of the stamp box in the bottom right (left, top, width, height)
    [SerializeField] private RectInt stampBox = new RectInt(33, 53, 10, 10);
    // The space above the signature line in the bottom left, where the score goes (left, top, width, height)
    [SerializeField] private RectInt scoreLine = new RectInt(7, 57, 23, 6);

    [Header("Fit To Screen")]
    // The biggest size the card can be drawn at (10 screen pixels per art pixel = 500 x 700)
    [SerializeField] private int maxScale = 10;
    // The smallest size the card can be drawn at
    [SerializeField] private int minScale = 4;
    // Screen space kept free above the card
    [SerializeField] private float topMargin = 30f;
    // Screen space kept free below the card for the Restart and Quit buttons
    [SerializeField] private float bottomMargin = 120f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The card's RectTransform, which is sized and moved to fit the screen
    private RectTransform rect;
    // The Canvas the card sits on, used to read the screen size
    private Canvas canvas;
    // The card art's size in art pixels
    private Vector2 artSize;
    // The size the card was last fitted at, so it only refits when the screen changes
    private float fittedHeight;

    // Made in code so nothing needs to be laid out by hand in the scene
    private TMP_Text header;
    private TMP_Text stamp;
    private TMP_Text score;
    private readonly List<Image> faces = new List<Image>();
    // The top of each face's row, in art pixels
    private readonly List<int> faceTops = new List<int>();
    private readonly List<TMP_Text> names = new List<TMP_Text>();
    // Remembers where each piece goes, in art pixels, so they can all be moved when the card is rescaled
    private readonly List<KeyValuePair<RectTransform, RectInt>> placements = new List<KeyValuePair<RectTransform, RectInt>>();

    /* Void Awake Method:
     * ~ Runs once when the card is loaded, before anything else
     * ~ Builds the card if Show hasn't already built it
     */
    private void Awake()
    {
        if (header == null)
        {
            Build();
        }
    }

    /* Void Build Method:
     * ~ Makes the header, the five customer rows, the stamp and the score, all as children of the card
     */
    private void Build()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        Image card = GetComponent<Image>();
        artSize = card.sprite != null ? card.sprite.rect.size : new Vector2(50f, 70f);

        // Pins the card to the top middle of the screen
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);

        header = MakeText("Header", headerBox, inkColor, TextAlignmentOptions.Center);
        header.text = headerText;

        for (int i = 0; i < lineRows.Length; i++)
        {
            int top = lineRows[i] - rowHeight;

            Image face = new GameObject("Face" + (i + 1), typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            face.transform.SetParent(transform, false);
            face.raycastTarget = false;
            face.preserveAspect = true;
            face.rectTransform.anchorMin = new Vector2(0f, 1f);
            face.rectTransform.anchorMax = new Vector2(0f, 1f);
            face.rectTransform.pivot = new Vector2(0f, 0.5f);
            faces.Add(face);
            faceTops.Add(top);

            names.Add(MakeText("Name" + (i + 1), new RectInt(nameLeft, top, nameRight - nameLeft, rowHeight), inkColor, TextAlignmentOptions.Left));
        }

        stamp = MakeText("Stamp", stampBox, gradeF, TextAlignmentOptions.Center);
        stamp.fontStyle = FontStyles.Bold;
        score = MakeText("Score", scoreLine, inkColor, TextAlignmentOptions.Left);

        FitToScreen();
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Refits the card if the window was resized
     */
    private void Update()
    {
        if (canvas != null && !Mathf.Approximately(fittedHeight, ScreenHeight()))
        {
            FitToScreen();
        }
    }

    /* Public Show Method:
     * ~ Called by GameOverPanel when the shift ends
     * ~ Writes each customer's name with the face they ended the shift with, then stamps the grade and writes the score
     */
    public void Show(OrderManager orderManager, string letterGrade, int finalScore)
    {
        // The card may still be hidden and not built yet when the shift ends
        if (header == null)
        {
            Build();
        }

        List<Order> orders = orderManager != null ? orderManager.GetOrders() : new List<Order>();

        for (int i = 0; i < faces.Count; i++)
        {
            // More lines than customers: the extra lines stay blank
            bool used = i < orders.Count;
            names[i].text = used ? orders[i].customerName : "";

            Sprite faceSprite = used ? GetFace(orderManager, orders[i]) : null;
            faces[i].color = Color.white;

            // No Gone face drawn yet, so a customer who left shows a faded Angry face
            if (used && faceSprite == null && !orderManager.IsDelivered(orders[i]))
            {
                faceSprite = angryFace;
                faces[i].color = goneFadedColor;
            }

            faces[i].sprite = faceSprite;
            faces[i].enabled = faceSprite != null;
        }

        stamp.text = letterGrade;
        stamp.color = GetGradeColor(letterGrade);
        score.text = "Score " + finalScore;

        // Sizes the faces now that they have their art
        FitToScreen();
    }

    /* Color GetGradeColor Method:
     * ~ Returns the stamp color for a letter grade
     */
    private Color GetGradeColor(string letterGrade)
    {
        switch (letterGrade)
        {
            case "S":
                return gradeS;
            case "A":
                return gradeA;
            case "B":
                return gradeB;
            case "C":
                return gradeC;
            case "D":
                return gradeD;
            default:
                return gradeF;
        }
    }

    /* Sprite GetFace Method:
     * ~ A delivered customer shows the mood they were in when Carrot arrived
     * ~ Anyone who left or never got their package shows the Gone face
     */
    private Sprite GetFace(OrderManager orderManager, Order order)
    {
        if (!orderManager.IsDelivered(order))
        {
            return goneFace;
        }

        switch (order.mood)
        {
            case CustomerMood.Happy:
                return happyFace;
            case CustomerMood.Impatient:
                return impatientFace;
            case CustomerMood.Angry:
                return angryFace;
            default:
                return goneFace;
        }
    }

    /* TMP_Text MakeText Method:
     * ~ Makes one piece of writing on the card, sized to fit its box without wrapping
     */
    private TMP_Text MakeText(string objectName, RectInt box, Color color, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI text = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(transform, false);
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8f;
        text.raycastTarget = false;
        text.margin = Vector4.zero;
        Place(text.rectTransform, box);
        return text;
    }

    /* Void Place Method:
     * ~ Remembers where a piece goes on the card, in art pixels from the top left
     */
    private void Place(RectTransform piece, RectInt box)
    {
        piece.anchorMin = new Vector2(0f, 1f);
        piece.anchorMax = new Vector2(0f, 1f);
        piece.pivot = new Vector2(0f, 1f);
        placements.Add(new KeyValuePair<RectTransform, RectInt>(piece, box));
    }

    /* Void FitToScreen Method:
     * ~ Draws the card at the biggest whole number scale that fits between the margins, so every art pixel stays square
     * ~ Moves and sizes every piece to match
     */
    private void FitToScreen()
    {
        float height = ScreenHeight();
        fittedHeight = height;

        int scale = Mathf.FloorToInt((height - topMargin - bottomMargin) / artSize.y);
        scale = Mathf.Clamp(scale, minScale, maxScale);

        rect.sizeDelta = artSize * scale;
        rect.anchoredPosition = new Vector2(0f, -topMargin);

        foreach (KeyValuePair<RectTransform, RectInt> placement in placements)
        {
            RectInt box = placement.Value;
            placement.Key.anchoredPosition = new Vector2(box.x * scale, -box.y * scale);
            placement.Key.sizeDelta = new Vector2(box.width * scale, box.height * scale);
        }

        // Each face pixel is drawn at a whole number of screen pixels, as close to faceHeight card pixels tall as it can get,
        // and centered on its row
        for (int i = 0; i < faces.Count; i++)
        {
            Sprite sprite = faces[i].sprite;
            Vector2 spriteSize = sprite != null ? sprite.rect.size : new Vector2(5f, 5f);
            float facePixel = Mathf.Max(1f, Mathf.Round(faceHeight * scale / spriteSize.y));
            RectTransform faceRect = faces[i].rectTransform;
            faceRect.sizeDelta = spriteSize * facePixel;
            faceRect.anchoredPosition = new Vector2(faceX * scale, -(faceTops[i] + rowHeight * 0.5f) * scale);
        }

        // The writing gets bigger with the card, but never taller than its box
        foreach (TMP_Text text in new[] { header, stamp, score })
        {
            text.fontSizeMax = text.rectTransform.sizeDelta.y * 1.2f;
        }
        foreach (TMP_Text text in names)
        {
            text.fontSizeMax = text.rectTransform.sizeDelta.y * 1.2f;
        }
    }

    /* Float ScreenHeight Method:
     * ~ The screen's height in canvas units (the Canvas uses constant pixel size, so this is usually just the screen height)
     */
    private float ScreenHeight()
    {
        if (canvas == null)
        {
            return Screen.height;
        }
        return canvas.pixelRect.height / canvas.scaleFactor;
    }
}
