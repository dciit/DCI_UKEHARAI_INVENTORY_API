using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Globalization;

namespace DCI_UKEHARAI_INVENTORY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UkeharaiController : Controller
    {
        private readonly DBSCM _DBSCM;
        private readonly DBBCS _DBBCS;
        private OraConnectDB _ALPHAPD = new OraConnectDB("ALPHAPD");
        //private ConnectDB _DBSCM_SQL = new ConnectDB("DBSCM");
        //private ConnectDB _DBIOTFAC2 = new ConnectDB("DBIOTFAC2");
        private Service serv = new Service();
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
        public async Task<IActionResult> UkeharaiContent([FromBody] MParam param)
        {
            string ym = param.ym;
            string year = ym.Substring(0, 4);
            string month = ym.Substring(4, 2);
            int dayOfMonth = DateTime.DaysInMonth(int.Parse(year), int.Parse(month));
            List<MInbound> mInbounds = new List<MInbound>();
            List<MMainResult> mMainResult = new Service(_DBSCM).GetResultMain(year, month);
            OracleCommand oracleCommand = new OracleCommand();
            oracleCommand.CommandText = $@"SELECT TO_CHAR(W.ASTDATE,'YYYY-MM-DD') AS ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE, SUM(W.ASTQTY) ASTQTY 
FROM SE.WMS_ASSORT W
WHERE comid = 'DCI'  AND MODEL LIKE '%' AND PLNO LIKE '%' 
 AND TO_CHAR(astdate,'YYYY-MM-DD') BETWEEN '{year}-{month}-01' AND '{year}-{month}-{dayOfMonth}' 
GROUP BY W.ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE";
            DataTable dt = _ALPHAPD.Query(oracleCommand);
            foreach (DataRow dr in dt.Rows)
            {
                MInbound mInbound = new MInbound();
                if (dt.Columns.Contains("ASTDATE") && dt.Columns.Contains("MODEL") && dt.Columns.Contains("ASTQTY"))
                {
                    string dmy = dr["ASTDATE"].ToString();
                    double qty = double.Parse(dr["ASTQTY"].ToString());
                    if (dmy != null && dmy.Length >= 10)
                    {
                        //dmy = dmy.Substring(0, 10);
                        //mInbound.astDate = DateTime.ParseExact("01/02/2024", "dd/MM/yyyy", CultureInfo.CurrentCulture);
                        mInbound.astDate = dr["ASTDATE"].ToString();
                        mInbound.model = dr["MODEL"].ToString();
                        mInbound.pltype = dr["PLTYPE"].ToString();
                        mInbound.astQty = qty;
                        mInbound.astType = dr["ASTTYPE"].ToString();
                    }
                    mInbounds.Add(mInbound);
                }
            }
            //return Ok(dt);


            string model = "1YC22DXD#A";
            List<MActual> response = new List<MActual>();
            List<MInbound> listInbound = new List<MInbound>();
            List<MWms_MstPkm> listPltype = new List<MWms_MstPkm>();
            //set inventory data
            List<MInventory> listInventory = new List<MInventory>();
            OracleCommand strGetPltypeOfModel = new OracleCommand();
            strGetPltypeOfModel.CommandText = @"select model, pltype, strloc
from wms_mstpkm
where lrev = '999' 
group by model, pltype, strloc";
            DataTable dtPltype = _ALPHAPD.Query(strGetPltypeOfModel);
            foreach (DataRow dr in dtPltype.Rows)
            {
                MWms_MstPkm iWmsMstPkm = new MWms_MstPkm();
                iWmsMstPkm.model = dr["MODEL"].ToString();
                iWmsMstPkm.pltype = dr["PLTYPE"].ToString();
                iWmsMstPkm.strloc = dr["STRLOC"].ToString();
                listPltype.Add(iWmsMstPkm);
            }

            OracleCommand strAlphaPD = new OracleCommand();
            strAlphaPD.CommandText = @"select model, pltype, count(serial) cnt,to_char(current_date,'YYYY-MM-DD') as currentDate
from fh001 
where comid='DCI' and nwc in ('DCI','SKO')  
  and substr(model, 1, 1) in ('1','2','J')
  and locacode like '%'
group by model, pltype
order by model";
            DataTable dtInventory = _ALPHAPD.Query(strAlphaPD);
            foreach (DataRow dr in dtInventory.Rows)
            {
                MInventory iInventory = new MInventory();
                iInventory.model = dr["model"].ToString();
                iInventory.date = dr["currentDate"].ToString();
                iInventory.pltype = dr["pltype"].ToString();
                iInventory.cnt = dr["cnt"].ToString();
                listInventory.Add(iInventory);
            }
            //end 


            //List<>
            List<AlSaleForecaseMonth> ListSaleForecast = _DBSCM.AlSaleForecaseMonths.Where(x => x.Ym == ym && x.Lrev == "999").ToList();
            List<AlGsdCurpln> ListCurrentPlan = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();
            List<AlGsdActpln> items = _DBSCM.AlGsdActplns.Where(x => x.DataType == "9" && x.Prdymd!.Contains(ym) ).ToList();
            List<string> ListPltype = new List<string>();
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
                        List<AlSaleForecaseMonth> itemSale = ListSaleForecast.Where(x => x.ModelName == itemModel).ToList();
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
                        foreach (MInbound oInbound in mInbounds.Where(x => x.model == model).ToList())
                        {
                            listInbound.Add(oInbound);
                        }
                        ListPltype = listInbound.Select(x => x.pltype).ToList();
                        response.Add(new MActual()
                        {
                            ym = ym,
                            wcno = itemWcno!.Value.ToString(),
                            model = itemModel,
                            sebango = sebango,
                            listCurpln = itemCurrentPlan,
                            listActPln = prevItem.OrderBy(x => x.Prdymd).ToList(),
                            listSaleForecast = itemSale,
                            listInbound = listInbound.OrderBy(x => x.astDate).ToList(),
                            listPltype = listPltype.Where(x => x.model == itemModel).Select(x => x.pltype).ToList(),
                            listInventory = listInventory.Where(x => x.model == itemModel).ToList(),
                            listActMain = mMainResult.Where(x=>x.Model_No == sebango).ToList()
                        });
                        listInbound.Clear();
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
    }
}
