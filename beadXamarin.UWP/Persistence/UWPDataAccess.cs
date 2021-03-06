using beadXamarin.Droid.Persistence;
using beadXamarin.Persistence;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(UWPDataAccess))]
namespace beadXamarin.Droid.Persistence
{
    /// <summary>
    /// Tic-Tac-Toe adatel�r�s megval�s�t�sa Android platformra.
    /// </summary>
    public class UWPDataAccess : IGameDataAccess
    {
        /// <summary>
        /// F�jl bet�lt�se.
        /// </summary>
        /// <param name="path">El�r�si �tvonal.</param>
        /// <returns>A beolvasott mez��rt�kek.</returns>
        public async Task<GameTable> LoadAsync(String path)
        {
            try
            {
                String filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);

                if (!File.Exists(path))
                {
                    throw new FileLoadException();
                }
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
        public async Task SaveAsync(String path, GameTable table)
        {
            String text = table.M.ToString()
                          + " " + table.N.ToString()
                          + "\n";

            for (Int32 i = 0; i < table.M; i++)
            {
                for (Int32 j = 0; j < table.N; j++)
                {
                    text += table.TableCharRepresentation[i, j] + " "; // mez��rt�kek
                }

                text += "\n";
            }

            String filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);

            //await Task.Run(() => File.WriteAllText(filePath, text));
        }

    }
}