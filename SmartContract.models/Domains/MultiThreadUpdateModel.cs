namespace SmartContract.models.Domains
{
    public class MultiThreadUpdateModel : BaseModel
    {
        public int IsProcessing { get; set; } = 0;
        public int Version { get; set; } = 0;
        public string Status { get; set; } = Commons.Constants.Status.STATUS_PENDING;
    }
}