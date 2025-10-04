Folder for Events scripts and files

`EventChannel<T>` works as a generic blueprint for an event channel, where `T` is
the type that is passed as an argument into the listener's method.

Usage is as such:

To create a `EventChannel` asset, first you need a concrete definition of what kind
of data that EventChannel will make use of. in `EventChannelTypes.cs`, there are 
some example class definitions for what those look like. Each one also has an
`[CreateAssetMenu]` tag that allows the `ScriptableObject` to be generated as an 
asset. Once created as an asset, it can be dragged in like any other game object
into a `[Serializable]` field supporting that type of Event.

Other examples of use for the base methods can be found in the 
`EventChannelTests.cs` in the testing folder.