using System;
using System.Collections;
using System.Collections.Generic;
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
    /*[SerializeField]
    private Text imageTrackedText; //ini untuk memanggil ImageTrackedText UI yang terdapat di Canvas*/
    [SerializeField]
    private RawImage insectRawImage;
    [SerializeField]
    private TextMeshProUGUI insectSpeciesText;
    [SerializeField]
    private TextMeshProUGUI insectGenusText;
    [SerializeField] 
    private TextMeshProUGUI insectFamilyText;
    [SerializeField]
    private TextMeshProUGUI insectOrderText;

    [SerializeField]
    private TextMeshProUGUI insectKeyText;

    [SerializeField]
    private Text insectDescriptionText;


    private readonly string baseApiGBIFURL = "https://api.gbif.org/v1/";
    int insectGBIFKey = 0;

    [SerializeField]
    private GameObject[] arObjectsToPlace; //membuat list baru untuk menyimpan prefab yang akan digunakan

    [SerializeField]
    private Insect[] arScriptableObjects;

    [SerializeField]
    private Vector3 scaleFactor = new Vector3(0.1f, 0.1f, 0.1f); //untuk scaling prefab

    private ARTrackedImageManager m_TrackedImageManager; //menamakan script ARTrackedImageManager as m_TrackedImageManager
                                                         //berfungsi untuk mendeklarasikan Serialized Library (XR Reference Image Library) yang nantinya akan digunakan
                                                         //juga berfungsi untuk mendeklarasikan Max Number Of Moving Images

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, Insect> arObjectsData = new Dictionary<string, Insect>();

    string currentActiveQR; //variabel yang nantinya digunakan untuk menyimpan nama trackedImage

	void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        // Setup semua game objects didalam Dictionary
        /*foreach (GameObject arObject in arObjectsToPlace)
        {
            GameObject newARObject = Instantiate(arObject, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
        }*/
        foreach (Insect arObject in arScriptableObjects)
        {
            GameObject newARObject = Instantiate(arObject.insectPrefab, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
            arObjectsData.Add(arObject.name, arObject);
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
            currentActiveQR = trackedImage.referenceImage.name; //Simpan namanya
            UpdateARImage(trackedImage); //lalu panggil fungsi UpdateARImage
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated) //Ketika imageTracked yang terdeteksi berubah
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                currentActiveQR = trackedImage.referenceImage.name;
                //imageTrackedText.text = currentActiveQR;

                //new
                Insect goARObjectData = arObjectsData[currentActiveQR];
                insectDescriptionText.text = goARObjectData.insectDescription;

                insectGBIFKey = goARObjectData.insectKey;
                StartCoroutine(GetInsectAtKey(insectGBIFKey));

                
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
            //UpdateARImage(trackedImage);
        }
        /*
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            arObjects[trackedImage.name].SetActive(false);
        }
        */
    }

    IEnumerator GetInsectAtKey(int insectGBIFnubKey)
    {
        insectKeyText.text = insectGBIFnubKey.ToString();

        string insectApiGBIFURL = baseApiGBIFURL + "occurrence/search?taxonkey=" + insectGBIFnubKey.ToString();

        UnityWebRequest insectInfoRequest = UnityWebRequest.Get(insectApiGBIFURL);
        yield return insectInfoRequest.SendWebRequest();

        if (insectInfoRequest.isNetworkError || insectInfoRequest.isHttpError)
        {
            Debug.LogError(insectInfoRequest.error);
            yield break;
        }

        JSONNode insectInfo = JSON.Parse(insectInfoRequest.downloadHandler.text);

        string insectSpecies = insectInfo["results"][0]["species"];
        insectSpeciesText.text = insectSpecies;

        string insectGenus = insectInfo["results"][0]["genus"];
        insectGenusText.text = insectGenus;

        string insectFamily = insectInfo["results"][0]["family"];
        insectFamilyText.text = insectFamily;

        string insectOrder = insectInfo["results"][0]["order"];
        insectOrderText.text = insectOrder;

        string insectSpriteURL = insectInfo["results"][0]["media"][0]["identifier"];
        //get insect sprite
        UnityWebRequest insectSpriteRequest = UnityWebRequestTexture.GetTexture(insectSpriteURL);

        yield return insectSpriteRequest.SendWebRequest();

        if (insectSpriteRequest.isNetworkError || insectSpriteRequest.isHttpError)
        {
            Debug.LogError(insectSpriteRequest.error);
            yield break;
        }
        insectRawImage.texture = DownloadHandlerTexture.GetContent(insectSpriteRequest);
    }

    private void UpdateARImage(ARTrackedImage trackedImage)
    {
        // Display nama dari tracked image ke Canvas (ImageTrackedText UI)
        //imageTrackedText.text = trackedImage.referenceImage.name;

        // Panggil fungsi AssignGameObject dengan nama dan posisi dari trackedImage sebagai parameternya
        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);

        //Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
    }

    void AssignGameObject(string name, Vector3 newPosition) //Karena di arObjectsToPlace terdapat lebih dari 1 arObject
    {
        if (arObjectsToPlace != null)
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
        if (arObjectsToPlace != null)
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