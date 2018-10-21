/*===============================================================================
Copyright (c) 2016-2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using UnityEngine.Networking;

/// <summary>
/// A custom handler which uses the VuMarkManager.
/// </summary>
public class TRCVuMarkHandler : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES
    // Define the number of persistent child objects of the VuMarkBehaviour. When
    // destroying the instance-specific augmentations, it will start after this value.
    // Persistent Children:
    // 1. Canvas -- displays info about the VuMark
    // 2. LineRenderer -- displays border outline around VuMark
    const int PersistentNumberOfChildren = 2;
    FusionProviderType fusionProviderType = FusionProviderType.OPTIMIZE_IMAGE_TARGETS_AND_VUMARKS;
    VuMarkManager vumarkManager;
    LineRenderer lineRenderer;
    Dictionary<string, Texture2D> productImageTextures;
    Dictionary<string, GameObject> vumarkAugmentationObjects;
    //Dictionary to hold all the descriptions pulled from server
    Dictionary<string, ProductInfo> vumarkProductInfos;

    PanelShowHide nearestVuMarkScreenPanel;
    VuMarkTarget closestVuMark;
    VuMarkTarget currentVuMark;
    #endregion // PRIVATE_MEMBER_VARIABLES

    public Button addToCartBtn;
    public string user_id = "toshi";
    public Sprite addedToCartSprite;
    public Sprite addToCartSprite;

    #region PUBLIC_MEMBERS
    [System.Serializable]
    public class AugmentationObject
    {
        public string vumarkID;
        public GameObject augmentation;
    }
    public AugmentationObject[] augmentationObjects;
    #endregion // PUBLIC_MEMBERS

    GameObject vuMarkUI;

    #region MONOBEHAVIOUR_METHODS
    void Awake()
    {
        VuforiaConfiguration.Instance.Vuforia.MaxSimultaneousImageTargets = 10; // Set to 10 for VuMarks
        VuforiaARController.Instance.RegisterBeforeVuforiaTrackersInitializedCallback(OnBeforeVuforiaTrackerInitialized);
    }

    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);

        this.productImageTextures = new Dictionary<string, Texture2D>();
        this.vumarkAugmentationObjects = new Dictionary<string, GameObject>();
        this.vumarkProductInfos = new Dictionary<string, ProductInfo>();

        Button btn1 = addToCartBtn.GetComponent<Button>();
        btn1.GetComponent<UnityEngine.UI.Image>().sprite = addToCartSprite;
        btn1.onClick.AddListener(addToCart);

        foreach (AugmentationObject obj in this.augmentationObjects)
        {
            this.vumarkAugmentationObjects.Add(obj.vumarkID, obj.augmentation);
        }

        // Hide the initial VuMark Template when the scene starts.
        VuMarkBehaviour vumarkBehaviour = FindObjectOfType<VuMarkBehaviour>();
        if (vumarkBehaviour)
        {
            ToggleRenderers(vumarkBehaviour.gameObject, false);
        }

        this.nearestVuMarkScreenPanel = FindObjectOfType<PanelShowHide>();
    }

    void Update()
    {
        UpdateClosestTarget();
    }

    void OnDestroy()
    {
        VuforiaConfiguration.Instance.Vuforia.MaxSimultaneousImageTargets = 4; // Reset back to 4 when exiting
        VuforiaARController.Instance.UnregisterBeforeVuforiaTrackersInitializedCallback(OnBeforeVuforiaTrackerInitialized);
        // Unregister callbacks from VuMark Manager
        this.vumarkManager.UnregisterVuMarkBehaviourDetectedCallback(OnVuMarkBehaviourDetected);
        this.vumarkManager.UnregisterVuMarkDetectedCallback(OnVuMarkDetected);
        this.vumarkManager.UnregisterVuMarkLostCallback(OnVuMarkLost);
    }

    #endregion // MONOBEHAVIOUR_METHODS

    void OnBeforeVuforiaTrackerInitialized()
    {
        // Set the selected fusion provider mask in the DeviceTrackerARController before it's being used.
        Debug.Log("DeviceTrackerARController.Instance.FusionProvider = " + this.fusionProviderType);
        DeviceTrackerARController.Instance.FusionProvider = this.fusionProviderType;
    }

    void OnVuforiaStarted()
    {
        // register callbacks to VuMark Manager
        this.vumarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        this.vumarkManager.RegisterVuMarkBehaviourDetectedCallback(OnVuMarkBehaviourDetected);
        this.vumarkManager.RegisterVuMarkDetectedCallback(OnVuMarkDetected);
        this.vumarkManager.RegisterVuMarkLostCallback(OnVuMarkLost);
    }

    #region VUMARK_CALLBACK_METHODS

    /// <summary>
    ///  Register a callback which is invoked whenever a VuMark-result is newly detected which was not tracked in the frame before
    /// </summary>
    /// <param name="vumarkBehaviour"></param>
    public void OnVuMarkBehaviourDetected(VuMarkBehaviour vumarkBehaviour)
    {
        Debug.Log("<color=cyan>VuMarkHandler.OnVuMarkBehaviourDetected(): </color>" + vumarkBehaviour.TrackableName);

        GenerateVuMarkBorderOutline(vumarkBehaviour);

        ToggleRenderers(vumarkBehaviour.gameObject, true);

        // Check for existance of previous augmentations and delete before instantiating new ones.
        DestroyChildAugmentationsOfTransform(vumarkBehaviour.transform);

        StartCoroutine(OnVuMarkTargetAvailable(vumarkBehaviour));
    }

    IEnumerator OnVuMarkTargetAvailable(VuMarkBehaviour vumarkBehaviour)
    {
        // We need to wait until VuMarkTarget is available
        yield return new WaitUntil(() => vumarkBehaviour.VuMarkTarget != null);

        Debug.Log("<color=green>TRCVuMarkHandler.OnVuMarkTargetAvailable() called: </color>" + GetVuMarkId(vumarkBehaviour.VuMarkTarget));

        //TODO: Make a new SetVuMarkInfoForCanvas as IEnumerator
        SetVuMarkInfoForCanvas(vumarkBehaviour);
        SetVuMarkAugmentation(vumarkBehaviour);
        SetVuMarkOpticalSeeThroughConfig(vumarkBehaviour);
    }

    /// <summary>
    /// This method will be called whenever a new VuMark is detected
    /// </summary>
    public void OnVuMarkDetected(VuMarkTarget vumarkTarget)
    {
        Debug.Log("<color=cyan>VuMarkHandler.OnVuMarkDetected(): </color>" + GetVuMarkId(vumarkTarget));

        // Check if this VuMark's ID already has a stored texture. Generate and store one if not.
        if (RetrieveProductInfoForVuMarkTarget(vumarkTarget) == null)
        {
            StartCoroutine(SetProductDescFromServer(vumarkTarget));
        }
    }

    IEnumerator SetProductDescFromServer(VuMarkTarget vuMarkTarget)
    {
        string id = GetVuMarkId(vuMarkTarget);
        string uri = "http://revamp.ap-southeast-1.elasticbeanstalk.com/item/" + id;
        //string uri = "https://revamp-api.herokuapp.com/item/" + id;
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            ProductInfo productInfo = new ProductInfo();
            productInfo = ProductInfo.CreateFromJSON(uwr.downloadHandler.text);
            yield return downloadProductImage(vuMarkTarget, productInfo.url);
            vumarkProductInfos.Add(GetVuMarkId(vuMarkTarget), productInfo);

        }

    }

    /// <summary>
    /// This method will be called whenever a tracked VuMark is lost
    /// </summary>
    public void OnVuMarkLost(VuMarkTarget vumarkTarget)
    {
        Debug.Log("<color=cyan>VuMarkHandler.OnVuMarkLost(): </color>" + GetVuMarkId(vumarkTarget));

        if (vumarkTarget == this.currentVuMark)
            this.nearestVuMarkScreenPanel.ResetShowTrigger();
    }

    #endregion // VUMARK_CALLBACK_METHODS


    #region PRIVATE_METHODS

    string GetVuMarkDataType(VuMarkTarget vumarkTarget)
    {
        switch (vumarkTarget.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return "Bytes";
            case InstanceIdType.STRING:
                return "String";
            case InstanceIdType.NUMERIC:
                return "Numeric";
        }
        return string.Empty;
    }

    string GetVuMarkId(VuMarkTarget vumarkTarget)
    {
        switch (vumarkTarget.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return vumarkTarget.InstanceId.HexStringValue;
            case InstanceIdType.STRING:
                return vumarkTarget.InstanceId.StringValue;
            case InstanceIdType.NUMERIC:
                return vumarkTarget.InstanceId.NumericValue.ToString();
        }
        return string.Empty;
    }

    Sprite GetProductImage(VuMarkTarget vumarkTarget)
    {
        var texture = RetrieveStoredTextureForVuMarkTarget(vumarkTarget);
        if (texture == null)
        {
            Debug.Log("NULL TEXTURE");
            return null;
        }
        
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }

    IEnumerator SetVuMarkInfoForCanvas(VuMarkBehaviour vumarkBehaviour)
    {
        Text canvasText = vumarkBehaviour.gameObject.GetComponentInChildren<Text>();
        UnityEngine.UI.Image canvasImage = vumarkBehaviour.gameObject.GetComponentsInChildren<UnityEngine.UI.Image>()[2];

        Texture2D vumarkInstanceTexture = RetrieveStoredTextureForVuMarkTarget(vumarkBehaviour.VuMarkTarget);
        Rect rect = new Rect(0, 0, vumarkInstanceTexture.width, vumarkInstanceTexture.height);

        yield return SetProductDescFromServer(vumarkBehaviour.VuMarkTarget);
        ProductInfo productInfo = RetrieveProductInfoForVuMarkTarget(vumarkBehaviour.VuMarkTarget);
        if (productInfo.golden)
        {
            canvasText.text =
            "<color=black>GOLDEN TICKET!!!</color>\n" +
                        "<color=black>Item Name: </color>" + "<color=red>" + productInfo.name + "</color>" + "\n" +
                        "<color=black>Price: </color>" + "<color=red>" + string.Format("{0:0.00}", productInfo.price) + "</color>" +
                        "\n\n<color=black>Item Description: </color>" +
                        "<color=red>" + productInfo.description + "</color>" + "\n";
        }
        else
        {
            canvasText.text =
                        "<color=black>Item Name: </color>" + productInfo.name + "\n" +
                        "<color=black>Price: </color>" + string.Format("{0:0.00}", productInfo.price) +
                        "\n\n<color=black>Product Description: </color>" +
                        productInfo.description + "\n" + "\n";
        }
        canvasImage.sprite = Sprite.Create(vumarkInstanceTexture, rect, new Vector2(2.0f, 0.5f));
    }

    void SetVuMarkAugmentation(VuMarkBehaviour vumarkBehaviour)
    {
        GameObject sourceAugmentation = GetValueFromDictionary(this.vumarkAugmentationObjects, GetVuMarkId(vumarkBehaviour.VuMarkTarget));

        if (sourceAugmentation)
        {
            GameObject augmentation = Instantiate(sourceAugmentation);
            augmentation.transform.SetParent(vumarkBehaviour.transform);
            augmentation.transform.localPosition = Vector3.zero;
            augmentation.transform.localScale = Vector3.one;
            augmentation.transform.localEulerAngles = Vector3.zero;
        }
    }

    void SetVuMarkOpticalSeeThroughConfig(VuMarkBehaviour vumarkBehaviour)
    {
        if (VuforiaConfiguration.Instance.DigitalEyewear.SeeThroughConfiguration == DigitalEyewearARController.SeeThroughConfiguration.HoloLens)
        {
            MeshRenderer meshRenderer = vumarkBehaviour.GetComponent<MeshRenderer>();

            // If the VuMark has per instance background info, turn off virtual target so that it doesn't cover modified physical target
            if (vumarkBehaviour.VuMarkTemplate.TrackingFromRuntimeAppearance)
            {
                if (meshRenderer)
                {
                    meshRenderer.enabled = false;
                }
            }
            else
            {
                // If the VuMark background is part of VuMark Template and same per instance, render the virtual target
                if (meshRenderer)
                {
                    meshRenderer.material.mainTexture = RetrieveStoredTextureForVuMarkTarget(vumarkBehaviour.VuMarkTarget);
                }
            }
        }
        else
        {
            MeshRenderer meshRenderer = vumarkBehaviour.GetComponent<MeshRenderer>();

            if (meshRenderer)
            {
                meshRenderer.enabled = false;
            }
        }
    }

    ProductInfo RetrieveProductInfoForVuMarkTarget(VuMarkTarget vumarkTarget)
    {
        return GetValueFromDictionary(this.vumarkProductInfos, GetVuMarkId(vumarkTarget));
    }

    Texture2D RetrieveStoredTextureForVuMarkTarget(VuMarkTarget vumarkTarget)
    {
        return GetValueFromDictionary(this.productImageTextures, GetVuMarkId(vumarkTarget));
    }

    IEnumerator downloadProductImage(VuMarkTarget vumarkTarget, string url)
    {
        using (WWW www = new WWW(url))
        {
            // Wait for download to complete
            yield return www;

            // assign texture
            productImageTextures.Add(GetVuMarkId(vumarkTarget),FlipTextureY(www.texture));            
        }
    }

    Texture2D FlipTextureY(Texture2D sourceTexture)
    {
        Debug.Log("<color=cyan>FlipTextureY() called.</color>");

        Texture2D targetTexture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
        Color[] sourceColors = sourceTexture.GetPixels();
        Color[] targetColors = targetTexture.GetPixels();
        int sourceIndex = 0;
        int targetIndex = 0;

        // read from the bottom of source texture
        for (int row = sourceTexture.height - 1; row > -1; row -= 1)
        {
            for (int rowPixel = 0; rowPixel < sourceTexture.width; rowPixel += 1)
            {
                sourceIndex = (row * sourceTexture.width) + rowPixel - 1;
                // When we reach first index of row 0, change the -1 to 0
                if (sourceIndex == -1) { sourceIndex = 0; }
                targetColors[targetIndex] = sourceColors[sourceIndex];
                targetIndex += 1;
            }
        }

        targetTexture.SetPixels(targetColors);
        targetTexture.Apply();
        return targetTexture;
    }

    void GenerateVuMarkBorderOutline(VuMarkBehaviour vumarkBehaviour)
    {
        this.lineRenderer = vumarkBehaviour.GetComponentInChildren<LineRenderer>();

        if (this.lineRenderer == null)
        {
            Debug.Log("<color=green>Existing Line Renderer not found. Creating new one.</color>");
            GameObject vumarkBorder = new GameObject("VuMarkBorder");
            vumarkBorder.transform.SetParent(vumarkBehaviour.transform);
            vumarkBorder.transform.localPosition = Vector3.zero;
            vumarkBorder.transform.localEulerAngles = Vector3.zero;
            vumarkBorder.transform.localScale =
                new Vector3(
                    1 / vumarkBehaviour.transform.localScale.x,
                    1,
                    1 / vumarkBehaviour.transform.localScale.z);
            this.lineRenderer = vumarkBorder.AddComponent<LineRenderer>();
            this.lineRenderer.enabled = false;
            this.lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            this.lineRenderer.receiveShadows = false;
            // This shader needs to be added in the Project's Graphics Settings,
            // unless it is already in use by a Material present in the project.
            this.lineRenderer.material.shader = Shader.Find("Unlit/Color");
            this.lineRenderer.material.color = Color.clear;
            this.lineRenderer.positionCount = 4;
            this.lineRenderer.loop = true;
            this.lineRenderer.useWorldSpace = false;
            Vector2 vumarkSize = vumarkBehaviour.GetSize();
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 1.0f);
            this.lineRenderer.widthCurve = curve;
            this.lineRenderer.widthMultiplier = 0.003f;
            float vumarkExtentsX = (vumarkSize.x * 0.5f) + (this.lineRenderer.widthMultiplier * 0.5f);
            float vumarkExtentsZ = (vumarkSize.y * 0.5f) + (this.lineRenderer.widthMultiplier * 0.5f);
            this.lineRenderer.SetPositions(new Vector3[]
            {
                new Vector3(-vumarkExtentsX, 0.001f, vumarkExtentsZ),
                new Vector3(vumarkExtentsX, 0.001f, vumarkExtentsZ),
                new Vector3(vumarkExtentsX, 0.001f, -vumarkExtentsZ),
                new Vector3(-vumarkExtentsX, 0.001f, -vumarkExtentsZ)
            });
        }
    }

    void DestroyChildAugmentationsOfTransform(Transform parent)
    {
        if (parent.childCount > PersistentNumberOfChildren)
        {
            for (int x = PersistentNumberOfChildren; x < parent.childCount; x++)
            {
                Destroy(parent.GetChild(x).gameObject);
            }
        }
    }

    T GetValueFromDictionary<T>(Dictionary<string, T> dictionary, string key)
    {
        if (dictionary.ContainsKey(key))
        {
            T value;
            dictionary.TryGetValue(key, out value);
            return value;
        }
        return default(T);
    }

    void ToggleRenderers(GameObject obj, bool enable)
    {
        var rendererComponents = obj.GetComponentsInChildren<Renderer>(true);
        var canvasComponents = obj.GetComponentsInChildren<Canvas>(true);

        foreach (var component in rendererComponents)
        {
            // Skip the LineRenderer
            if (!(component is LineRenderer))
            {
                component.enabled = enable;
            }
        }

        foreach (var component in canvasComponents)
        {
            component.enabled = enable;
        }
    }

    void UpdateClosestTarget()
    {
        if (VuforiaRuntimeUtilities.IsVuforiaEnabled() && VuforiaARController.Instance.HasStarted)
        {
            Camera cam = DigitalEyewearARController.Instance.PrimaryCamera ?? Camera.main;

            float closestDistance = Mathf.Infinity;

            foreach (VuMarkBehaviour vumarkBehaviour in this.vumarkManager.GetActiveBehaviours())
            {
                Vector3 worldPosition = vumarkBehaviour.transform.position;
                Vector3 camPosition = cam.transform.InverseTransformPoint(worldPosition);

                float distance = Vector3.Distance(Vector2.zero, camPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    this.closestVuMark = vumarkBehaviour.VuMarkTarget;
                }
            }

            if (this.closestVuMark != null &&
                this.currentVuMark != this.closestVuMark)
            {
                ProductInfo productInfo = new ProductInfo();
                productInfo = RetrieveProductInfoForVuMarkTarget(this.closestVuMark);
                string name = productInfo.name;
                string price = string.Format("${0:0.00}", decimal.Parse(productInfo.price));
                string description = productInfo.description;
                Sprite image = GetProductImage(this.closestVuMark);
                bool isGold = productInfo.golden;
                this.currentVuMark = this.closestVuMark;

                StartCoroutine(ShowPanelAfter(0.5f, name, price, description, image, isGold));
            }
        }
    }

    IEnumerator ShowPanelAfter(float seconds, string vuMarkId, string vuMarkDataType, string vuMarkDesc, Sprite vuMarkImage, bool vuMarkisGold)    {
        addToCartBtn.GetComponent<UnityEngine.UI.Image>().sprite = addToCartSprite;
        addToCartBtn.interactable = true;
        yield return new WaitForSeconds(seconds);

        if (this.nearestVuMarkScreenPanel)
        {
            nearestVuMarkScreenPanel.Show(vuMarkId, vuMarkDataType, vuMarkDesc, vuMarkImage, vuMarkisGold);
        }
    }

    public void addToCart()
    {
        string id = GetVuMarkId(this.currentVuMark);
        StartCoroutine(updateServerAboutCart(id));
        addToCartBtn.GetComponent<UnityEngine.UI.Image>().sprite = addedToCartSprite;
        addToCartBtn.interactable = false;

    }
    IEnumerator updateServerAboutCart(string id)
    {
        string uri = "http://revamp.ap-southeast-1.elasticbeanstalk.com/user/" + user_id + "/shopping/add/item/vumark/" + id;
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            //no need response
        }
    }

    #endregion // PRIVATE_METHODS
}


[System.Serializable]
public class ProductInfo
{
    public string name;
    public string id;
    public string country;
    public string description;
    public string url;
    public string price;
    public string rating;
    public string serial;
    public string supplier;
    public string weight;
    public bool golden;

    public ProductInfo()
    {
        name = "";
        id = "";
        country = "";
        description = "";
        url = "";
        price = "";
        rating = "";
        serial = "";
        weight = "";
        supplier = "";
        golden = false;
    }
    public static ProductInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ProductInfo>(jsonString);
    }
}

//TODO: should i initialize all dictionaries with blank object ref first?