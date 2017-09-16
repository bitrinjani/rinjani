namespace Rinjani.Bitflyer
{
    public class ExecutionReply
    {
        public int id { get; set; }
        public string child_order_id { get; set; }
        public string side { get; set; }
        public decimal price { get; set; }
        public decimal size { get; set; }
        public decimal commission { get; set; }
        public string exec_date { get; set; }
        public string child_order_acceptance_id { get; set; }
    }
}