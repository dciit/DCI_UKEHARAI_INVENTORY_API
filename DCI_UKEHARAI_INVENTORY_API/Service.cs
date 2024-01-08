using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;

namespace DCI_UKEHARAI_INVENTORY_API
{
    public class Service
    {
        private readonly DBSCM _DBSCM;

        public Service(DBSCM dBSCM)
        {
            _DBSCM = dBSCM;
        }

        public List<AlGsdActpln> Plans(string ym = "")
        {
            List<AlGsdActpln> res = new List<AlGsdActpln>();
            if (ym != "")
            {
                res = _DBSCM.AlGsdActplns.Where(x => ym.Contains(x.Prdymd != null ? x.Prdymd : "")).ToList();
            }
            return res;
        }
    }
}
