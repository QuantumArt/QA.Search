using Newtonsoft.Json;

namespace QA.Search.Integration.Demosite.Models
{
    public class TextPageDto
    {
        [JsonProperty("itemid")]
        public int? ItemID { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("contentitemid")]
        public int ContentItemId { get; set; }
    }
}
