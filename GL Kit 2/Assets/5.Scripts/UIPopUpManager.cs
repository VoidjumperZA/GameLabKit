﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLab;

public class UIPopUpManager : MonoBehaviour
{
    [SerializeField] private DialogueObject[] levelPopupDialogueObjects;
    [SerializeField] private Canvas parentCanvas;
    private int levelIndex = 0;

    public void Start()
    {
        registerAllListeners();
    }

    /// <summary>
    /// Registers all event listeners this class needs to care about.
    /// </summary>
    private void registerAllListeners()
    {
        //EventSystem.RegisterListener(EventType.UI_NEXT_ROOM, OnNextRoomCommand);
        EventManager.Instance.AddListener<CreatePopUpEvent>(OnEventMarkerCall);
        EventManager.Instance.AddListener<LevelProgressEvent>(OnLevelProgess);
    }

    public void Thing()
    {
        CreatePopUp(levelPopupDialogueObjects[levelIndex]);
    }
    

    public void CreatePopUp(DialogueObject pDiagObj)
    {
        /*if (dialogueObject.GetFileName() != Settings.LEVEL_NAMES[Settings.LEVEL_ID_FOR_POPUP_FILE])
        {
            Debug.Log(Settings.ERR_ASSERT_POPUP_ASSIGNED_INCORRECT_FILE);
            return;
        }*/

        if (pDiagObj.Info.fieldName != Settings.JSON_POPUP_IDENTIFIER_KEY &&
            pDiagObj.GetTextAndIterate() != Settings.JSON_POPUP_IDENTIFIER_VALUE)
        {
            Debug.Log(Settings.ERR_DIAG_OBJ_POPUP_INCORRECT);
            return;
        }



        Debug.Log("DETECT");
        //layout visible
        string path = Settings.PATH_PREFABS + Settings.OBJ_NAME_UI_POPUP;
        Debug.Log(path);
        Canvas UICanvas = GameObject.Instantiate(Resources.Load<Canvas>(path));
        //GameObject.Instantiate(UICanvas);
        if (UICanvas == null)
        {
            Debug.Log("UICanvas is null");
        }

        UICanvas.name = "hello";

        UIPopUp popup = UICanvas.GetComponent<UIPopUp>();
        Debug.Assert(popup != null, Settings.ERR_ASSERT_UI_POPUP_MISSING_COMPONENT);
        UIPopUp.PopupFields data = popup.PopupFieldsData;

        data.title.text = pDiagObj.GetTextAndIterate();
        data.body.text = pDiagObj.GetTextAndIterate();

        string textFieldEnabled = pDiagObj.GetTextAndIterate();
        bool t = (textFieldEnabled.IsNullOrEmpty() || textFieldEnabled.ToLower() == "false") == true ? false : true;
        Debug.Log("Input Field is: " + t);
        data.inputField.gameObject.SetActive(t);

        string[] buttonNames = pDiagObj.GetTextAndIterate().Split(' ');
        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject newButton = GameObject.Instantiate(data.button.gameObject, UICanvas.transform);
            Vector3 newPosition = data.buttonAnchorPoint.position;
            newPosition.x += ((newButton.GetComponent<RectTransform>().rect.width * i) + data.buttonPadding);
            newButton.transform.position = newPosition;
            newButton.GetComponentInChildren<Text>().text = buttonNames[i];
        }
    }

    public void OnLevelProgess(LevelProgressEvent info)
    {
        levelIndex = Mathf.Clamp(info.levelID, 0, levelPopupDialogueObjects.Length - 1);
    }

    public void OnEventMarkerCall()
    {          
        //Don't uncomment this until a bug is fixed
        Debug.Log("level index: " + levelIndex);
        CreatePopUp(levelPopupDialogueObjects[levelIndex]);
    }

    public void OnCreateSpesifiedPopup(CreateSpesifiedPopUpEvent info)
    {
        CreatePopUp(info.dialogueObject);
    }
}
