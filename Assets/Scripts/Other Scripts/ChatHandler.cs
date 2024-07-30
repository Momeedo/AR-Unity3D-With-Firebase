using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatHandler : MonoBehaviour
{
    DatabaseAPI database;
    PermissionsManager permissionsManager;
    string permission;

    public InputField textIF;
    public GameObject ChatContent;
    public GameObject MessageTextPrefab;

    ScrollRect scrollViewScrollRect;


    // Start is called before the first frame update
    void Start()
    {
        permissionsManager = GameObject.Find("PermissionsManager").GetComponent<PermissionsManager>();
        database = GameObject.Find("DatabaseManager").GetComponent<DatabaseAPI>();

        database.ListenForNewMessages(InstantiateMessage, Debug.Log);

        //Check is Server or Client
        //This can be replaced by the user id/name if authentification is implemented
        permission = "Client";
        if (permissionsManager.serverSide)
            {
            permission = "Server";
            }

        scrollViewScrollRect = ChatContent.transform.parent.parent.gameObject.GetComponent<ScrollRect>();
    }

    public void SendMessage()
    {
        if (textIF.text != "")
        {
            //Save message to database
            database.PostMessage(new Message(permission, textIF.text), () => Debug.Log("Log from Firebase: Message sent."), exception => Debug.LogError(exception));

            //Reset InputText 
            textIF.text = "";
        }
    }

    void InstantiateMessage(Message message)
    {
        //Instantite the new message
        GameObject intanciatedMessageText = Instantiate(MessageTextPrefab);
        intanciatedMessageText.transform.SetParent(ChatContent.transform, false);
        intanciatedMessageText.GetComponent<TextMeshProUGUI>().text = message.sender + ": " + message.text;

        //Scroll to the last message
        ScrollToBottom(scrollViewScrollRect);
    }

    void ScrollToBottom(ScrollRect scrollRect)
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = new Vector2(0, 0);
        Canvas.ForceUpdateCanvases();
        }

}

