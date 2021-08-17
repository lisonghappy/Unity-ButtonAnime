using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ButtonAnime : UnityEngine.UI.Selectable,
    IPointerClickHandler,
    ISubmitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Space(20f)]

    [SerializeField,Tooltip("enable button animation")] private bool EnableAnime = true;
    [SerializeField] private bool EnableHoverAnime = true;
    
    [Tooltip("button animation execution time")]
    [ SerializeField] private float duration = 0.1f;
    [Tooltip("button animation scale weights")]
    [SerializeField] private float weight = 0.1f;
    [Tooltip("button animation transition type")]
    [SerializeField] private Ease animeEace = Ease.Linear;


    private Transform trans;
    private bool isPointEnter = false;
    private bool isPointerDown = false;
    
    
    protected override void Start()
    {
        trans = transform;
    }




    public void OnSubmit(BaseEventData eventData)
    {

    }


    public override void OnPointerEnter(PointerEventData eventData)
    {

        base.OnPointerEnter(eventData);

        if (!EnableAnime) return;

        if (!isPointerDown && !EnableHoverAnime) return;

        if (isPointEnter) return;
        trans.DOScale(Vector2.one * (1f - weight), duration).OnComplete(() => {
            isPointEnter = true;
        });
        Debug.LogWarning("--------------Enter---------------");
    }


    public override void OnPointerExit(PointerEventData eventData)
    {

        base.OnPointerExit(eventData);

        if (!EnableAnime) return;

        if (!isPointerDown && !EnableHoverAnime) return;

        if (!isPointEnter) return;
        trans.DOScale(Vector2.one, duration).OnComplete(() => {
            isPointEnter = false;
        });
        Debug.LogWarning("--------------Exit---------------");
    }


    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!EnableAnime) return;

        isPointerDown = true;
        trans.DOScale(Vector2.one * (1f - weight), duration);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!EnableAnime) return;

        trans.DOScale(Vector2.one, duration);
        isPointerDown = false;
    }




    public void OnPointerClick(PointerEventData eventData)
    {

        if (!EnableAnime) return;

        //trans.DOPunchScale(Vector2.one * 0.05f, 0.5f, 0, 0.5f).OnComplete(() => { trans.localScale = Vector3.one; });
        Sequence seq = DOTween.Sequence();
        seq.Append(trans.DOScale(Vector2.one * (1f + weight), duration));
        seq.Append(trans.DOScale(Vector2.one, duration)).SetEase(animeEace);
    }


#if UNITY_EDITOR
    [MenuItem("GameObject/UI/ButtonAnime")]
    static void CreateButtonEx(MenuCommand menuCmd)
    { 
        //create button object
        GameObject btnRoot = new GameObject("Button");
        RectTransform rect = btnRoot.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160f, 30f);
        // add text object
        CreateTextComponent(btnRoot);
         
        btnRoot.AddComponent<CanvasRenderer>();

        // add image
        Image img = btnRoot.AddComponent<Image>();
        img.color = Color.white;
        img.fillCenter = true;
        img.raycastTarget = true;
        img.sprite = findRes<Sprite>("UISprite");
        if (img.sprite != null)
            img.type = Image.Type.Sliced;

        btnRoot.AddComponent<ButtonAnime>();
        btnRoot.GetComponent<Selectable>().image = img;

        // Put it in UI Canvas
        PlaceUIElementRoot(btnRoot, menuCmd);
    }


    #region Create UI Component

    private static void CreateTextComponent(GameObject parent)
    {
        GameObject childText = CreateUIObject("Text", parent);
        Text v = childText.AddComponent<Text>();
        v.text = "Button";
        v.alignment = TextAnchor.MiddleCenter;
        v.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        RectTransform r = childText.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

    }



    private static GameObject CreateUIObject(string name, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        SetParentAndAlign(go, parent);
        return go;
    }

    private static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

        child.transform.SetParent(parent.transform, false);
        SetLayerRecursively(child, parent.layer);
    }
    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }


    public static T findRes<T>(string name) where T : Object
    {
        T[] objs = Resources.FindObjectsOfTypeAll<T>();
        if (objs != null && objs.Length > 0)
        {
            foreach (Object obj in objs)
            {
                if (obj.name == name)
                    return obj as T;
            }
        }
        objs = AssetBundle.FindObjectsOfType<T>();
        if (objs != null && objs.Length > 0)
        {
            foreach (Object obj in objs)
            {
                if (obj.name == name)
                    return obj as T;
            }
        }
        return default(T);
    }

    public static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
    { 
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            parent = GetOrCreateCanvasGameObject();
        }

        string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
        element.name = uniqueName;
        Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
        Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
        GameObjectUtility.SetParentAndAlign(element, parent);
        if (parent != menuCommand.context) // not a context click, so center in sceneview
            SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

        Selection.activeGameObject = element; 
    }

    public static GameObject GetOrCreateCanvasGameObject()
    { 
        GameObject selectedGo = Selection.activeGameObject;

        // Try to find a gameobject that is the selected GO or one if its parents.
        Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        // No canvas in selection or its parents? Then use just any canvas..
        canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        // No canvas in the scene at all? Then create a new one.
        return CreateNewUI(); 
    }

    private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
    {
        // Find the best scene view
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null && SceneView.sceneViews.Count > 0)
            sceneView = SceneView.sceneViews[0] as SceneView;

        // Couldn't find a SceneView. Don't set position.
        if (sceneView == null || sceneView.camera == null)
            return;

        // Create world space Plane from canvas position.
        Vector2 localPlanePosition;
        Camera camera = sceneView.camera;
        Vector3 position = Vector3.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
        {
            // Adjust for canvas pivot
            localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
            localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

            localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
            localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

            // Adjust for anchoring
            position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
            position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

            Vector3 minLocalPosition;
            minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
            minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

            Vector3 maxLocalPosition;
            maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
            maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

            position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
            position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
        }

        itemTransform.anchoredPosition = position;
        itemTransform.localRotation = Quaternion.identity;
        itemTransform.localScale = Vector3.one;
    } 

    public static GameObject CreateNewUI()
    {
        // Root for the UI
        var root = new GameObject("Canvas");
        root.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

         Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

        // if there is no event system add one...
        CreateEventSystem(false, null);
        return root;
    }

    public static void CreateEventSystem(bool select, GameObject parent)
    { 
        var esys = Object.FindObjectOfType<EventSystem>();
        if (esys == null)
        {
            var eventSystem = new GameObject("EventSystem");
            GameObjectUtility.SetParentAndAlign(eventSystem, parent);
            esys = eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
        }

        if (select && esys != null)
        {
            Selection.activeGameObject = esys.gameObject;
        } 
    }

    #endregion

#endif

}
