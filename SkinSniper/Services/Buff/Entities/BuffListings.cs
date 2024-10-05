namespace SkinSniper.Services.Buff.Entities
{
    class BuffListings
    {
        internal class _Data
        {
            internal class Item
            {
                internal class AssetInfo
                {
                    internal class _Info
                    {
                        internal class PhaseData
                        {
                            public string Color { get; set; }
                            public string Name { get; set; }
                        }

                        public PhaseData? Phase_Data { get; set; }
                    }

                    public string PaintWear { get; set; }
                    public _Info Info { get; set; }
                }

                public string Id { get; set; }
                public string Price { get; set; }
                public AssetInfo Asset_Info { get; set; }
                public long Updated_At { get; set; }
            }

            public List<Item> Items { get; set; }
        }

        public string Code { get; set; }
        public _Data? Data { get; set; }
        public string Msg { get; set; }
    }
}
