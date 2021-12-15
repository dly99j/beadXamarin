using System;
using System.IO;
using System.Threading.Tasks;

namespace beadXamarin.Persistence
{
    public class GameDataAccess : IGameDataAccess
    {
        public async Task<GameTable> LoadAsync(string path)
        {
            try
            {
                using (var reader = new StreamReader(path))
                {
                    var line = await reader.ReadLineAsync();
                    var size = line.Split(' ');
                    var tableM = int.Parse(size[0]);
                    var tableN = int.Parse(size[1]);
                    var table = new GameTable(tableM, tableN);

                    for (var i = 0; i < tableM; ++i)
                    {
                        line = await reader.ReadLineAsync();
                        for (var j = 0; j < tableN; ++j)
                            table.setFieldOnInit(i, j, line[j]);
                    }

                    return table;
                }
            }
            catch
            {
                throw new GameDataException();
            }
        }

        public async Task SaveAsync(string path, GameTable table)
        {
            throw new NotImplementedException();
        }
    }
}