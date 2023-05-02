using System;
using System.Collections.Generic;
using System.Text;

namespace NetEye.res.model
{
    public class RepairRequest
    {
        public int Id { get; set; }

        public string TechIpAddress { get; set; }

        public string TechEquipmentId { get; set; }

        public int UserFromId { get; set; }

        public int UserToId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }
    }
    public enum RepairRequestStatus
    {
        Accepted,
        Working,
        Finished,
        Cancelled
    }
}
