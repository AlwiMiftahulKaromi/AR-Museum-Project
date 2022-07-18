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

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoMultipleManager : MonoBehaviour
{
    public string ImageName;
    [Space]
    [SerializeField]
    private Texture2D noImageFound;
    [SerializeField]
    private RawImage insectRawImage;
    public GameObject loading;

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

    [SerializeField]
    private Vector3 scaleFactor = new Vector3(0.05f, 0.05f, 0.05f); //untuk scaling prefab

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
    //till here

    string currentActiveQR; //variabel yang nantinya digunakan untuk menyimpan nama trackedImage

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
            UpdateARImage(trackedImage);
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
                    insectGenusText.text = "Genus: " + genusDictionary[currentActiveQR];
                }
                if (familyDictionary != null)
                {
                    insectFamilyText.text = "Family: " + familyDictionary[currentActiveQR];
                }
                if (orderDictionary != null)
                {
                    insectOrderText.text = "Order: " + orderDictionary[currentActiveQR];
                }
                if (occurrenceMediaDictionary != null)
                {
                    loading.SetActive(true);
                    loading.SetActive(false);
                    insectRawImage.texture = occurrenceMediaDictionary[currentActiveQR];
                }
                //till here

                AssignGameObject(currentActiveQR, trackedImage.transform.position);
            }
            else
            {
                if (currentActiveQR != trackedImage.referenceImage.name)
                {
                    ReAssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);
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

        string insectScientificName = insectInfo["results"][0]["scientificName"];
        scientificNameDictionary.Add(nama, insectScientificName);

        string insectGenus = insectInfo["results"][0]["genus"];
        genusDictionary.Add(nama, insectGenus);

        string insectFamily = insectInfo["results"][0]["family"];
        familyDictionary.Add(nama, insectFamily);

        string insectOrder = insectInfo["results"][0]["order"];
        orderDictionary.Add(nama, insectOrder);

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

    private void UpdateARImage(ARTrackedImage trackedImage)
    {
        // Panggil fungsi AssignGameObject dengan nama dan posisi dari trackedImage sebagai parameternya
        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);

        //Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
    }
    void AssignGameObject(string name, Vector3 newPosition) //Karena di arObjectsToPlace terdapat lebih dari 1 arObject
    {
        if (arObjects != null)
        //if (arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name]; //Maka GameObject yang akan di assign adalah arObject yang memiliki nama yang sama dengan imageTracked
            goARObject.SetActive(true); //Tampilkan (aktifkan) arObject tersebut
            goARObject.transform.position = newPosition; //Posisikan juga arObject sesuai sesuai dengan posisi imageTracked
            goARObject.transform.localScale = scaleFactor; //Skalasi arObject sesuai skala yang sudah di set di awal

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

    void ReAssignGameObject(string name, Vector3 newPosition)
    {
        if (arObjects != null)
        //if (arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name];
            goARObject.SetActive(true);
            goARObject.transform.position = newPosition;
            goARObject.transform.localScale = scaleFactor;
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