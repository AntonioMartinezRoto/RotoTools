
namespace RotoEntities
{
    public class Colour
    {
        public int Id { get; set; }   // PK obligatoria
        public string Name { get; set; }
        public List<Article> ArticleList { get; set; }
    }
}
