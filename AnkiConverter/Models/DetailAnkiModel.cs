namespace AnkiConverter.Models
{
    public class DetailAnkiModel
    {
        public List<FileItemModel> Files { get; set; }
        public List<ItemCardModel> Texts { get; set; }
        public string Media { get; set; }
    }
}
