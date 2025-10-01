
namespace RotoEntities
{
    public class Fitting
    {
        public int Id { get; set; }
        public string Ref { get; set; }
        public string Description { get; set; }
        public string Manufacturer{ get; set; }
        public int FittingGroupId { get; set; }
        public string Location {  get; set; }   
        public string FittingType { get; set; }
        public string System { get; set; }
        public string HandUseable { get; set; }
        public double Lenght { get; set; }
        public bool StartCuttable { get; set; }
        public bool EndCuttable { get; set; }
        public List<Article> ArticleList { get; set; }
        public List<Operation> OperationList { get; set; }
        public FittingGroup FittingGroup { get; set; }
    }
}
