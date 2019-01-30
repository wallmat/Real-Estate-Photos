using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class PhotoManager : MonoBehaviour 
{
    string m_FolderPath = "";
    string m_NewFolderPath = "";

    public InputField m_Price = null;
    public InputField m_Location = null;
    public InputField m_Info = null;

    public Text m_ErrorText = null;
    public Text m_NextButtonText = null;

    public Button m_NextButton = null;
    public Image m_House = null;

    int m_CurrentPhoto = 0;
    List<FileInfo> m_Photos = new List<FileInfo>();

	// Use this for initialization
	void Awake () 
    {
        m_FolderPath = Application.dataPath + "/Photos";
        m_NewFolderPath = Application.dataPath + "/NewPhotos";
	}

    void Start()
    {
        //Look in the local directory for all the images
        //This should be in a location based on a config but
        //this was only a prototype
        if (!Directory.Exists(m_NewFolderPath))
            Directory.CreateDirectory(m_NewFolderPath);

        DirectoryInfo dir = new DirectoryInfo(m_FolderPath);
        FileInfo[] _photos = dir.GetFiles("*.*");
        for (int i = 0; i < _photos.Length; i++)
        {
            if (_photos[i].Name.Contains(".meta"))
                continue;

            //Debug.Log("Adding photo : " + _photos[i].Name);
            m_Photos.Add(_photos[i]);
        }

        if (!Directory.Exists(m_FolderPath) || m_Photos.Count < 1)
        {
            Directory.CreateDirectory(m_FolderPath);
            m_ErrorText.text = "Photos not found, Please make sure you have your photos in :" + '\n' + m_FolderPath;
            return;
        }

        StartCoroutine(LoadNextPhoto());
    }

    public void NextPhotoButtonClicked()
    {
        StartCoroutine(PrepForNextPhoto());
    }

    IEnumerator PrepForNextPhoto()
    {
        //when they are finished with that photo we can hide the next button
        //and fix the price to be a currency 
        Cursor.visible = false;
        m_NextButton.gameObject.SetActive(false);
        m_Price.text = string.Format("${0:N0}", long.Parse(m_Price.text));

        yield return StartCoroutine(CreateNewPhoto());

        //move to the next photo, if that was the last one just exit for them
        ++m_CurrentPhoto;
        if (m_CurrentPhoto + 1 > m_Photos.Count)
        {
            Application.Quit();
            yield break;
        }

        //turn the cursor back on and the next button
        Cursor.visible = true;
        m_NextButton.gameObject.SetActive(true);

        //clear out the fields and get ready to do it again
        ClearFields();

        if (m_CurrentPhoto + 1 >= m_Photos.Count)
            m_NextButtonText.text = "Finish";

        yield return StartCoroutine(LoadNextPhoto());
    }

    //just clear the fields for the next image
    void ClearFields()
    {
        m_Info.text = "";
        m_Price.text = "";
        m_Location.text = "";

        m_House.sprite = null;
    }

    IEnumerator LoadNextPhoto()
    {
        //Debug.Log("Current Index : " + m_CurrentPhoto + " Number of Photos : " + m_Photos.Count);
        //Debug.Log("Loading Picture : " + m_Photos[m_CurrentPhoto].Name);

        m_NextButton.interactable = false;

        WWW _image = new WWW("file://" + m_Photos[m_CurrentPhoto].FullName);
        yield return _image;

        m_NextButton.interactable = true;

        Sprite _temp = Sprite.Create(_image.texture, new Rect(0, 0, _image.texture.width, _image.texture.height), new Vector2(0.5f, 0.5f));
        m_House.sprite = _temp;

        //if there is only one photo they will be done after this
        if(m_Photos.Count == 1)
            m_NextButtonText.text = "Finish";
    }

    //creates a screen shot of what unity is displaying as a new image for them to use
    public IEnumerator CreateNewPhoto()
    {
        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D(Screen.width, Screen.height);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToJPG();

        // save in memory
        System.IO.File.WriteAllBytes(m_NewFolderPath + "/" + m_Photos[m_CurrentPhoto].Name, bytes);
    }
}
