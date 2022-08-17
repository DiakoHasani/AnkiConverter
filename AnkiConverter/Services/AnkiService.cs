using AnkiConverter.Models;
using Microsoft.Data.Sqlite;
using System.IO.Compression;

namespace AnkiConverter.Services
{
    public class AnkiService : IAnkiService
    {
        public async Task<List<ItemCardModel>> GetAnkies(ZipArchiveEntry item)
        {
            var collectionFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Items/Ankies", $"collection{Guid.NewGuid()}.anki");
            using (var sqlTempFile = File.OpenWrite(collectionFilePath))
            {
                await item.Open().CopyToAsync(sqlTempFile);
            }
            using (var sqlDb = new SqliteConnection("Data Source=" + Path.GetFullPath(collectionFilePath)))
            {
                await sqlDb.OpenAsync();
                var command = new SqliteCommand("select * from notes", sqlDb);
                var dataReader = command.ExecuteReader();

                var texts = new List<ItemCardModel>();
                while (dataReader.Read())
                {
                    texts.Add(new ItemCardModel
                    {
                        From = dataReader["flds"].ToString(),
                        Back = dataReader["sfld"].ToString()
                    });
                }
                return texts;
            }
        }

        public async Task<ResultModel<DetailAnkiModel>> GetDetailAnki(string fileName)
        {
            try
            {
                var file = GetStream(fileName);
                if (file == null)
                {
                    return new ResultModel<DetailAnkiModel>
                    {
                        Code = 404,
                        Message = "not found file"
                    };
                }
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Items", Guid.NewGuid().ToString());
                using (var tmpStream = File.OpenWrite(path))
                {
                    await file.CopyToAsync(tmpStream);
                }

                using (var zipFile = ZipFile.OpenRead(path))
                {
                    var media = GetMedia(zipFile.Entries.FirstOrDefault(x => x.Name.Equals("media", StringComparison.InvariantCultureIgnoreCase)));

                    var items = zipFile.Entries.Where(a => a.Name != "media");

                    var files = new List<FileItemModel>();
                    var texts = new List<ItemCardModel>();

                    foreach (var item in items)
                    {
                        if (item.Name == "collection.anki2")
                        {
                            texts = await GetAnkies(item);
                        }
                        else
                        {
                            try
                            {
                                var itemName = GetFileNameWithMedia(item.Name, media);
                                if (string.IsNullOrWhiteSpace(itemName))
                                {
                                    itemName = $"{Guid.NewGuid()}.jpg";
                                }
                                item.ExtractToFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Items/FileCards", itemName));
                                files.Add(new FileItemModel
                                {
                                    Name = item.Name,
                                    Url = itemName
                                });
                            }
                            catch(Exception)
                            {

                            }
                        }
                    }

                    return new ResultModel<DetailAnkiModel>
                    {
                        Result = true,
                        Data = new DetailAnkiModel
                        {
                            Files = files,
                            Texts = texts,
                            Media=media
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResultModel<DetailAnkiModel>
                {
                    Message = $"error in main try catch GetDetailAnki error Message: {ex.Message}"
                };
            }
        }

        public string GetFileNameWithMedia(string name, string media)
        {
            try
            {
                var split1 = media.Split(name + ":");
                var split2 = split1[1].Split(",");
                var text = split2[0].Replace("}", "").Replace("{", "");
                return text;
            }
            catch
            {
                return "";
            }
        }

        public string GetMedia(ZipArchiveEntry item)
        {
            var mediaPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Items/Medias", $"media{Guid.NewGuid()}.txt");
            item.ExtractToFile(mediaPath);
            var media = "";
            foreach (var line in File.ReadAllLines(mediaPath))
            {
                media += line.Replace("\"", "");
            }
            return media;
        }

        public Stream GetStream(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files", fileName);
            if (File.Exists(path))
            {
                return File.OpenRead(path);
            }

            return null;
        }
    }
    public interface IAnkiService
    {
        Task<ResultModel<DetailAnkiModel>> GetDetailAnki(string fileName);
        Stream GetStream(string fileName);

        string GetMedia(ZipArchiveEntry item);
        string GetFileNameWithMedia(string name, string media);
        Task<List<ItemCardModel>> GetAnkies(ZipArchiveEntry item);
    }
}
