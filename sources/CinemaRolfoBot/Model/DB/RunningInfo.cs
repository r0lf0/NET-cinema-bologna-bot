using CinemaRolfoBot.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaRolfoBot.Model.DB
{
    public enum ERunningInfoId
    {
        LastReset,
        LastUpdate
    }

    public class RunningInfo
    {
        [Key]
        public ERunningInfoId Id { get; set; }

        public DateTime Value
        {
            get => _Value;
            set { _Value = value.SetKindUtc(); }
        }

        [NotMapped]
        private DateTime _Value;

        public RunningInfo()
        {
        }

        public RunningInfo(ERunningInfoId Id, DateTime Value)
        {
            this.Id = Id;
            this.Value = Value;
        }
    }
}