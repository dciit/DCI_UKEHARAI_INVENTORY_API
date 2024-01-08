using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DCI_UKEHARAI_INVENTORY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UkeharaiController : Controller
    {
        private readonly DBSCM _DBSCM;
        private readonly DBBCS _DBBCS;
        private OraConnectDB _ALPHAPD = new OraConnectDB("ALPHAPD");
        public UkeharaiController(DBSCM dBSCM, DBBCS dBBCS)
        {
            _DBSCM = dBSCM;
            _DBBCS = dBBCS;
        }

        [HttpGet]
        [Route("/model/get/{ym}")]
        public IActionResult GetModels(string ym)
        {
            var models = _DBSCM.AlGsdActplns.Where(x => x.DataType == "9" && x.Prdymd!.Contains(ym)).OrderBy(x => x.Model).Select(x => x.Model).Distinct();
            return Ok(models);
        }
        [HttpPost]
        [Route("/plan/get")]
        public IActionResult Acts([FromBody] MParam param)
        {
            List<MInbound> mInbounds = new List<MInbound>();
            OracleCommand oracleCommand = new OracleCommand();
            oracleCommand.CommandText = @"SELECT W.ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE, SUM(W.ASTQTY) ASTQTY 
FROM SE.WMS_ASSORT W
WHERE comid = 'DCI'  AND MODEL LIKE '%' AND PLNO LIKE '%' 
 AND TO_CHAR(astdate,'YYYY-MM-DD') BETWEEN '2023-12-01' AND '2023-12-31' 
GROUP BY W.ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE";
            DataTable dt = _ALPHAPD.Query(oracleCommand);
            foreach (DataRow dr in dt.Rows)
            {
                 MInbound mInbound = new MInbound();
                mInbound.astDate = dr["ASP"].ToString();
            }
            return Ok(dt);


            string model = "1Y115BKAX1N#A";
            List<MActual> response = new List<MActual>();
            string ym = param.ym!;
            List<AlSaleForecaseMonth> ListSaleForecast = _DBSCM.AlSaleForecaseMonths.Where(x => x.Ym == ym).ToList();
            List<AlGsdCurpln> ListCurrentPlan = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();
            List<AlGsdActpln> items = _DBSCM.AlGsdActplns.Where(x => x.DataType == "9" && x.Prdymd!.Contains(ym) && x.Model == model).ToList();
            if (ym != "")
            {
                var wcnos = items.Select(x => x.Wcno).Distinct();
                foreach (var itemWcno in wcnos)
                {
                    var models = items.Where(x => x.Wcno == itemWcno).Select(x => x.Model).Distinct().ToList();
                    foreach (var itemModel in models)
                    {
                        string sebango = "";
                        var modelDetail = _DBSCM.PnCompressors.Where(x => x.Model == itemModel).FirstOrDefault();
                        if (modelDetail != null)
                        {
                            sebango = modelDetail.ModelCode;
                        }
                        List<AlSaleForecaseMonth> itemSale = ListSaleForecast.Where(x => x.ModelCode == itemModel).ToList();
                        List<AlGsdActpln> prevItem = new List<AlGsdActpln>();
                        AlGsdCurpln itemCurrentPlan = new AlGsdCurpln();
                        AlGsdCurpln resultCurrentPlan = ListCurrentPlan.Where(x => x.Model == itemModel).FirstOrDefault();
                        if (resultCurrentPlan != null)
                        {
                            itemCurrentPlan = resultCurrentPlan;
                        }
                        var plans = items.Where(x => x.Wcno == itemWcno && x.Model == itemModel).ToList();
                        foreach (var itemPlan in plans)
                        {
                            prevItem.Add(new AlGsdActpln()
                            {
                                Model = itemModel,
                                Wcno = itemWcno,
                                Qty = itemPlan.Qty,
                                Prdymd = itemPlan.Prdymd!.Substring(itemPlan.Prdymd.Length - 2),
                            });
                        }
                        //response.Add(prevItem);
                        response.Add(new MActual()
                        {
                            ym = ym,
                            wcno = itemWcno!.Value.ToString(),
                            model = itemModel,
                            sebango = sebango,
                            listCurpln = itemCurrentPlan,
                            listActPln = prevItem.OrderBy(x => x.Prdymd).ToList(),
                            listSaleForecast = itemSale
                        });
                    }
                }

                //response = from act in itemAct
                //           orderby act.Prdymd, act.Model descending
                //           select new
                //           {
                //               wcno = act.Wcno,
                //               ym = act.Prdymd!.Substring(0, 6),
                //               d = act.Prdymd!.Substring(act.Prdymd.Length - 2),
                //               model = act.Model,
                //               qty = act.Qty
                //           };
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("/test")]
        public IActionResult Test()
        {
            return Ok("1");
        }
    }
}
