namespace TransactionStore.DataLayer.Repositories;

public class BaseRepository(TransactionStoreContext context)
{
    protected readonly TransactionStoreContext _ctx = context;
}