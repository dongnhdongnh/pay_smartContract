using SmartContract.Commons.Helpers;

namespace SmartContract.models.Domains
{
    public class BaseModel
    {
        public string Id { get; set; } = CommonHelper.GenerateUuid();
        public long CreatedAt { get; set; } = CommonHelper.GetUnixTimestamp();
        public long UpdatedAt { get; set; } = CommonHelper.GetUnixTimestamp();
    }
}