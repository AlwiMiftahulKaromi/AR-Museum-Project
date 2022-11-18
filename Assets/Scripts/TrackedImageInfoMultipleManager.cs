using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using SimpleJSON;
using TMPro;
using System.Linq;


[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoMultipleManager : MonoBehaviour
{
    public string ImageName;
    [Space]
    [SerializeField]
    private GameObject scanHelper;
    [SerializeField]
    private Texture2D noImageFound;
    [SerializeField]
    private RawImage insectRawImage;
    public GameObject loading;

    [SerializeField]
    private TextMeshProUGUI countrysText;
    [SerializeField]
    private TextMeshProUGUI[] countryTexts;
    [SerializeField]
    private TextMeshProUGUI[] lonTexts;
    [SerializeField]
    private TextMeshProUGUI[] latTexts;
    [SerializeField]
    private TextMeshProUGUI insectSpeciesText;
    [SerializeField]
    private TextMeshProUGUI insectGenusText;
    [SerializeField] 
    private TextMeshProUGUI insectFamilyText;
    [SerializeField]
    private TextMeshProUGUI insectOrderText;
    

    [SerializeField]
    private Text insectDescriptionText;


    private readonly string baseApiGBIFURL = "https://api.gbif.org/v1/";
    int insectGBIFTaxonKey = 0;

    [SerializeField]
    private Insect[] arScriptableObjects;

    /*[SerializeField]
    private Vector3 scaleFactor = new Vector3(0.05f, 0.05f, 0.05f); //untuk scaling prefab*/

    private ARTrackedImageManager m_TrackedImageManager; //menamakan script ARTrackedImageManager as m_TrackedImageManager
                                                         //berfungsi untuk mendeklarasikan Serialized Library (XR Reference Image Library) yang nantinya akan digunakan
                                                         //juga berfungsi untuk mendeklarasikan Max Number Of Moving Images

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>(); //ini buat nyimpen prefab
    private Dictionary<string, Insect> arObjectsData = new Dictionary<string, Insect>();

    //new nyoba buat dictionary buat nyimpen data scientific name //dan ternyata bisa
    private Dictionary<string, string> scientificNameDictionary = new Dictionary<string, string>();
    private Dictionary<string, string> genusDictionary = new Dictionary<string, string>();
    private Dictionary<string, string> familyDictionary = new Dictionary<string, string>();
    private Dictionary<string, string> orderDictionary = new Dictionary<string, string>();

    private Dictionary<string, Texture2D> occurrenceMediaDictionary = new Dictionary<string, Texture2D>();

    //ini baru
    private Dictionary<string, List<string>> countryDictionary = new Dictionary<string, List<string>>();
    private List<string> countryList = new List<string>();
    private Dictionary<string, string> countryDistinctDictionary = new Dictionary<string, string>();
    private List<string> countrys = new List<string>();
    private List<string> countryDistinctList = new List<string>();

    private Dictionary<string, List<float>> longitudeDictionary = new Dictionary<string, List<float>>();
    private List<float> longitudeList = new List<float>();

    private Dictionary<string, List<float>> latitudeDictionary = new Dictionary<string, List<float>>();
    private List<float> latitudeList = new List<float>();
    //sampe sini
    //till here

    string currentActiveQR; //variabel yang nantinya digunakan untuk menyimpan nama trackedImage

    //new stopwatch
    bool stopwatchActive = false;
    float currentTime;
    public TextMeshProUGUI currentTimeText;

	void Start()
	{
        currentTime = 0;
	}

	void Update()
	{
        if (stopwatchActive == true)
        {
            currentTime = currentTime + Time.deltaTime;
        }
        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.Minutes.ToString() + ":" + time.Seconds.ToString() + ":" + time.Milliseconds.ToString();
	}
	//till here

	void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        // Setup semua game objects didalam Dictionary
        
        foreach (Insect arObject in arScriptableObjects)
        {
            GameObject newARObject = Instantiate(arObject.insectPrefab, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
            arObjectsData.Add(arObject.name, arObject);

            //new
            string namaa = arObject.name;
            insectGBIFTaxonKey = arObject.insectKey;
            StartCoroutine(GetInsectAtTaxonKey(namaa, insectGBIFTaxonKey));
            //till here
            //baru
            newARObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added) //Ketika salah satu dari setiap trackedImage yang berada di Serialized Library terdeteksi kamera
        {
            //UpdateARImage(trackedImage);
            Vector3 newRot = new Vector3(trackedImage.transform.localEulerAngles.x, trackedImage.transform.localEulerAngles.y, trackedImage.transform.localEulerAngles.z);
            //Vector3 newRot = new Vector3(trackedImage.transform.eulerAngles.x, trackedImage.transform.eulerAngles.y, trackedImage.transform.eulerAngles.z);
            AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position, newRot);
			//Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
			scanHelper.SetActive(false);
            stopwatchActive = false;
        }

		foreach (ARTrackedImage trackedImage in eventArgs.updated) //Ketika imageTracked yang terdeteksi berubah
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                currentActiveQR = trackedImage.referenceImage.name;

                Insect goARObjectData = arObjectsData[currentActiveQR];
                insectDescriptionText.text = goARObjectData.insectDescription;

                //new
                if (scientificNameDictionary != null)
                {
                    insectSpeciesText.text = scientificNameDictionary[currentActiveQR];
                }
                if (genusDictionary != null)
                {
                    insectGenusText.text = genusDictionary[currentActiveQR];
                }
                if (familyDictionary != null)
                {
                    insectFamilyText.text = familyDictionary[currentActiveQR];
                }
                if (orderDictionary != null)
                {
                    insectOrderText.text = orderDictionary[currentActiveQR];
                }
                //ini baru
                if (countryDistinctDictionary != null)
                {
                    countrysText.text = countryDistinctDictionary[currentActiveQR];
                }
                if (countryDictionary != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        countryTexts[i].text = (countryDictionary[currentActiveQR][i].ToString());
                    }
                }
                if (longitudeDictionary != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        lonTexts[i].text = (longitudeDictionary[currentActiveQR][i].ToString());
                    }
                }
                if (latitudeDictionary != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        latTexts[i].text = (latitudeDictionary[currentActiveQR][i].ToString());
                    }
                }
                //sampe sini
                if (occurrenceMediaDictionary != null)
                {
                    loading.SetActive(true);
                    loading.SetActive(false);
                    insectRawImage.texture = occurrenceMediaDictionary[currentActiveQR];
                }
                //till here
                Vector3 newRot = new Vector3(trackedImage.transform.localEulerAngles.x, trackedImage.transform.localEulerAngles.y, trackedImage.transform.localEulerAngles.z);
                //Vector3 newRot = new Vector3(trackedImage.transform.eulerAngles.x, trackedImage.transform.eulerAngles.y, trackedImage.transform.eulerAngles.z);
                AssignGameObject(currentActiveQR, trackedImage.transform.position, newRot);
                scanHelper.SetActive(false);
                stopwatchActive = false;
            }
            else
            {
                if (currentActiveQR != trackedImage.referenceImage.name)
                {
                    Vector3 newRot = new Vector3(trackedImage.transform.localEulerAngles.x, trackedImage.transform.localEulerAngles.y, trackedImage.transform.localEulerAngles.z);
					//Vector3 newRot = new Vector3(trackedImage.transform.eulerAngles.x, trackedImage.transform.eulerAngles.y, trackedImage.transform.eulerAngles.z);
					ReAssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position, newRot);
                }
			}
        }
        
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            arObjects[trackedImage.name].SetActive(false);
        }
        
    }

    //new
    IEnumerator GetInsectAtTaxonKey(string nama, int insectTaxonKey)
    {
        string insectApiGBIFURL = baseApiGBIFURL + "occurrence/search?taxonkey=" + insectTaxonKey.ToString();
        UnityWebRequest insectInfoRequest = UnityWebRequest.Get(insectApiGBIFURL);
        yield return insectInfoRequest.SendWebRequest();

        if (insectInfoRequest.isNetworkError || insectInfoRequest.isHttpError)
        {
            Debug.LogError(insectInfoRequest.error);
            yield break;
        }
        JSONNode insectInfo = JSON.Parse(insectInfoRequest.downloadHandler.text);

        string insectScientificName = insectInfo["results"][0]["acceptedScientificName"];
        scientificNameDictionary.Add(nama, insectScientificName);

        string insectGenus = insectInfo["results"][0]["genus"];
        genusDictionary.Add(nama, insectGenus);

        string insectFamily = insectInfo["results"][0]["family"];
        familyDictionary.Add(nama, insectFamily);

        string insectOrder = insectInfo["results"][0]["order"];
        orderDictionary.Add(nama, insectOrder);

        //ini baru
        //Debug.Log(nama);
        //longitudeList.Clear();
        countryList = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            string insectCountry = insectInfo["results"][i]["country"];
            if (insectCountry != null)
            {
                countryList.Add(insectCountry);
            }
            else
            {
                countryList.Add("Tidak ada");
            }

        }
        countryDictionary.Add(nama, countryList);
        
        //list baru
        countrys = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            string insectCountrys = insectInfo["results"][i]["country"];
            if (insectCountrys != null)
            {
                countrys.Add(insectCountrys);
            }
        }
        countrys.Sort();
        countryDistinctList = new List<string>();
        countryDistinctList = countrys.Distinct().ToList();
        string result = "";
        foreach (var co in countryDistinctList)
        {
            result += "- " + co.ToString() + "\n";
        }
        Debug.Log(result);
        countryDistinctDictionary.Add(nama, result);
        //sampe sini

        longitudeList = new List<float>();
        for (int i = 0; i < 10; i++)
        {
            float insectLongitude = insectInfo["results"][i]["decimalLongitude"];
            if (insectLongitude != null)
            {
                longitudeList.Add(insectLongitude);
            }
            else
            {
                longitudeList.Add(0);
            }
            
        }

        longitudeDictionary.Add(nama, longitudeList);

        latitudeList = new List<float>();
        for (int i = 0; i < 10; i++)
        {
            float insectLatitude = insectInfo["results"][i]["decimalLatitude"];
            if (insectLatitude != null)
            {
                latitudeList.Add(insectLatitude);
            }
            else
            {
                latitudeList.Add(0);
            }

        }
        latitudeDictionary.Add(nama, latitudeList);
        //sampe sini

        string insectMediaURL = insectInfo["results"][0]["media"][0]["identifier"];

        if (string.IsNullOrEmpty(insectMediaURL))
        {
            occurrenceMediaDictionary.Add(nama, noImageFound);
        }
        else
        {
            string insectMediaSmallURL = insectMediaURL.Replace("original", "small");

            //get insect sprite
            Debug.Log("Loading...");
            //WWW wwwLoader = new WWW(insectSpriteSmallURL); //create WWW object pointing to the url
            loading.SetActive(true);
            stopwatchActive = false; //stopwatch mulai
            currentTime = 0; //reset stopwatch
            //yield return wwwLoader; //start loading whatever in that url (delay happens here)

            if (!File.Exists(Application.persistentDataPath + ImageName))
            {
                //if internet available
                WWW wwwLoader = new WWW(insectMediaSmallURL);
                yield return wwwLoader;
                if (wwwLoader.error == null)
                {
                    //when image downloaded...
                    Debug.Log("Loaded");
                    loading.SetActive(false);
                    stopwatchActive = true; //stopwatch mulai
                    Texture2D textureNew = wwwLoader.texture;
                    //insectRawImage.texture = textureNew;
                    occurrenceMediaDictionary.Add(nama, textureNew);

                    byte[] dataByte = textureNew.EncodeToPNG();
                    File.WriteAllBytes(Application.persistentDataPath + ImageName, dataByte);
                    Debug.Log("Image Saved");
                }
                //if internet not available
                else
                {
                    Debug.Log("We Have Error! Please Try Again...");
                }
            }
            else
                if (File.Exists(Application.persistentDataPath + ImageName))
            {
                loading.SetActive(false);
                stopwatchActive = true; //stopwatch mulai
                byte[] uploadByte = File.ReadAllBytes(Application.persistentDataPath + ImageName);
                Texture2D textureNew = new Texture2D(10, 10);
                textureNew.LoadImage(uploadByte);
                //insectRawImage.texture = textureNew;
                occurrenceMediaDictionary.Add(nama, textureNew);
                Debug.Log("Image Loaded");
            }
        }   

    }
    //till here
    void AssignGameObject(string name, Vector3 newPosition, Vector3 newRotation) //Karena di arObjectsToPlace terdapat lebih dari 1 arObject
    {
        if (arObjects != null)
        //if (arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name]; //Maka GameObject yang akan di assign adalah arObject yang memiliki nama yang sama dengan imageTracked
            goARObject.SetActive(true); //Tampilkan (aktifkan) arObject tersebut
            goARObject.transform.position = newPosition; //Posisikan juga arObject sesuai sesuai dengan posisi imageTracked
            goARObject.transform.eulerAngles = newRotation;
            //goARObject.transform.localScale = scaleFactor; //Skalasi arObject sesuai skala yang sudah di set di awal

            foreach (GameObject go in arObjects.Values) //Untuk setiap arObject lainnya
            {
                //Debug.Log($"Go in arObjects.Values: {go.name}");
                if (go.name != name) //Jika namanya tidak sama dengan nama imageTracked
                {
                    go.SetActive(false); //Jangan ditampilkan
                }
            }
        }
    }

    void ReAssignGameObject(string name, Vector3 newPosition, Vector3 newRotation)
    {
        if (arObjects != null)
        //if (arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name];
            goARObject.SetActive(true);
            goARObject.transform.position = newPosition;
            goARObject.transform.eulerAngles = newRotation;
            //goARObject.transform.localScale = scaleFactor;
            foreach (GameObject go in arObjects.Values)
            {
                if (go.name == name)
                {
                    go.SetActive(false);
                }
            }
        }
    }
}