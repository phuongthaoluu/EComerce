namespace Wholesale.Web.Models
{
    public class Agent
    {
        public int AgentID { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public User? User { get; set; }
    }
}
