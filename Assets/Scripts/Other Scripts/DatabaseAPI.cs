using Firebase;
using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class DatabaseAPI : MonoBehaviour
{
    FirebaseApp app;

    DatabaseReference reference;
    PermissionsManager permissionsManager;

    private EventHandler<ChildChangedEventArgs> selectedListener;

    private void Awake()
    {
        /*
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Debug.Log("Firebase is ready to use.");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
        */

        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Start()
    {
        permissionsManager = GameObject.Find("PermissionsManager").GetComponent<PermissionsManager>();
    }

    public void PostMessage(Message message, Action callback, Action<AggregateException> fallback)
    {
        var messageJSON = StringSerializationAPI.Serialize(typeof(Message), message);
        reference.Child("messages").Push().SetRawJsonValueAsync(messageJSON).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else callback();
        });
    }

    public void ListenForNewMessages(Action<Message> callback, Action<AggregateException> fallback)
    {
        void CurrentListener (object o, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                fallback(new AggregateException(new Exception(args.DatabaseError.Message)));
                Debug.LogError(args.DatabaseError.Message);
            }
            else
            {
                callback(StringSerializationAPI.Deserialize(typeof(Message), args.Snapshot.GetRawJsonValue()) as Message);
            }
        }

        reference.Child("messages").ChildAdded += CurrentListener;
    }

    public void UpdateSelectObject(string selectedid, string objectName, Action callback, Action<AggregateException> fallback)
    {
        selectedid = "selecteditems";
        var objectNameJSON = StringSerializationAPI.Serialize(typeof(string), objectName);
        reference.Child($"selected/{selectedid}/name").SetRawJsonValueAsync(objectNameJSON).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else callback();
        });
    }

    public void ListenForSelectedObject(Action<Selected> callback, Action<AggregateException> fallback)
    {
        void CurrentListener(object o, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null) fallback(new AggregateException(new Exception(args.DatabaseError.Message)));
            else
                callback(StringSerializationAPI.Deserialize(typeof(Selected), args.Snapshot.GetRawJsonValue()) as Selected);
        }

        reference.Child("selected").ChildChanged += CurrentListener;
    }


    public void ResetTrail(string uniqueID, Trail trail)
    {
        UpdatePositions(trail, () => Debug.Log("Log from Firebase: Trail added."), exception => Debug.LogError(exception));
    }

    void UpdatePositions(Trail trail, Action callback, Action<AggregateException> fallback)
    {
        
        reference.Child($"trails/{trail.user}").SetRawJsonValueAsync(null).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else callback();
        });

        var TrailRecordedJSON = StringSerializationAPI.Serialize(typeof(Trail), trail);
        reference.Child($"trails/{trail.user}").SetRawJsonValueAsync(TrailRecordedJSON).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else callback();
        });

    }

    //WARNING: This is a bonus added for "Experimental" purposes.
    //It does not apply to a Client's own trail since it's also stored in Client side
    //To properly delete, all Clients need to close up then Delete with the Button from the Server Side
    public void DeleteAllTrails(Action callback, Action<AggregateException> fallback)
    {
        reference.Child("trails").SetRawJsonValueAsync(null).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else callback();
        });
    }

    public void ListenForTrailChanges(Action<Trail> callback, Action<AggregateException> fallback)
        {
        void CurrentListener(object o, ChildChangedEventArgs args)
            {
            if (args.DatabaseError != null)
                {
                fallback(new AggregateException(new Exception(args.DatabaseError.Message)));
                Debug.LogError(args.DatabaseError.Message);
                }
            else
                {
                callback(StringSerializationAPI.Deserialize(typeof(Trail), args.Snapshot.GetRawJsonValue()) as Trail);
                }
            }

        reference.Child("trails").ChildAdded += CurrentListener;
        }

    }
