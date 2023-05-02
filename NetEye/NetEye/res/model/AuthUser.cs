using System;
using System.Collections.Generic;
using System.Text;

namespace NetEye.res.model
{
    public class AuthUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        public ICollection<RepairRequest> RepairRequestsSubmitted { get; set; }
        public ICollection<RepairRequest> RepairRequestsReceived { get; set; }

        public string FullName => string.Join(" ", FirstName, LastName, Patronymic);
    }
}
