namespace HB.FullStack.Client.Network
{
    public enum ConnectivityStatus
    {
        Disconnected, //Unkown, None, Local, ConstrainedInternet
        ConnectedSyncing,//Internet but syncing ,not ready for other requests
        ConnectedReady,//Internet, ready for requests
    }
}
