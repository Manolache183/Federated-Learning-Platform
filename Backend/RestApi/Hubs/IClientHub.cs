namespace RestApi.Hubs
{
    public interface IClientHub
    {
        Task ReceiveNotification(string message);
    }
}
