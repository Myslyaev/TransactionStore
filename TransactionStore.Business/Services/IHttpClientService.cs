namespace TransactionStore.Business.Services;

public interface IHttpClientService
{
    Task<T> Get<T>(string urlForRequest, CancellationToken cancellationToken);
}