using System.Collections.Generic;

namespace Rinjani.Coincheck
{
    public class LeveragePositionReply
    {
        public bool success { get; set; }
        public Pagination pagination { get; set; }
        public List<Datum> data { get; set; }

        public class Pagination
        {
            public int limit { get; set; }
            public string order { get; set; }
            public object starting_after { get; set; }
            public object ending_before { get; set; }
        }

        public class NewOrder
        {
            public string id { get; set; }
            public string side { get; set; }
            public object rate { get; set; }
            public object amount { get; set; }
            public string pending_amount { get; set; }
            public string status { get; set; }
            public string created_at { get; set; }
        }

        public class CloseOrder
        {
            public string id { get; set; }
            public string side { get; set; }
            public string rate { get; set; }
            public string amount { get; set; }
            public string pending_amount { get; set; }
            public string status { get; set; }
            public string created_at { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string pair { get; set; }
            public string status { get; set; }
            public string created_at { get; set; }
            public object closed_at { get; set; }
            public string open_rate { get; set; }
            public object closed_rate { get; set; }
            public decimal amount { get; set; }
            public string all_amount { get; set; }
            public string side { get; set; }
            public string pl { get; set; }
            public NewOrder new_order { get; set; }
            public List<CloseOrder> close_orders { get; set; }
        }
    }
}