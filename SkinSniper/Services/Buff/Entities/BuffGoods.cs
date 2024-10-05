namespace SkinSniper.Services.Buff.Entities
{
    class BuffGoods
    {
        internal class _Data
        {
            internal class Item
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public List<Item> Items { get; set; }
            public int Page_Num { get; set; }
            public int Page_Size { get; set; }
            public int Total_Count { get; set; }
            public int Total_Page { get; set; }
        }

        public string Code { get; set; }
        public _Data? Data { get; set; }
        public string Msg { get; set; }
    }
}
