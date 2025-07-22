namespace Game.Services
{
    public interface IPoolable
    {
        void OnGetFromPool();
        void OnReturnToPool();
    }
}