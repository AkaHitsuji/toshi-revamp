using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

public class PathPlannerManager : MonoBehaviour {

    public GameObject currentLocation;
    public GameObject targetLocation;
    public GameObject arrows;
    public GameObject allTargets;
    public List<GameObject> displayedArrows;
    public List<GameObject> arrowList;
    private bool targetChanged;
	// Use this for initialization
	void Start () {
        getAllArrows();
        foreach (GameObject arrow in arrowList)
        {
            arrow.GetComponent<MeshRenderer>().enabled = false;
        }
        currentLocation = null;
        //StartCoroutine(SetTargetLocationFromServer("initial"));
        SetTargetLocationOffline("Table1");
        targetChanged = false;
	}


    protected void SetTargetLocationOffline(string tag)
    {
        if (tag == "Table1")
            targetLocation = GameObject.FindWithTag("Table3");
        else if (tag == "Table3")
            targetLocation = GameObject.FindWithTag("Table5");
        else if (tag == "Table5")
            targetLocation = GameObject.FindWithTag("Table1");
    }

    IEnumerator SetTargetLocationFromServer(string tag)
    {
        //TODO: change URL
        string uri = "https://revamp-api.herokuapp.com/item/";
        UnityWebRequest uwr = UnityWebRequest.Post(uri, tag);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            ProductInfo productInfo = new ProductInfo();
            productInfo = ProductInfo.CreateFromJSON(uwr.downloadHandler.text);
            targetLocation = GameObject.FindWithTag(productInfo.name);
            targetChanged = true;
        }
    }
    

	// Update is called once per frame
	void Update () {}

    public void updateCurrentLocation(GameObject location)
    {
        if (location != currentLocation) //means new location detected
        {
            currentLocation = location;

            if (currentLocation == targetLocation) //means you've reached the target location
            {
                //SetTargetLocationFromServer(targetLocation.tag);
                SetTargetLocationOffline(targetLocation.tag);
            }
            updateDisplayedArrows();
        }
        
    }

    public void hideAllExceptDisplayedArrows()
    {
        foreach (GameObject arrow in arrowList)
        {
            if (displayedArrows.Contains(arrow))
                arrow.GetComponent<MeshRenderer>().enabled = true;
            else
                arrow.GetComponent<MeshRenderer>().enabled = false;
        } 
    }

    public void getAllArrows()
    {
        for (int i = 0; i < arrows.transform.childCount; i++)
        {
            arrowList.Add(arrows.transform.GetChild(i).gameObject);
        }
    }
    public void updateDisplayedArrows()
    {
        switch (targetLocation.tag)
        {
            case "Table1":
                displayedArrows.Add(arrowList[8]);
                if (currentLocation.tag == "Table5" || currentLocation.tag == "Table6")
                {
                    displayedArrows.Add(arrowList[4]);
                    displayedArrows.Add(arrowList[5]);
                    displayedArrows.Add(arrowList[6]);
                    displayedArrows.Add(arrowList[7]);
                }
                else if (currentLocation.tag == "Table3" || currentLocation.tag == "Table4")
                {
                    displayedArrows.Add(arrowList[6]);
                    displayedArrows.Add(arrowList[7]);
                }
                break;
            case "Table2":
                displayedArrows.Add(arrowList[9]);
                if (currentLocation.tag == "Table5" || currentLocation.tag == "Table6")
                {
                    displayedArrows.Add(arrowList[4]);
                    displayedArrows.Add(arrowList[5]);
                    displayedArrows.Add(arrowList[6]);
                    displayedArrows.Add(arrowList[7]);
                }
                else if (currentLocation.tag == "Table3" || currentLocation.tag == "Table4")
                {
                    displayedArrows.Add(arrowList[6]);
                    displayedArrows.Add(arrowList[7]);
                }
                break;
            case "Table3":
                displayedArrows.Add(arrowList[10]);
                if (currentLocation.tag == "Table1" || currentLocation.tag == "Table2")
                {
                    displayedArrows.Add(arrowList[0]);
                    displayedArrows.Add(arrowList[1]);
                }
                else if (currentLocation.tag == "Table5" || currentLocation.tag == "Table6")
                {
                    displayedArrows.Add(arrowList[4]);
                    displayedArrows.Add(arrowList[5]);
                }
                break;
            case "Table4":
                displayedArrows.Add(arrowList[11]);
                if (currentLocation.tag == "Table1" || currentLocation.tag == "Table2")
                {
                    displayedArrows.Add(arrowList[0]);
                    displayedArrows.Add(arrowList[1]);
                }
                else if (currentLocation.tag == "Table5" || currentLocation.tag == "Table6")
                {
                    displayedArrows.Add(arrowList[4]);
                    displayedArrows.Add(arrowList[5]);
                }
                break;
            case "Table5":
                displayedArrows.Add(arrowList[12]);
                if (currentLocation.tag == "Table1" || currentLocation.tag == "Table2")
                {
                    displayedArrows.Add(arrowList[0]);
                    displayedArrows.Add(arrowList[1]);
                    displayedArrows.Add(arrowList[2]);
                    displayedArrows.Add(arrowList[3]);
                }
                else if (currentLocation.tag == "Table3" || currentLocation.tag == "Table4")
                {
                    displayedArrows.Add(arrowList[2]);
                    displayedArrows.Add(arrowList[3]);
                }
                break;
            case "Table6":
                displayedArrows.Add(arrowList[13]);
                if (currentLocation.tag == "Table1" || currentLocation.tag == "Table2")
                {
                    displayedArrows.Add(arrowList[0]);
                    displayedArrows.Add(arrowList[1]);
                    displayedArrows.Add(arrowList[2]);
                    displayedArrows.Add(arrowList[3]);
                }
                else if (currentLocation.tag == "Table3" || currentLocation.tag == "Table4")
                {
                    displayedArrows.Add(arrowList[2]);
                    displayedArrows.Add(arrowList[3]);
                }
                break;
            default:
                break;
        }
        hideAllExceptDisplayedArrows();
    }
}
public class ProductInfo
{
    public string name;
    public string id;
    public string country;
    public string description;
    public string image;
    public string price;
    public string rating;
    public string serial;
    public string supplier;

    public ProductInfo()
    {
        name = "";
        id = "";
        country = "";
        description = "";
        image = "";
        price = "";
        rating = "";
        serial = "";
        supplier = "";
    }
    public static ProductInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ProductInfo>(jsonString);
    }
}