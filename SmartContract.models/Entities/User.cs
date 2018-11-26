using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities
{
    [Table("member")]
    public class User
    {
        [Column("mem_id")] public string Id { get; set; }
        [Column("mem_receive_email")] public string EmailReceive { get; set; }
        [Column("mem_email_cert")] public string EmailCert { get; set; }
        [Column("mem_phone")] public string PhoneNumber { get; set; }
        [Column("mem_phone_cert")] public string PhoneNumberCert { get; set; }
        [Column("mem_img")] public string Avatar { get; set; }
        [Column("mem_register_ip")] public string RegisterIp { get; set; }
        [Column("mem_lastlogin_ip")] public string LastLoginIp { get; set; }
        [Column("mem_pw")] public string Password { get; set; }
        [Column("mem_username")] public string Username { get; set; }
        [Column("mem_nickname")] public string Nickname { get; set; }
        [Column("mem_auth_key")] public string AuthKey { get; set; }
        [Column("mem_register_datetime")] public DateTime Register { get; set; }
        [Column("mem_lastlogin_datetime")] public DateTime Lastlogin { get; set; }
        [Column("mem_denied")] public int Denied { get; set; }
        [Column("mem_level")] public int level { get; set; }
        [Column("mem_like")] public int Like { get; set; }
        [Column("mem_recom")] public int Recom { get; set; }
        [Column("mem_article_cnt")] public int ArticleCnt { get; set; }
        [Column("mem_article_like")] public int ArticleLike { get; set; }
        [Column("ico_write_auth")] public int Auth { get; set; }
        [Column("mem_mileage")] public decimal DN { get; set; }
       
        public static User FromJson(string json) =>
            JsonHelper.DeserializeObject<User>(json, JsonHelper.CONVERT_SETTINGS);

        public static string ToJson(User self) =>
            JsonHelper.SerializeObject(self, JsonHelper.CONVERT_SETTINGS);
        
       
    }
}