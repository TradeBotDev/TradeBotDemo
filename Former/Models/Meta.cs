using System.Collections.Generic;
using System.Linq;

namespace Former.Models
{
    public class Meta
    {
        public static readonly List<Metadata> MetadataList = new();

        public static Metadata GetMetadata(string sessionId, string tradeMarket, string slot, string userid)
        {
            var result = MetadataList.FirstOrDefault(el => el.Sessionid== sessionId && el.Trademarket == tradeMarket && el.Slot == slot);
            if (result is not null) return result;
            result = new Metadata
            {
                Sessionid = sessionId,
                Trademarket = tradeMarket,
                Slot = slot,
                UserId = userid
            };
            MetadataList.Add(result);
            return result;
        }

        public static List<Metadata> GetMetaList()
        {
            return MetadataList;
        }
    }
}
