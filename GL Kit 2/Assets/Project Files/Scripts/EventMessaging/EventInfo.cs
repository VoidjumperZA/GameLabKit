﻿using System.Collections;
using System.Collections.Generic;
using CustomEventCallbacks;
using UnityEngine;

//
//                    ABSTRACT
//
public abstract class EventInfo
{
    public string eventDescription = EventSystem.DESC_EVENT_GENERIC;
}

//
//                GENERIC EVENT
//
public class GenericEventInfo : EventInfo
{
    public GenericEventInfo(string pDescription)
    {
        eventDescription = pDescription;
    }
}

//
//                    PROGRESS CURRENT DIALOGUE
//
public class DialogueProgressEventInfo : EventInfo
{
    //An int used to represent the index in an array of strings
    public int progress;
    public DialogueProgressEventInfo(string pDescription, int pProgress)
    {
        eventDescription = pDescription;
        progress = pProgress;
    } 
}

//
//                    FETCH NEXT DIALOGUE
//
public class DialogueFetchNextEventInfo : EventInfo
{
    //An int used to represent the index in an array of strings
    public string fieldName;
    public DialogueFetchNextEventInfo(string pDescription, string pFieldName)
    {
        eventDescription = pDescription;
        fieldName = pFieldName;
    } 
}


//
//                    CAMERA TARGET
//
public class CameraTargetSelectEventInfo : EventInfo
{
    
    public Transform FocalA;
    public Transform FocalB;
    public CameraTargetSelectEventInfo(string pDescription, Transform pFocalA, Transform pFocalB)
    {
        eventDescription = pDescription;
        FocalA = pFocalA;
        FocalB = pFocalB;
    } 
}

//
//                    NEXT ROOM
//
public class NextRoomEventInfo : EventInfo
{
    public NextRoomEventInfo(string pDescription)
    {
        eventDescription = pDescription;  
    } 
}



//
//                    DETECT MARKER
//
public class DetectedMarkerEventInfo : EventInfo
{
    public string marker;
    public DetectedMarkerEventInfo(string pDescription, string pMarker)
    {
        eventDescription = pDescription;
        marker = pMarker;
    } 
}


//
//                    R2 OBJECT FOUND
//
public class ObjectFoundEventInfo : EventInfo
{
    public bool correct;
    public Settings.R2_ObjectsToFind objectFound;
    public ObjectFoundEventInfo(string pDescription, bool pCorrect, Settings.R2_ObjectsToFind pObjectFound)
    {
        eventDescription = pDescription;
        correct = pCorrect;
        objectFound = pObjectFound;
    } 
}