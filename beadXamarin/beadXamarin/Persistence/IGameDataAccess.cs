using System.Threading.Tasks;

namespace beadXamarin.Persistence
{
    public interface IGameDataAccess
    {
        Task<GameTable> LoadAsync(string path);
    }
}