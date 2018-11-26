using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities
{
    [Table("member")]
    public class User
    {
        [Key] [Column("mem_id")] public string mem_id { get; set; }
        [Column("mem_receive_email")] public string mem_receive_email { get; set; }
        [Column("mem_email_cert")] public string mem_email_cert { get; set; }
        [Column("mem_phone")] public string mem_phone { get; set; }
        [Column("mem_phone_cert")] public string mem_phone_cert { get; set; }
        [Column("mem_img")] public string mem_img { get; set; }
        [Column("mem_register_ip")] public string mem_register_ip { get; set; }
        [Column("mem_lastlogin_ip")] public string mem_lastlogin_ip { get; set; }
        [Column("mem_pw")] public string mem_pw { get; set; }
        [Column("mem_username")] public string mem_username { get; set; }
        [Column("mem_nickname")] public string mem_nickname { get; set; }
        [Column("mem_auth_key")] public string mem_auth_key { get; set; }
        [Column("mem_register_datetime")] public DateTime mem_register_datetime { get; set; }
        [Column("mem_lastlogin_datetime")] public DateTime mem_lastlogin_datetime { get; set; }
        [Column("mem_denied")] public int mem_denied { get; set; }
        [Column("mem_level")] public int mem_level { get; set; }
        [Column("mem_like")] public int mem_like { get; set; }
        [Column("mem_recom")] public int mem_recom { get; set; }
        [Column("mem_article_cnt")] public int mem_article_cnt { get; set; }
        [Column("mem_article_like")] public int mem_article_like { get; set; }
        [Column("ico_write_auth")] public int ico_write_auth { get; set; }
        [Column("mem_mileage")] public decimal mem_mileage { get; set; }
        [Column("mem_address")] public string mem_address { get; set; }
        public int IsProcessing { get; set; } = 0;
        public int Version { get; set; } = 0;
        public string Status { get; set; } = Commons.Constants.Status.STATUS_PENDING;
        public static User FromJson(string json) =>
            JsonHelper.DeserializeObject<User>(json, JsonHelper.CONVERT_SETTINGS);

        public static string ToJson(User self) =>
            JsonHelper.SerializeObject(self, JsonHelper.CONVERT_SETTINGS);
        
       
    }
}