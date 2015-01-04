using System;
using System.ComponentModel.DataAnnotations;

namespace ZkData
{
    public partial class Punishment
    {
        public int PunishmentID { get; set; }

        public int AccountID { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        public DateTime Time { get; set; }

        public DateTime? BanExpires { get; set; }

        public bool BanMute { get; set; }

        public bool BanCommanders { get; set; }

        public bool BanUnlocks { get; set; }

        public bool BanSite { get; set; }

        public bool BanLobby { get; set; }

        [StringLength(1000)]
        public string BanIP { get; set; }

        public bool BanForum { get; set; }

        public long? UserID { get; set; }

        public int? CreatedAccountID { get; set; }

        public bool DeleteInfluence { get; set; }

        public bool DeleteXP { get; set; }

        public bool SegregateHost { get; set; }

        public bool SetRightsToZero { get; set; }

        public virtual Account AccountByAccountID { get; set; }

        public virtual Account AccountByCreatedAccountID { get; set; }
    }
}