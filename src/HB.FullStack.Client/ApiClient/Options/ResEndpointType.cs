namespace HB.FullStack.Client.ApiClient
{
    public enum ResEndpointType
    {
        //EndpointValue为从哪个Controller来获取。也是Model的名字，这个Resource的主要Model来源,归谁管
        ControllerModel,

        PlainUrl
    }
}
