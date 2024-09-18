using Azure;
using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Intrinsics.Arm;

namespace DCI_UKEHARAI_INVENTORY_API.Controllers
{
    // A001 (20240528 21:04) : แก้ไขการเรียกข้อมูลแผนการขาย โดยหาก lrev = 999 (แจกจ่าย) ใช้งานได้เลย แต่ถ้าไม่ (lrev != 999) ให้ไปใช้ rev, lrev ก่อนหน้า เช่น ปัจจุบัน rev = 10 && lrev = 10 จะได้ filter rev = 9 (10-1) , lrev = 10
    // A002 (20240530 14:00) : method : GetUkeharaiData เพิ่มส่ง InventoryPlanningFinal (ODM)
    // A003 (20240607 12:00) : get list pltype of model from WMS_MDW27_MODEL_MASTER 
    [ApiController]
    [Route("[controller]")]
    public class UkeharaiController : Controller
    {
        private readonly DBSCM _DBSCM;
        private readonly DBBCS _DBBCS;
        private OraConnectDB _ALPHAPD = new OraConnectDB("ALPHAPD");
        private OraConnectDB _ALPHAPD1 = new OraConnectDB("ALPHA01");
        private OraConnectDB _ALPHAPD2 = new OraConnectDB("ALPHA02");
        private SqlConnectDB _SQLSCM = new SqlConnectDB("dbSCM");
        Helper helper = new Helper();
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

        [HttpGet]
        [Route("/getmodel")]
        public async Task<IActionResult> getModels()
        {
            List<PnCompressor> models = serv.getModels();
            return Ok(models);
        }

        [HttpPost]
        [Route("/getUkeCurPln")]
        public async Task<IActionResult> getUkeCurPln([FromBody] MGetUkeCurPlan param)
        {
            List<int> wcno = _DBSCM.PnCompressors.Where(x => x.Model == param.model).Select(o => o.Line).ToList();
            List<UkeCurpln> data = _DBSCM.UkeCurplns.Where(x => x.Model == param.model && x.Prdym == ("2024" + param.month)).ToList();
            return Ok(new
            {
                wcno = wcno,
                data = data
            });
        }

        [HttpGet]
        [Route("/getCurplns/{year}")]
        public async Task<IActionResult> getUkeCurPln(string year = "")
        {
            List<UkeCurpln> list = _DBSCM.UkeCurplns.Where(x => x.Prdym.StartsWith(year)).ToList();
            return Ok(list);
        }


        [HttpPost]
        [Route("/distribution")]
        public async Task<IActionResult> distribution([FromBody] MDistribution param)
        {
            string model = param.model;
            string ym = param.ym;
            int? rev = _DBSCM.UkeCurplns.Where(x => x.Model == model && x.Prdym == ym).Max(x => x.Rev);
            List<UkeCurpln> list = _DBSCM.UkeCurplns.Where(x => x.Model == model && x.Prdym == ym && x.Rev == rev).ToList();
            list.ForEach(x => x.Lrev = 999);
            int update = _DBSCM.SaveChanges();
            return Ok(list);
        }



        [HttpPost]
        [Route("/ukeharai/adjustInventoryMain")]
        public async Task<IActionResult> AdjustInventoryMain([FromBody] MAdjustInventoryMain param)
        {
            string ym = param.ym;
            string empcode = param.empcode;
            string wcno = param.wcno;
            string model = param.model;
            string val = param.val;
            EkbWipPartStock content = await _DBSCM.EkbWipPartStocks.FirstOrDefaultAsync(x => x.Ptype == "MAIN" && x.Partno == model && x.Wcno == wcno && x.Ym == ym);
            if (content != null)
            {
                content.Bal = val != "" ? Convert.ToDecimal(val) : 0;
                _DBSCM.EkbWipPartStocks.Update(content);
            }
            else
            {
                EkbWipPartStock newRow = new EkbWipPartStock();
                newRow.Ym = ym;
                newRow.Wcno = wcno;
                newRow.Partno = model;
                newRow.PartDesc = "";
                newRow.Cm = "";
                newRow.Lbal = 0;
                newRow.Recqty = 0;
                newRow.Issqty = 0;
                newRow.Bal = val != "" ? Convert.ToDecimal(val) : 0;
                newRow.UpdateBy = empcode;
                newRow.UpdateDate = DateTime.Now;
                newRow.Ptype = "MAIN";
                _DBSCM.EkbWipPartStocks.Add(newRow);
            }
            int action = await _DBSCM.SaveChangesAsync();
            return Ok(new
            {
                status = action
            });
        }


        [HttpPost]
        [Route("/ukeharai/getAdjustInventoryMain")]
        public IActionResult GetAdjustInventoryMain([FromBody] MAdjustInventoryMain param)
        {
            string ym = param.ym;
            string empcode = param.empcode;
            string wcno = param.wcno;
            string model = param.model;
            string val = param.val;
            EkbWipPartStock content = new EkbWipPartStock();
            var getContent = _DBSCM.EkbWipPartStocks.FirstOrDefault(x => x.Ptype == "MAIN" && x.Partno == model && x.Wcno == wcno && x.Ym == ym);
            if (getContent != null)
            {
                content.Bal = getContent.Bal;
                content.Partno = getContent.Partno;
                content.Wcno = getContent.Wcno;
                content.PartDesc = getContent.PartDesc;
            }
            return Ok(content);
        }

        [HttpGet]
        [Route("/getCurPlnByYM/{ym}")]
        public IActionResult GetCurPlnByYm(string ym)
        {
            // (##) INIT VARIABLE 
            List<UkeCurpln> res = new List<UkeCurpln>();
            if (ym != "" && ym != null && ym.Length == 6)
            {
                // (##) GET (APS) CURPLN OF 'YM' :::: [SCM].[AL_GSD_ACTPLN]
                List<AlGsdCurpln> rApsCurPln = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();

                // (##) GET (UKE) CURPLN OF 'YM' :::: [SCM].[UKE_CURPLN]
                List<UkeCurpln> rUkeCurPln = _DBSCM.UkeCurplns.Where(o => o.Prdym == ym).ToList();

                // (##) GET MODEL(S)
                List<PnCompressor> rModel = _DBSCM.PnCompressors.Where(o => o.Status == "ACTIVE" && o.ModelType != "PACKING" && o.ModelType != "SPECIAL").ToList();

                // (##) DISTINCT MODEL NAME OF MODEL(S)
                List<string> rModelNames = rModel.Select(o => o.Model).Distinct().OrderBy(o => o).ToList();

                // (##) LOOP ITEM OF MODEL(S)
                foreach (string oModel in rModelNames)
                {
                    UkeCurpln oItem = new UkeCurpln();
                    // (##) GET WCNO OF CURPLN (APS)
                    List<int> rWcno = rApsCurPln.Where(x => x.Model == oModel).Select(x => x.Wcno).ToList();
                    foreach (int oWcno in rWcno)
                    {
                        oItem.Model = oModel;
                        oItem.Wcno = oWcno;
                        oItem.Prdym = ym;
                        res.Add(oItem);
                    }
                }

                // (##) CHECK EXIST ROW OF "YM"
                if (rUkeCurPln.Count == 0)
                {
                    int rowNum = 1;
                    foreach (UkeCurpln rowUkeCurPln in res)
                    {
                        rowUkeCurPln.RowNum = rowNum;
                        _DBSCM.UkeCurplns.Add(rowUkeCurPln);
                        rowNum++;
                    }
                    int insert = _DBSCM.SaveChanges();
                    if (insert == 0)
                    {
                        res.Clear();
                    }
                }
            }

            // (03) GET WCNO 
            // (04) LOOP WCNO
            // (##) CHECK EXIST CURPLN
            // (##) IF EXIST CURPLN SET PLAN
            // (##) PUSH TO RES[]
            return Ok(res);
        }

        [HttpPost]
        [Route("/change_uke_curpln")]
        public IActionResult changeUkeCurPln([FromBody] UkeCurpln rowChange)
        {
            string prdym = rowChange.Prdym;
            string model = rowChange.Model;
            int? rowNum = rowChange.RowNum;
            if (prdym != "" && model != "" && rowNum != 0)
            {
                UkeCurpln oUkeCurPln = _DBSCM.UkeCurplns.FirstOrDefault(x => prdym == prdym && x.Model == model && x.RowNum == rowNum);
                if (oUkeCurPln != null)
                {
                    _DBSCM.UkeCurplns.Update(oUkeCurPln);
                    int update = _DBSCM.SaveChanges();
                    return Ok(new
                    {
                        status = update,
                        error = "ไม่พบข้อมูลที่คุณกำลังแก้ไข"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        error = "ไม่พบข้อมูลที่คุณกำลังแก้ไข"
                    });
                }
            }
            else
            {
                return Ok(new
                {
                    status = false,
                    error = "ไม่พบข้อมูลบางส่วน"
                });
            }
        }



        [HttpPost]
        [Route("/changePlan")]
        public async Task<IActionResult> changePlan([FromBody] List<UkeCurpln> param)
        {
            bool status = true;
            if (param.Count > 0)
            {
                string ym = param[0].Prdym;
                if (ym != "")
                {
                    List<UkeCurpln> dtPlan = _DBSCM.UkeCurplns.Where(x => x.Prdym == ym && x.Lrev != 999).ToList();
                    foreach (UkeCurpln item in param)
                    {
                        string model = item.Model;
                        int wcno = item.Wcno;
                        UkeCurpln drPlan = dtPlan.FirstOrDefault(x => x.Model == model && x.Wcno == wcno);
                        if (drPlan != null)
                        {

                        }
                        else
                        {
                            item.Rev = 1;
                            item.Lrev = 1;
                            item.Udate = DateTime.Now;
                            item.Cdate = DateTime.Now;
                            _DBSCM.UkeCurplns.Add(item);
                        }
                    }
                    int action = _DBSCM.SaveChanges();
                    if (action == 0)
                    {
                        status = false;
                    }
                }
            }

            return Ok(new
            {
                status = status
            });
        }

        [HttpPost]
        [Route("/getUkeharaiGroupModel")]
        public IActionResult getUkeharaiGroupModel([FromBody] MParam param)
        {
            List<MGetUkeharaiGroupModel> rRes = new List<MGetUkeharaiGroupModel>();
            string ym = param.ym;
            int yyyy = Convert.ToInt32(ym.Substring(0, 4));
            int mm = Convert.ToInt32(ym.Substring(4, 2));
            DateTime dtStart = new DateTime(yyyy, mm, 01);
            DateTime dtEnd = new DateTime(yyyy, mm, DateTime.DaysInMonth(yyyy, mm));
            List<string> rModelGroup = new List<string>() { "1YC", "2YC", "SCR", "ODM" };

            // (##) ------- GET CURPLN 
            List<AlGsdCurpln> rCurrentPlan = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();
            var rCurPlnOfGroupModel = rCurrentPlan.Select(x => new
            {
                line = x.Wcno,
                x.Model,
                x.Sebango,
                x.Prdym,
                modelGroup = (x.Model.Substring(0, 1) == "1" || x.Model.Substring(0, 1) == "2") ? (x.Model.Substring(0, 1) + "YC") : (x.Model.Substring(0, 1) == "J" ? "SCR" : "ODM")
            }).ToList();
            // (##) ------- END

            // (##) ------- GET SALS FORECASE 
            List<AlSaleForecaseMonth> rSales = _DBSCM.AlSaleForecaseMonths.Where(x => x.Ym == ym && (x.Rev == x.Lrev || x.Lrev == "999")).ToList();
            // (##) ------- END


            foreach (string oModelGroup in rModelGroup)
            {
                MGetUkeharaiGroupModel iRes = new MGetUkeharaiGroupModel();
                List<MLineItem> mLineItems = new List<MLineItem>();
                List<string> rLineOfModelGroup = rCurPlnOfGroupModel.Where(x => x.modelGroup == oModelGroup).ToList().Select(x => x.line.ToString()).Distinct().ToList();
                foreach (string iLine in rLineOfModelGroup)
                {
                    MLineItem oLineItem = new MLineItem();
                    List<MGroupModelItem> rSaleGroup = new List<MGroupModelItem>();
                    oLineItem.line = iLine;
                    DateTime dtLoop = dtStart;
                    string dd = dtLoop.ToString("dd");
                    while (dtLoop < dtEnd.AddDays(1))
                    {
                        try
                        {
                            MGroupModelItem oSale = new MGroupModelItem();
                            oSale.date = dtLoop.ToString("yyyyMMdd");
                            List<AlSaleForecaseMonth> oSaleOfDay = rSales.Where(x => x.Ym == ym && x.Lrev == "999" && x.ModelName != "" && serv.getModelGroup(x.ModelName) == oModelGroup).ToList();
                            if (oSaleOfDay.Count > 0)
                            {
                                oSale.qty = oSaleOfDay.Sum(x => Convert.ToInt32(x.GetType().GetProperty($"D{dd}").GetValue(x).ToString())).ToString();
                            }
                            rSaleGroup.Add(oSale);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        dtLoop = dtLoop.AddDays(1);
                    }
                    oLineItem.sale = rSaleGroup;
                    mLineItems.Add(oLineItem);
                }
                iRes.groupModel = oModelGroup;
                iRes.line = mLineItems;
                rRes.Add(iRes);
            }

            return Ok(rRes);
        }

        [HttpPost]
        [Route("/plan/get")]
        public async Task<IActionResult> GetUkeharaiData([FromBody] MParam param)
        {
            string ym = param.ym;
            string year = ym.Substring(0, 4);
            string month = ym.Substring(4, 2);
            DateTime dtFilter = new DateTime(int.Parse(year), int.Parse(month), 1);
            int dayOfMonth = DateTime.DaysInMonth(int.Parse(year), int.Parse(month));
            List<MInbound> mInbounds = new List<MInbound>();

            // (##) RESULT MAIN
            List<MMainResult> mMainResult = new Service(_DBSCM).GetResultMain(year, month);

            // (##) GET INBOUND
            OracleCommand oracleCommand = new OracleCommand();
            oracleCommand.CommandText = $@"SELECT TO_CHAR(W.ASTDATE,'YYYY-MM-DD') AS ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE, SUM(W.ASTQTY) ASTQTY 
FROM SE.WMS_ASSORT W
WHERE comid = 'DCI'  AND MODEL LIKE '%' AND PLNO LIKE '%' 
 AND TO_CHAR(astdate -8/24,'YYYY-MM-DD') BETWEEN '{year}-{month}-01' AND '{year}-{month}-{dayOfMonth}' 
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
                        mInbound.astDate = dr["ASTDATE"].ToString();
                        mInbound.model = dr["MODEL"].ToString();
                        mInbound.pltype = dr["PLTYPE"].ToString();
                        mInbound.astQty = qty;
                        mInbound.astType = dr["ASTTYPE"].ToString();
                    }
                    mInbounds.Add(mInbound);
                }
            }

            List<MActual> response = new List<MActual>();
            List<MWms_MstPkm> listPltype = new List<MWms_MstPkm>();
            List<MInventory> allInventory = new List<MInventory>();
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


            List<MLastInventory> oLastInventory = new List<MLastInventory>();
            OracleCommand strGetLastInventory = new OracleCommand();
            strGetLastInventory.CommandText = @"SELECT W.YM, 
   W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
FROM SE.WMS_STKBAL W
WHERE comid= 'DCI' and ym =  :YM
  and wc in ('DCI','SKO')
GROUP BY W.YM, W.MODEL";
            string ymLastInventory = "";
            if (month == "01")
            {
                ymLastInventory = (Convert.ToInt32(year) - 1).ToString("D4") + "" + month;
            }
            else
            {
                ymLastInventory = year + "" + (Convert.ToInt32(month) - 1).ToString("D2");
            }
            strGetLastInventory.Parameters.Add(new OracleParameter(":YM", ymLastInventory));
            DataTable dtLastInventory = _ALPHAPD.Query(strGetLastInventory);
            foreach (DataRow dr in dtLastInventory.Rows)
            {
                MLastInventory iLastInventory = new MLastInventory();
                iLastInventory.ym = dr["YM"].ToString();
                //iLastInventory.wc = dr["WC"].ToString();
                iLastInventory.model = dr["MODEL"].ToString();
                iLastInventory.lbalstk = dr["LBALSTK"].ToString();
                iLastInventory.balstk = dr["BALSTK"].ToString();
                oLastInventory.Add(iLastInventory);
            }


            List<MHoldInventory> oHoldInventory = new List<MHoldInventory>();
            OracleCommand strHoldInventory = new OracleCommand();
            strHoldInventory.CommandText = @"SELECT W.YM ,
   W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
FROM SE.WMS_STKBAL W
WHERE comid= 'DCI' and ym = :YM
  and wc in ('HWH','RWQ')
and balstk > 0
GROUP BY W.YM,  W.MODEL";
            // RWQ
            strHoldInventory.Parameters.Add(new OracleParameter(":YM", (year + "" + month)));
            DataTable dtHoldInventory = _ALPHAPD.Query(strHoldInventory);
            foreach (DataRow dr in dtHoldInventory.Rows)
            {
                MHoldInventory iLastInventory = new MHoldInventory();
                iLastInventory.ym = dr["YM"].ToString();
                iLastInventory.model = dr["MODEL"].ToString();
                iLastInventory.balstk = dr["BALSTK"].ToString();
                iLastInventory.lbalstk = dr["LBALSTK"].ToString();
                oHoldInventory.Add(iLastInventory);
            }

            List<MHoldInventory> oPDTInventory = new List<MHoldInventory>();
            OracleCommand strPDT = new OracleCommand();
            strPDT.CommandText = @"SELECT W.YM, W.WC, 
   W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
FROM SE.WMS_STKBAL W
WHERE comid= 'DCI' and ym = :YM
  and wc in ('PDT')
  and balstk > 0
GROUP BY W.YM, W.WC, W.MODEL";
            strPDT.Parameters.Add(new OracleParameter(":YM", (year + "" + month)));
            DataTable dtPDT = _ALPHAPD.Query(strPDT);
            foreach (DataRow dr in dtPDT.Rows)
            {
                MHoldInventory iLastInventory = new MHoldInventory();
                iLastInventory.ym = dr["YM"].ToString();
                iLastInventory.wc = dr["WC"].ToString();
                iLastInventory.model = dr["MODEL"].ToString();
                iLastInventory.balstk = dr["BALSTK"].ToString();
                iLastInventory.lbalstk = dr["LBALSTK"].ToString();
                oPDTInventory.Add(iLastInventory);
            }


            OracleCommand strAlphaPD = new OracleCommand();
            strAlphaPD.CommandText = @"select model, pltype, count(serial) cnt,to_char(current_date,'YYYY-MM-DD') as currentDate
from fh001 
where comid='DCI' and nwc in ('DCI','SKO')  
  and locacode like '%'
group by model, pltype
order by model";
            //and substr(model, 1, 1) in ('1', '2', 'J')
            DataTable dtInventory = _ALPHAPD.Query(strAlphaPD);
            foreach (DataRow dr in dtInventory.Rows)
            {
                MInventory iInventory = new MInventory();
                iInventory.model = dr["model"].ToString();
                iInventory.date = dr["currentDate"].ToString();
                iInventory.pltype = dr["pltype"].ToString();
                iInventory.cnt = dr["cnt"].ToString();
                allInventory.Add(iInventory);
            }
            //end

            // (##) GET OSW03 DELIVERY 
            List<MOSW03Delivery> mOSW03Deliveries = new List<MOSW03Delivery>();
            OracleCommand strGetDelivery = new OracleCommand();
            strGetDelivery.CommandText = $@"SELECT TO_CHAR(H.DELDATE, 'yyyyMMdd') DELDATE,  
                                               W.MODEL, W.PLTYPE,   
                                               SUM(W.QTY) QTY, 
                                               SUM(W.ALQTY) ALQTY,   
                                               SUM(W.PICQTY) PICQTY   
                                            FROM SE.WMS_DELCTN W
                                            LEFT JOIN SE.WMS_DELCTL H ON H.COMID='DCI' AND H.IVNO = W.IVNO AND H.DONO = W.DONO 
                                            WHERE W.CFBIT = 'F' AND W.IFBIT = 'F' AND TO_CHAR(H.DELDATE, 'yyyyMMdd') LIKE '{year}{month}%'
                                            GROUP BY TO_CHAR(H.DELDATE, 'yyyyMMdd') , W.MODEL, W.PLTYPE   ";
            DataTable dtDelivery = _ALPHAPD.Query(strGetDelivery);
            foreach (DataRow dr in dtDelivery.Rows)
            {
                MOSW03Delivery oSW03Delivery = new MOSW03Delivery();
                oSW03Delivery.model = dr["MODEL"].ToString().Trim();
                oSW03Delivery.pltype = dr["PLTYPE"].ToString().Trim();
                oSW03Delivery.deldate = dr["DELDATE"].ToString();
                oSW03Delivery.qty = Convert.ToInt32(dr["QTY"].ToString());
                oSW03Delivery.alqty = Convert.ToInt32(dr["ALQTY"].ToString());
                oSW03Delivery.picqty = Convert.ToInt32(dr["PICQTY"].ToString());
                mOSW03Deliveries.Add(oSW03Delivery);
            }


            /* A003 */
            List<WmsMdw27ModelMaster> rMDW27 = new List<WmsMdw27ModelMaster>();
            SqlCommand sql = new SqlCommand();
            sql.CommandText = @"SELECT MODEL,PLTYPE FROM [dbSCM].[dbo].[WMS_MDW27_MODEL_MASTER] where active = 'active' group by MODEL,PLTYPE order by model asc,pltype asc";
            DataTable dtMDW27 = _SQLSCM.Query(sql);
            foreach (DataRow dr in dtMDW27.Rows)
            {
                WmsMdw27ModelMaster oModelPltype = new WmsMdw27ModelMaster();
                oModelPltype.Model = dr["MODEL"].ToString();
                oModelPltype.Pltype = dr["PLTYPE"].ToString();
                rMDW27.Add(oModelPltype);
            }
            /* [E] A003 */

            // (##) GET AREA GST_SALMDL
            List<GstSalMdl> rGstSalMdl = new List<GstSalMdl>();
            OracleCommand strGstSalMdl = new OracleCommand();
            strGstSalMdl.CommandText = @"SELECT G.AREA SKU, G.MODL_NM MODELNAME  FROM PLAN.GST_SALMDL G where lrev = '999'";
            DataTable dtGstSalMdl = _ALPHAPD1.Query(strGstSalMdl);
            foreach (DataRow drGstSalMdl in dtGstSalMdl.Rows)
            {
                GstSalMdl oGstSalMdl = new GstSalMdl();
                string modelName = drGstSalMdl["MODELNAME"].ToString();
                string sku = drGstSalMdl["SKU"].ToString();
                oGstSalMdl.modelName = modelName;
                oGstSalMdl.sku = sku;
                rGstSalMdl.Add(oGstSalMdl);
            }
            // END 

            //List<>
            List<PnCompressor> rModel = serv.getModels();
            List<string> rModelType = rModel.Select(x => x.ModelType).ToList();


            /* A001 */
            int rev = 0;
            int lrev = 0;
            SqlCommand sqlCheckVersion = new SqlCommand();
            sqlCheckVersion.CommandText = @"SELECT TOP(1) REV,LREV FROM [dbSCM].[dbo].[AL_SaleForecaseMonth] WHERE ym LIKE '" + year + "%'   order by CAST(rev as int) desc , CAST(lrev as int) desc";
            DataTable dtGetVersion = _SQLSCM.Query(sqlCheckVersion);
            if (dtGetVersion.Rows.Count > 0)
            {

                rev = Convert.ToInt32(dtGetVersion.Rows[0]["REV"].ToString());
                lrev = Convert.ToInt32(dtGetVersion.Rows[0]["LREV"].ToString());
                // ถ้าเจอ lrev = 999 ใช้งานได้เลย เนื่องจาก แจกจ่าย แล้ว
                if (lrev != 999)  // ค้นหา (rev - 1), lrev = (rev - 1) เพื่อหาข้อมูลที่ Distribution ก่อนหน้านี้
                {
                    //rev = lrev - 1;
                    rev = rev - 1;
                }
            }
            /* A001 */

            List<AlSaleForecaseMonth> ListSaleForecast = _DBSCM.AlSaleForecaseMonths.Where(x => x.Ym == ym && x.Rev == rev.ToString() && x.Lrev == lrev.ToString()).ToList();
            //List<AlSaleForecaseMonth> ListSaleForecast = _DBSCM.AlSaleForecaseMonths.Where(x => x.Ym == ym && (x.Rev == x.Lrev || x.Lrev == "999")).ToList();
            List<AlGsdCurpln> rCurrentPlan = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();
            List<DstWipPrd> rResultFinal = new List<DstWipPrd>();
            OracleCommand strGetResultFinal = new OracleCommand();
            strGetResultFinal.CommandText = @"select prdymd,wcno,model,sum(qty) as QTY  from dst_wippdr where prdymd like '" + ym + "%' and lrev = 999  group by prdymd,wcno,model";
            DataTable dtResultFinal = _ALPHAPD2.Query(strGetResultFinal);
            foreach (DataRow dr in dtResultFinal.Rows)
            {
                DstWipPrd itemResultFinal = new DstWipPrd();
                itemResultFinal.prdymd = dr["PRDYMD"].ToString();
                itemResultFinal.model = dr["MODEL"].ToString().Trim();
                itemResultFinal.qty = dr["QTY"].ToString() != "" ? decimal.Parse(dr["QTY"].ToString()) : 0;
                itemResultFinal.wcno = dr["WCNO"].ToString();
                rResultFinal.Add(itemResultFinal);
            }
            List<string> ListPltype = new List<string>();
            List<EkbWipPartStock> listEkbInventoryMain = _DBSCM.EkbWipPartStocks.Where(x => x.Ym == ymLastInventory && x.Ptype == "MAIN").ToList();
            List<string> rGroupModel = new List<string>();

            List<string> rYM = new List<string>() { ym }; // สำหรับเอาไป contains saleforecase มากกว่า 1 m
            for (int i = 1; i < 3; i++)
            {
                int y = int.Parse(rYM.LastOrDefault().Substring(0, 4));
                int m = int.Parse(rYM.LastOrDefault().Substring(4, 2));
                DateTime dtNext = new DateTime(y, m, 1);
                rYM.Add(dtNext.AddMonths(i).ToString("yyyyMM"));
            }
            List<AlSaleForecaseMonth> rSaleForeCaseAlLCustomer = _DBSCM.AlSaleForecaseMonths.Where(x => x.Lrev == "999" && rYM.Contains(x.Ym)).ToList();
            //List<PnCompressor> rModelDetail = _DBSCM.PnCompressors.Where(x => x.Status == "ACTIVE").ToList();
            List<WmsMdw27ModelMaster> rMdw27 = _DBSCM.WmsMdw27ModelMasters.ToList();
            DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtStart = DateTime.Now;

            //List<EkbWipPartStock> rStockCurrent = _DBSCM.EkbWipPartStocks.Where(x => x.Ym == ym && x.Ptype == "MAIN").ToList();
            List<ModelUkeharai> rUke = new List<ModelUkeharai>();
            List<DictMstr> ListPalletOfModel = _DBSCM.DictMstrs.Where(x => x.DictSystem == "SALEFC" && x.DictType == "CUST_PL" && x.Ref1 != null && x.DictStatus == "ACTIVE").ToList();

            if (ym != "")
            {
                var groupModel = rMdw27.Select(o => o.Model).Distinct();
                foreach (string oModel in rMdw27.Select(o => o.Model).Distinct())
                {
                    List<MHoldInventory> rInvHoldOfModel = oHoldInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();
                    List<MDelivery> rDelivery = new List<MDelivery>();
                    string modelGroup = serv.getModelGroup(oModel);
                    WmsMdw27ModelMaster oModelDetail = rMdw27.FirstOrDefault(x => x.Model == oModel);
                    string modelName = oModelDetail.Model;
                    string sebango = oModelDetail != null ? oModelDetail.Sebango : "";
                    MActual oResponse = new MActual();
                    oResponse.ym = ym;
                    oResponse.modelGroup = modelGroup;
                    oResponse.model = oModel;
                    oResponse.sebango = helper.SetDigit(sebango, 4);
                    oResponse.modelCode = helper.SetDigit(sebango, 4);
                    List<MMainResult> rMainResult = mMainResult.Where(x => x.Model_No == helper.ConvStrToInt(sebango).ToString("D4") || x.ModelName == oModel.Trim()).ToList();

                    // (1) GET,SET SALE FORECASE
                    //List<AlSaleForecaseMonth> rSaleForecase = ListSaleForecast.Where(x => x.ModelName == oModel && x.Ym == ym && x.Lrev == "999").ToList();

                    /* A001 */
                    List<AlSaleForecaseMonth> rSaleForecase = ListSaleForecast.Where(x => x.ModelName == oModel && x.Ym == ym && x.Rev == rev.ToString() && x.Lrev == lrev.ToString()).ToList();
                    if (rSaleForecase != null)
                    {
                        oResponse.listSaleForecast = rSaleForecase;
                    }

                    // (##) CAL INV.PLN.MAIN 
                    oResponse.listInventoryPlanningMain = serv.getInvPlnMain(ym, oModel, listEkbInventoryMain, rSaleForecase, oHoldInventory, rMainResult, rInvHoldOfModel);

                    // (##) GET INV.PLN.MAIN
                    List<EkbWipPartStock> rEKBInvPlnMain = _DBSCM.EkbWipPartStocks.Where(x => x.Ym == ym && x.Partno == oModel).ToList();
                    try
                    {
                        if (rEKBInvPlnMain.Count > 0)
                        {
                            oResponse.totalInventoryPlanningMain = rEKBInvPlnMain.Count > 0 ? (int)rEKBInvPlnMain.Sum(x => x.Bal) : 0;
                        }
                        else
                        {
                            oResponse.totalInventoryPlanningMain = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        oResponse.totalInventoryPlanningMain = 0;
                    }

                    List<MInventory> rInventory = allInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();
                    if (rInventory.Count > 0)
                    {
                        MInventory oInventory = rInventory.FirstOrDefault()!;
                        if (oInventory != null && oInventory.date.Substring(0, 7).Replace("-", "") == ym)
                        {
                            oResponse.Inventory = rInventory;
                        }
                    }



                    // (3) INBOUND
                    List<MInbound> rInbound = mInbounds.Where(x => x.model == oModel).OrderBy(x => x.astDate).ToList();

                    // (4) CURRENT PLAN
                    List<AlGsdCurpln> rCurrentPlanOfModel = rCurrentPlan.Where(x => x.Model == oModel && x.Prdym == ym).ToList();
                    List<int> rWcno = rCurrentPlan.Where(x => x.Model == oModel).Select(x => x.Wcno).ToList();
                    foreach (int oWcno in rWcno)
                    {
                        AlGsdCurpln oCurrentPlan = rCurrentPlan.FirstOrDefault(x => x.Model == oModel && x.Wcno == oWcno);
                        if (oCurrentPlan != null)
                        {
                            oResponse.listCurpln.Add(oCurrentPlan);
                        }
                    }

                    // --------- CHECK IF HAVE (PLAN,SALE,INV) IS SHOW  --------//
                    //if (rSaleForecase.Count > 0 && rInventory.Count > 0 && oResponse.listCurpln.Count > 0)
                    //{

                    // (6) INVENTORY PDT
                    oResponse.listPDTInventory = oPDTInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();

                    // (7) INVENTORY PLANNING
                    List<MLastInventory> rLastInventory = oLastInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();
                    double nLastInventory = (rLastInventory.Count > 0) ? Convert.ToDouble(rLastInventory.FirstOrDefault().balstk) : 0;
                    oResponse.LastInventory = nLastInventory;

                    // (##) GROUP PLTYPE OF INVENTORY 
                    List<MCntOfPltype> GroupPltype = allInventory.Where(x => x.model == oModel.Trim()).ToList().Select(g => new MCntOfPltype()
                    {
                        pltype = g.pltype,
                        cnt = Convert.ToInt32(g.cnt)
                    }).ToList();

                    // (##) CAL DELIVERY OF DAY
                    var PalletsOfModel = ListPalletOfModel.Where(x => x.RefCode == oModel).GroupBy(x => new
                    {
                        model = x.RefCode,
                        pallet = x.Ref1
                    }).Select(o => new { o.Key.pallet }).ToList();
                    foreach (var oPallet in PalletsOfModel)
                    {
                        string pallet = oPallet.pallet;
                        //string customer = oPallet.customer;
                        DateTime dateDelivery = dtFilter;
                        MDelivery oDelivery = new MDelivery();
                        oDelivery.pltype = pallet;
                        oDelivery.customer = "";
                        bool isValDelivery = false;
                        while (dateDelivery.Date < new DateTime(dtFilter.Year, dtFilter.Month, DateTime.DaysInMonth(dtFilter.Year, dtFilter.Month)).AddDays(1))
                        {
                            string strDtDelivery = dateDelivery.ToString("yyyyMMdd");
                            MData iDelivery = new MData();
                            iDelivery.date = strDtDelivery;
                            List<MOSW03Delivery> itemDelivery = mOSW03Deliveries.Where(x => x.model == oModel && x.pltype == pallet && x.deldate == strDtDelivery).ToList();
                            if (itemDelivery.Count > 0)
                            {
                                iDelivery.value = itemDelivery.FirstOrDefault()!.qty;
                                isValDelivery = true;
                            }
                            //iDelivery.customer = customer;
                            oDelivery.data.Add(iDelivery);
                            dateDelivery = dateDelivery.AddDays(1);
                        }
                        if (isValDelivery == true)
                        {
                            rDelivery.Add(oDelivery);
                        }
                    }
                    List<MData> rInventoryPlanning = new List<MData>();
                    List<MData> rSaleAllCusOfModel = new List<MData>();
                    List<MInventory> rInventoryPlanningMainOrFinal = new List<MInventory>();
                    List<InventoryBalance> rInventoryBalance = new List<InventoryBalance>();
                    if (ym == DateTime.Now.ToString("yyyyMM"))
                    {
                        double TotalInventory = allInventory.Where(x => x.model == oModel.Trim()).Sum(x => Convert.ToInt32(x.cnt));
                        var ListPlOfModel = ListPalletOfModel.Where(x => x.RefCode == oModel.Trim()).GroupBy(x => x.Ref1).Select(x => x.Key);
                        foreach (string pallet in ListPlOfModel)  /* [E]A003 */
                        {
                            double TotalInventoryOfPltype = TotalInventory;
                            DateTime dtLoopPltype = DateTime.Now;
                            double nStartInvBalancePltype = Convert.ToDouble(GroupPltype.Where(x => x.pltype == pallet).Sum(y => y.cnt));
                            InventoryBalancePltype oInventoryBalancePltype = new InventoryBalancePltype();
                            oInventoryBalancePltype.pltype = pallet;
                            oInventoryBalancePltype.modelName = oModel.Trim();
                            List<InventoryBalancePltypeData> rInventoryBalancePltypeData = new List<InventoryBalancePltypeData>();
                            while (dtLoopPltype.Date < new DateTime(dtNow.Year, dtNow.Month, DateTime.DaysInMonth(dtNow.Year, dtNow.Month)).AddDays(1))
                            {
                                List<MOSW03Delivery> itemDelivery = mOSW03Deliveries.Where(x => x.model == oModel && x.pltype == pallet && x.deldate == dtLoopPltype.ToString("yyyyMMdd")).ToList();
                                int nDeliveryOfDay = 0;
                                if (itemDelivery.Count > 0)
                                {
                                    nDeliveryOfDay = itemDelivery.Sum(x => x.qty);
                                }
                                double oSaleOfPltypePerDay = rSaleForecase.Where(x => x.Pltype == pallet).Sum(y => Convert.ToDouble(y.GetType().GetProperty("D" + dtLoopPltype.ToString("dd")).GetValue(y).ToString()));
                                nStartInvBalancePltype = (nStartInvBalancePltype - oSaleOfPltypePerDay) + nDeliveryOfDay;
                                rInventoryBalancePltypeData.Add(new InventoryBalancePltypeData()
                                {
                                    date = dtLoopPltype.ToString("yyyyMMdd"),
                                    value = nStartInvBalancePltype
                                });
                                dtLoopPltype = dtLoopPltype.AddDays(1);
                            }
                            oInventoryBalancePltype.data = rInventoryBalancePltypeData;
                            oResponse.InventoryBalancePltype.Add(oInventoryBalancePltype);
                        }
                        DateTime dtCalInvBal = DateTime.Now; // เริ่มจากวันปัจจุบัน
                        while (dtCalInvBal.Date < new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).AddDays(1))
                        {
                            InventoryBalance iInventoryBalance = new InventoryBalance();
                            double iSaleOfDay = rSaleForecase.Sum(x => int.Parse(x.GetType().GetProperty("D" + dtCalInvBal.ToString("dd")).GetValue(x).ToString()));
                            List<MOSW03Delivery> itemDelivery = mOSW03Deliveries.Where(x => x.model == oModel && x.deldate == dtCalInvBal.ToString("yyyyMMdd")).ToList();
                            int nDeliveryOfDay = 0;
                            if (itemDelivery.Count > 0)
                            {
                                nDeliveryOfDay = itemDelivery.Sum(x => x.qty);
                            }
                            TotalInventory = (TotalInventory - iSaleOfDay) + nDeliveryOfDay;
                            iInventoryBalance.value = TotalInventory;
                            iInventoryBalance.date = dtCalInvBal.ToString("yyyyMMdd");
                            rInventoryBalance.Add(iInventoryBalance);
                            dtCalInvBal = dtCalInvBal.AddDays(1);
                        }
                    }


                    DateTime dtLoop = dtNow;
                    double sumAllInbound = 0; // ผลรวม Inbound ประจำวัน
                    List<MInbound> resInbound = new List<MInbound>();
                    double nInvPln = nLastInventory;


                    // (##) CAL INBOUND
                    DateTime dtStartWarning = dtNow;
                    DateTime dtEndWarning = dtNow.AddDays(10);
                    while (dtLoop.Date <= new DateTime(dtNow.Year, dtNow.Month, DateTime.DaysInMonth(dtNow.Year, dtNow.Month)))
                    {
                        string d = dtLoop.ToString("dd");
                        string ymdInbound = $"{ym}{d}";
                        double sumDayInbound = 0;
                        foreach (MInbound oInbound in rInbound.Where(x => x.astDate.Replace("-", "") == ymdInbound).ToList())
                        {
                            if (oInbound.astType == "IN")
                            {
                                sumDayInbound += oInbound.astQty;
                                sumAllInbound += oInbound.astQty;
                            }
                            else
                            {
                                sumDayInbound -= oInbound.astQty;
                                sumAllInbound -= oInbound.astQty;
                            }
                        }
                        resInbound.Add(new MInbound()
                        {
                            astDate = ymdInbound,
                            astQty = sumDayInbound
                        });
                        dtLoop = dtLoop.AddDays(1);
                    }

                    // (##) CAL INV.PLN PER DAY

                    /* A002 */
                    decimal? nStockCurrent = 0;
                    EkbWipPartStock oInventoryPrevMonth = listEkbInventoryMain.FirstOrDefault(x => x.Partno == oModel.Trim() && x.Wcno == "999");
                    decimal? nInventory = oInventoryPrevMonth != null ? oInventoryPrevMonth.Bal : 0;

                    DateTime dtStartInvPln = new DateTime(int.Parse(year), int.Parse(month), 1);
                    DateTime dtEndInvPln = new DateTime(dtStartInvPln.Year, dtStartInvPln.Month, DateTime.DaysInMonth(dtStartInvPln.Year, dtStartInvPln.Month));
                    while (dtStartInvPln <= dtEndInvPln)
                    {
                        decimal nSaleOfDay = rSaleForecase.Where(x => x.Ym == ym).Sum(x => int.Parse(x.GetType().GetProperty("D" + dtStartInvPln.ToString("dd")).GetValue(x).ToString()));
                        /* A002[S] */
                        decimal nResultPrevDay = 0;
                        try
                        {
                            if (dtStartInvPln.ToString("dd") != "01")
                            {
                                int removeDay = (dtStartInvPln.ToString("yyyyMMdd") == dtEndInvPln.ToString("yyyyMMdd")) ? 0 : -1;
                                nResultPrevDay = modelGroup != "ODM" ? rMainResult.Where(o => o.shiftDate == dtStartInvPln.AddDays(-1).ToString("yyyy-MM-dd")).Sum(x => x.cnt) : rResultFinal.Where(x => x.model == oModel && x.prdymd == dtStartInvPln.AddDays(-1).ToString("yyyyMMdd")).Sum(x => x.qty);
                            }
                        }
                        catch
                        {
                            nResultPrevDay = 0;
                        }
                        nInventory = (nInventory - nSaleOfDay) + nResultPrevDay;
                        rInventoryPlanningMainOrFinal.Add(new MInventory()
                        {
                            date = dtStartInvPln.ToString("yyyyMMdd"),
                            model = oModel,
                            cnt = nInventory.ToString(),
                            pltype = ""
                        });
                        if (dtStartInvPln.ToString("yyyyMMdd") == dtEndInvPln.ToString("yyyyMMdd"))
                        {
                            nStockCurrent = nInventory + (modelGroup != "ODM" ? rMainResult.Where(o => o.shiftDate == dtStartInvPln.ToString("yyyy-MM-dd")).Sum(x => x.cnt) : rResultFinal.Where(x => x.model == oModel && x.prdymd == dtStartInvPln.ToString("yyyyMMdd")).Sum(x => x.qty));
                        }
                        /* A002[E] */

                        // (5) INVENTORY HOLD
                        if (dtStartInvPln.Date == DateTime.Now.Date)
                        {
                            oResponse.listHoldInventory = rInvHoldOfModel.ToList();
                        }
                        string d = dtStartInvPln.ToString("dd");
                        MData oInvPlnOfDay = new MData();
                        double rSaleOfDay = 0;
                        oInvPlnOfDay.date = $"{ym}{dtStartInvPln.ToString("dd")}";
                        if (d == "01")
                        {
                            nInvPln -= rSaleOfDay;
                            oInvPlnOfDay.value = nInvPln;
                        }
                        else
                        {
                            int dayPrev = dtStartInvPln.AddDays(-1).Day; // วันที่ loop - 1 day
                            double nCurPlnOfPrevDay = rCurrentPlanOfModel.Sum(x => double.Parse(x.GetType().GetProperty("Day" + dayPrev.ToString("D2")).GetValue(x).ToString())); // ตัวเลขแผนผลิต -1d 
                            int nInvHold = 0;

                            if ($"{ym}{dtStartInvPln.ToString("dd")}" == dtNow.ToString("yyyyMM"))
                            {
                                List<MHoldInventory> rInvHoldOfDay = oHoldInventory.Where(x => x.model.Trim() == oModel).ToList();
                                if (rInvHoldOfDay.Count > 0 && rInvHoldOfDay.FirstOrDefault() != null)
                                {
                                    nInvHold = rInvHoldOfDay.FirstOrDefault()?.balstk != "" ? int.Parse(rInvHoldOfDay.FirstOrDefault()?.balstk) : 0;
                                }
                            }

                            nInvPln = (nInvPln + nCurPlnOfPrevDay + nInvHold) - (double)nSaleOfDay;
                            oInvPlnOfDay.value = nInvPln;
                        }
                        dtStartInvPln = dtStartInvPln.AddDays(1);
                        rInventoryPlanning.Add(oInvPlnOfDay);
                        oResponse.totalInventoryPlanning = nInvPln;
                    }

                    if (oResponse.warning == true)
                    {
                        DateTime dtLoopWarning = dtStart;
                        int YearWarning = int.Parse(rYM.LastOrDefault().Substring(0, 4));
                        int MonthWarning = int.Parse(rYM.LastOrDefault().Substring(4, 2));
                        while (dtLoopWarning.Date < new DateTime(YearWarning, MonthWarning, DateTime.DaysInMonth(YearWarning, MonthWarning)).AddDays(1))
                        {

                            // (##) SUM SALE OF DAY
                            MData SaleAllCusOfDay = new MData(); // แผนการขายต่อวัน รวมทุกลูกค้า
                            SaleAllCusOfDay.date = dtLoopWarning.ToString("yyyyMMdd");
                            List<AlSaleForecaseMonth> test = rSaleForeCaseAlLCustomer.Where(x => x.ModelName == oModel && x.Ym == dtLoopWarning.ToString("yyyyMM")).ToList();
                            SaleAllCusOfDay.value = rSaleForeCaseAlLCustomer.Where(x => x.ModelName == oModel && x.Ym == dtLoopWarning.ToString("yyyyMM")).ToList().Sum(x => int.Parse(x.GetType().GetProperty("D" + dtLoopWarning.ToString("dd")).GetValue(x).ToString()));
                            rSaleAllCusOfModel.Add(SaleAllCusOfDay);
                            dtLoopWarning = dtLoopWarning.AddDays(1);
                        }
                    }
                    /* A002[S] */
                    oResponse.listInventoryPlanningMainOrFinal = rInventoryPlanningMainOrFinal; /* FIND LIST NUMBER (STOCK [LAST MONTH] - SALE [OF DAY]) + RESULT [ODM = FINAL, MAIN] */
                    /* A002[S] ==> FIND  NUMBER STOCK CURRENT OF THIS MONTH */
                    oResponse.inventoryPlanningMainOrFinalEnd = nStockCurrent;
                    /* A002[E] */

                    // (##) SET DELIVERY
                    oResponse.listDelivery = rDelivery;

                    // (##) SET SALE ALL CUSTOMER FOR WARNING COMPONENT
                    oResponse.listSaleForeCaseAllCustomer = rSaleAllCusOfModel;

                    // (##) SET INVENTORY PLANNING
                    oResponse.listInventoryPlanning = rInventoryPlanning;
                    // (##) SET LIST INBOUND
                    oResponse.listInbound = resInbound;

                    // (##) SET INVENTORY BALANCE 
                    oResponse.InventoryBalance = rInventoryBalance;

                    oResponse.listActMain = rMainResult;

                    // (9) INVENTORY MAIN 
                    oResponse.LastInventoryMain = listEkbInventoryMain.FirstOrDefault(x => x.Partno == oModel.Trim() && x.Wcno == "999");

                    // (10) RESULT FINAL 
                    List<AlGsdActpln> itemFinal = new List<AlGsdActpln>();
                    foreach (int oWcno in rWcno)
                    {
                        var plans = rResultFinal.Where(x => x.wcno == oWcno.ToString() && x.model == oModel).ToList();
                        foreach (var itemPlan in plans)
                        {
                            itemFinal.Add(new AlGsdActpln()
                            {
                                Model = oModel,
                                Wcno = oWcno,
                                Qty = itemPlan.qty,
                                Prdymd = itemPlan.prdymd!.Substring(itemPlan.prdymd.Length - 2),
                            });
                        }
                    }

                    // (11) GET SBU (AREATYPE) OF SBU ARRAY
                    oResponse.sbu = "";
                    GstSalMdl oSBU = rGstSalMdl.FirstOrDefault(z => z.modelName == oModel.Trim());
                    if (oSBU != null)
                    {
                        oResponse.sbu = oSBU.sku;
                    }
                    oResponse.listActFinal = itemFinal;
                    response.Add(oResponse);
                    //}
                }
            }

            return Ok(new
            {
                content = response,
                modeltype = rModelType
            });
        }


        //[HttpGet]
        //[Route("/GRAFANA_SUMMARY_INVENTORY_AND_SALE")]
        //public IActionResult GRAFANA_SUMMARY_INVENTORY_AND_SALE()
        //{
        //    List<PnCompressor> listModel = _DBSCM.PnCompressors.Where(x => x.Status == "ACTIVE").ToList();
        //    List<string> rGroupType = listModel.Select(x => serv.getModelGroup(x.Model)).Distinct().ToList();
        //    return Ok();
        //}

        //[HttpGet]
        //[Route("/warning/get/{ym}/{inventorytype}")]
        [HttpGet]
        [Route("/warning/get")]
        public IActionResult GetWarningData()
        {
            // (##) สร้างตัวแปร
            // (##) เก็บข้อมูลการขาย
            DateTime dtNow = DateTime.Now;
            List<MWarning> res = new List<MWarning>();
            DateTime dtStartWarning = DateTime.Now;
            DateTime dtEndWarning = dtStartWarning.AddDays(10);
            string yNow = dtStartWarning.ToString("yyyy");
            string mNow = dtStartWarning.ToString("yyyy");
            List<string> rYM = new List<string>();
            if (dtStartWarning.Month != dtEndWarning.Month) // เช็คว่า เดือนเริ่ม เดือนสิ้นสุด ต่างกันหรือไม่ ? สำหรับ where table schema D1-31
            {
                rYM.Add(dtStartWarning.ToString("yyyyMM"));
                rYM.Add(dtEndWarning.ToString("yyyyMM"));
            }
            else
            {
                rYM.Add(dtStartWarning.ToString("yyyyMM"));
            }
            // (##) 

            // (##) GET <IN> WH
            List<MInbound> rInbound = serv.GetInbound(dtStartWarning, dtEndWarning);
            // (##)

            // (##) GET MASTER
            string ymd = "";
            if (DateTime.Now.Hour < 8)
            {
                ymd = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                ymd = DateTime.Now.ToString("yyyyMMdd");
            }


            List<MHoldInventory> oHoldInventory = new List<MHoldInventory>();
            OracleCommand strHoldInventory = new OracleCommand();
            strHoldInventory.CommandText = @"SELECT W.YM ,
   W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
FROM SE.WMS_STKBAL W
WHERE comid= 'DCI' and ym = :YM
  and wc in ('HWH','RWQ')
and balstk > 0
GROUP BY W.YM,  W.MODEL";
            // RWQ
            strHoldInventory.Parameters.Add(new OracleParameter(":YM", DateTime.Now.ToString("yyyyMM")));
            DataTable dtHoldInventory = _ALPHAPD.Query(strHoldInventory);
            foreach (DataRow dr in dtHoldInventory.Rows)
            {
                MHoldInventory iLastInventory = new MHoldInventory();
                iLastInventory.ym = dr["YM"].ToString();
                iLastInventory.model = dr["MODEL"].ToString();
                iLastInventory.balstk = dr["BALSTK"].ToString();
                iLastInventory.lbalstk = dr["LBALSTK"].ToString();
                oHoldInventory.Add(iLastInventory);
            }


            List<MInventory> rInventory = new List<MInventory>();

            List<MOSW03Delivery> mOSW03Deliveries = new List<MOSW03Delivery>();
            OracleCommand strGetDelivery = new OracleCommand();
            strGetDelivery.CommandText = $@"SELECT TO_CHAR(H.DELDATE, 'yyyyMMdd') DELDATE,  
                                               W.MODEL, W.PLTYPE,   
                                               SUM(W.QTY) QTY, 
                                               SUM(W.ALQTY) ALQTY,   
                                               SUM(W.PICQTY) PICQTY   
                                            FROM SE.WMS_DELCTN W
                                            LEFT JOIN SE.WMS_DELCTL H ON H.COMID='DCI' AND H.IVNO = W.IVNO AND H.DONO = W.DONO 
                                            WHERE W.CFBIT = 'F' AND W.IFBIT = 'F' AND TO_CHAR(H.DELDATE, 'yyyyMMdd') LIKE '{dtNow.Year}{dtNow.ToString("MM")}%'
                                            GROUP BY TO_CHAR(H.DELDATE, 'yyyyMMdd') , W.MODEL, W.PLTYPE   ";
            DataTable dtDelivery = _ALPHAPD.Query(strGetDelivery);
            foreach (DataRow dr in dtDelivery.Rows)
            {
                MOSW03Delivery oSW03Delivery = new MOSW03Delivery();
                oSW03Delivery.model = dr["MODEL"].ToString().Trim();
                oSW03Delivery.pltype = dr["PLTYPE"].ToString().Trim();
                //oSW03Delivery.cfdate = dr["CFDATE"].ToString();
                //oSW03Delivery.ifdate = dr["IFDATE"].ToString();
                oSW03Delivery.deldate = dr["DELDATE"].ToString();
                oSW03Delivery.qty = Convert.ToInt32(dr["QTY"].ToString());
                oSW03Delivery.alqty = Convert.ToInt32(dr["ALQTY"].ToString());
                oSW03Delivery.picqty = Convert.ToInt32(dr["PICQTY"].ToString());
                mOSW03Deliveries.Add(oSW03Delivery);
            }

            SqlCommand strGetUkeAlphaInv = new SqlCommand();
            strGetUkeAlphaInv.CommandText = @"SELECT * FROM UKE_ALPHA_INVENTORY WHERE YMD = @YMD";
            strGetUkeAlphaInv.Parameters.Add(new SqlParameter("@YMD", ymd));
            DataTable dtInv = _SQLSCM.Query(strGetUkeAlphaInv);
            foreach (DataRow dr in dtInv.Rows)
            {
                MInventory item = new MInventory();
                item.cnt = dr["CNT"].ToString() != "" ? dr["CNT"].ToString() : "0";
                item.model = dr["MODEL"].ToString();
                rInventory.Add(item);
            }
            List<GstSalMdl> rSKU = serv.GetSKU();
            List<PnCompressor> rModel = serv.getModels();
            /* A001 */
            int rev = 0;
            int lrev = 0;
            SqlCommand sqlCheckVersion = new SqlCommand();
            sqlCheckVersion.CommandText = @"SELECT TOP(1) REV,LREV FROM [dbSCM].[dbo].[AL_SaleForecaseMonth] WHERE ym LIKE '" + yNow + "%'   order by CAST(rev as int) desc , CAST(lrev as int) desc";
            DataTable dtGetVersion = _SQLSCM.Query(sqlCheckVersion);
            if (dtGetVersion.Rows.Count > 0)
            {

                rev = Convert.ToInt32(dtGetVersion.Rows[0]["REV"].ToString());
                lrev = Convert.ToInt32(dtGetVersion.Rows[0]["LREV"].ToString());
                if (lrev != 999)
                {
                    //rev = lrev - 1;
                    rev = rev - 1;
                }
            }
            List<AlSaleForecaseMonth> rSaleForecase = _DBSCM.AlSaleForecaseMonths.Where(x => x.Rev == rev.ToString() && x.Lrev == lrev.ToString() && rYM.Contains(x.Ym)).ToList();
            /* A001 */

            List<WmsMdw27ModelMaster> rModelDetail = _DBSCM.WmsMdw27ModelMasters.Where(x => x.Active == "ACTIVE").ToList();
            // (##)
            List<string> Models = rModelDetail.Select(x => x.Model).Distinct().ToList();

            foreach (string oModel in Models)
            {

                bool warning = false;
                List<string> rCustomer = new List<string>();
                List<string> rPltype = new List<string>();
                MWarning item = new MWarning();
                string model = oModel.Trim();
                var sku = rSKU.FirstOrDefault(x => x.modelName == model);
                var oModelDetail = rModelDetail.FirstOrDefault(x => x.Model == model);
                string sebango = oModelDetail != null ? oModelDetail.Sebango : "";
                item.model = model;
                item.sbu = sku != null ? sku.sku : "";
                item.sebango = sebango;
                DateTime dtLoop = dtStartWarning;
                string ymdLoop = dtLoop.ToString("yyyyMMdd");
                List<MInventory> oInventory = rInventory.Where(x => x.model.Trim() == model).ToList(); // (##) find Inventory of day by field model, date
                double nInventory = oInventory.Count > 0 ? oInventory.Sum(x => double.Parse(x.cnt)) : 0;
                MOSW03Delivery oItemDelivery = mOSW03Deliveries.FirstOrDefault(x => x.model == model && x.deldate == dtNow.ToString("yyyyMMdd"));
                if (oItemDelivery != null)
                {
                    nInventory += oItemDelivery.qty;
                }

                // ============ CEHCK HOLD =================//
                MHoldInventory oHold = oHoldInventory.FirstOrDefault(x => x.model == model);
                if (oHold != null)
                {
                    item.hold = helper.ConvStrToDB(oHold.balstk);
                }

                // ---------- CHECK HAVE INBOUND  ------------//
                if (model == "1YC25DXD#A")
                {
                    Console.WriteLine("asdd");
                }
                List<MInbound> ItemInBound = rInbound.Where(x => x.model == model).ToList();
                foreach (MInbound oInbound in ItemInBound)
                {
                    if (oInbound.astType == "IN")
                    {
                        nInventory = nInventory + oInbound.astQty;
                    }
                    else
                    {
                        nInventory = nInventory - oInbound.astQty;
                    }
                }
                double? resultInbound = ItemInBound.Where(x => x.astType == "IN").Sum(x => x.astQty) - ItemInBound.Where(x => x.astType == "OUT").Sum(x => x.astQty);
                item.inbound = Math.Abs(resultInbound.Value);
                item.inboundType = resultInbound.Value >= 0 ? "IN" : "OUT";

                // --------------------- END ------------------//

                item.inventory = nInventory;
                List<MPltypeOfCustomer> rPltypeOfCustomer = new List<MPltypeOfCustomer>();
                while (dtLoop < dtEndWarning)
                {
                    MData mSale = new MData(); // Sale of day
                    MData mInventory = new MData(); // Inv:accu - Sale of day

                    List<AlSaleForecaseMonth> rSale = rSaleForecase.Where(x => x.ModelName == model).ToList();
                    List<string> rCustomerOfSalePerDay = rSale.Select(x => x.Customer).Distinct().ToList();
                    double nSumSaleOfCustomerPerDay = 0;
                    foreach (string customer in rCustomerOfSalePerDay)
                    {
                        List<AlSaleForecaseMonth> oSale = rSale.Where(x => x.Customer == customer).ToList();
                        double nSale = oSale.Where(x => x.Ym == dtLoop.ToString("yyyyMM") && x.Customer == customer).Sum(x => int.Parse(x.GetType().GetProperty("D" + dtLoop.ToString("dd")).GetValue(x).ToString()));
                        //if (nSale > 0)
                        //{
                        nSumSaleOfCustomerPerDay = nSumSaleOfCustomerPerDay + nSale;
                        rCustomer.Add(customer);
                        rPltype.Add(oSale.Select(x => x.Pltype).FirstOrDefault());
                        MPltypeOfCustomer oPltypeOfCustomer = new MPltypeOfCustomer();
                        int index = rPltypeOfCustomer.FindIndex(x => x.customer == customer);
                        if (index < 0)
                        {
                            oPltypeOfCustomer.customer = customer;
                            oPltypeOfCustomer.pltype.Add(oSale.Select(x => x.Pltype).FirstOrDefault());
                            rPltypeOfCustomer.Add(oPltypeOfCustomer);
                        }
                        else
                        {
                            rPltypeOfCustomer[index].pltype.Add(oSale.Select(x => x.Pltype).FirstOrDefault());
                            rPltypeOfCustomer[index].pltype = rPltypeOfCustomer[index].pltype.Distinct().ToList();
                        }
                        //}
                    }
                    mSale.date = dtLoop.ToString("yyyyMMdd");
                    mSale.value = nSumSaleOfCustomerPerDay;
                    //MOSW03Delivery oItemDelivery = mOSW03Deliveries.FirstOrDefault(x => x.model == item.model && x.ifdate == dtNow.ToString("yyyyMMdd"));
                    //if (oItemDelivery == null)
                    //{
                    nInventory = nInventory - nSumSaleOfCustomerPerDay;
                    //}
                    //else
                    //{
                    //    Console.WriteLine("X");
                    //}
                    if (nInventory < 0)
                    {
                        warning = true;
                        // GET DATA SALE BY CUSTOMER && PLTYPE FOR EXPORT TO EXCEL
                        List<MWarningExcel> rExcelData = new List<MWarningExcel>();
                        foreach (string customer in rCustomerOfSalePerDay)
                        {
                            MWarningExcel oExcelData = new MWarningExcel();
                            oExcelData.model = model;
                            oExcelData.customer = customer;
                            foreach (string pltype in rSale.Where(x => x.Customer == customer).Select(x => x.Pltype).Distinct().ToList())
                            {
                                oExcelData.pltype = pltype;
                                List<MData> rItemExcel = new List<MData>();
                                DateTime dtLoopWarning = dtStartWarning;
                                while (dtLoopWarning < dtEndWarning)
                                {
                                    MData oItemExcel = new MData();
                                    int nSale = rSale.Where(x => x.Ym == dtLoopWarning.ToString("yyyyMM") && x.Customer == customer && x.Pltype == pltype).Sum(x => int.Parse(x.GetType().GetProperty("D" + dtLoopWarning.ToString("dd")).GetValue(x).ToString()));
                                    oItemExcel.date = dtLoopWarning.ToString("ddMMyyyy");
                                    oItemExcel.value = nSale;
                                    oExcelData.data.Add(oItemExcel);
                                    dtLoopWarning = dtLoopWarning.AddDays(1);
                                }
                            }
                            rExcelData.Add(oExcelData);

                        }
                        item.listSaleExcel = rExcelData;

                        // END
                    }
                    mInventory.date = dtLoop.ToString("yyyyMMdd");
                    mInventory.value = nInventory;
                    item.listSale.Add(mSale);
                    item.listInventory.Add(mInventory);
                    dtLoop = dtLoop.AddDays(1);
                }
                if (warning == true)
                {
                    item.total = nInventory;
                    res.Add(item);
                    item.customer = rCustomer.Distinct().ToList();
                    item.pltype = rPltypeOfCustomer;
                }
            }
            List<MWarning> resSort = new List<MWarning>();
            List<MSBUSort> rSBUSort = new List<MSBUSort>();
            SqlCommand sqlGetDictSort = new SqlCommand();
            sqlGetDictSort.CommandText = @"SELECT Dict_Name FROM [dbSCM].[dbo].[UKE_Dict] ORDER BY cast(Dict_RefCode as int) asc";
            DataTable dtSort = _SQLSCM.Query(sqlGetDictSort);
            foreach (DataRow dr in dtSort.Rows)
            {
                MSBUSort item = new MSBUSort();
                item.Dict_Name = dr["Dict_Name"].ToString().Replace(Environment.NewLine, "");
                rSBUSort.Add(item);
            }
            foreach (MSBUSort oSort in rSBUSort)
            {
                List<MWarning> rFindSBUSort = res.Where(x => x.sbu == oSort.Dict_Name.Replace("\\r\n", "")).ToList();
                resSort = resSort.Concat(rFindSBUSort).ToList();
            }
            return Ok(resSort);
        }


        [HttpPost]
        [Route("/update_inventory_main")]
        public IActionResult UpdateInventoryMain([FromBody] MUpdateInventoryMain param)
        {
            string ym = param.ym;
            string empcode = param.empcode;
            List<InventoryMain> data = param.data;
            try
            {
                foreach (InventoryMain item in data)
                {
                    EkbWipPartStock oEkb = _DBSCM.EkbWipPartStocks.FirstOrDefault(x => x.Partno == item.model.Trim() && x.Ym == ym && x.Ptype == "MAIN");
                    if (oEkb != null)
                    {
                        oEkb.Bal = item.value;
                        oEkb.UpdateDate = DateTime.Now;
                        oEkb.UpdateBy = empcode;
                        _DBSCM.Update(oEkb);
                    }
                    else
                    {
                        EkbWipPartStock newEkb = new EkbWipPartStock();
                        newEkb.Ym = ym;
                        newEkb.Wcno = "999";
                        newEkb.Cm = "";
                        newEkb.PartDesc = "";
                        newEkb.Partno = item.model;
                        newEkb.Lbal = 0;
                        newEkb.Recqty = 0;
                        newEkb.Issqty = 0;
                        newEkb.Bal = item.value;
                        newEkb.UpdateBy = empcode;
                        newEkb.UpdateDate = DateTime.Now;
                        newEkb.Ptype = "MAIN";
                        _DBSCM.Add(newEkb);
                    }
                }
                int action = _DBSCM.SaveChanges();
                return Ok(new
                {
                    status = action
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    status = false
                });
            }
        }

        [HttpPost]
        [Route("/chart")]
        public IActionResult Chart([FromBody] MChartParam param)
        {
            string ym = param.ym;
            int yyyy = Convert.ToInt16(ym.Substring(0, 4));
            int m = Convert.ToByte(ym.Substring(4, 2));
            List<MChart> rData = new List<MChart>();
            List<List<string>> rProdType = new List<List<string>>();
            rProdType.Add(new List<string>() { "1YC", "2YC", "SCR" });
            rProdType.Add(new List<string>() { "ODM" });
            List<MStyleChartOfCustomer> rStyle = new List<MStyleChartOfCustomer>()
            {
                new MStyleChartOfCustomer()
                {
                    customer = "DAM",
                    backgroundColor = "#f44336ab",
                    //borderColor = "#f44336"

                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DIT",
                    backgroundColor = "#009688ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DAV",
                    backgroundColor = "#795548ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DAMA",
                    backgroundColor = "#e91e63ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DAIPL",
                    backgroundColor = "#4caf50ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DIL",
                    backgroundColor = "#9e9e9eab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DMMX",
                    backgroundColor = "#9c27b0ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DAP",
                    backgroundColor = "#8bc34aab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DTL-T",
                    backgroundColor = "#607d8bab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "SDS",
                    backgroundColor = "#673ab7ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DICZ",
                    backgroundColor = "#cddc39ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DDC",
                    backgroundColor = "#3f51b5ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DNA",
                    backgroundColor = "#ffeb3bab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DRDM",
                    backgroundColor = "#2196f3ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DMS",
                    backgroundColor = "#ffc107ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DTAS",
                    backgroundColor = "#03a9f4ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DAA",
                    backgroundColor = "#ff9800ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DENV",
                    backgroundColor = "#00bcd4ab",
                    //borderColor = "#f44336"
                },
                  new MStyleChartOfCustomer()
                {
                    customer = "DSZ",
                    backgroundColor = "#ff5722ab",
                    //borderColor = "#f44336"
                }
            };
            List<WmsStkBal> rInventory = serv.MWSGetInventory(yyyy, m);
            List<GstSalMdl> rSKUData = serv.GetSKU();
            MSaleVersion oSaleVersion = serv.getSaleVersion(yyyy.ToString());
            List<AlSaleForecaseMonth> rSaleForecase = _DBSCM.AlSaleForecaseMonths.FromSqlRaw($@"SELECT  * FROM [dbSCM].[dbo].[AL_SaleForecaseMonth] WHERE YM = '{ym}'  AND LREV = '{oSaleVersion.lrev}' AND REV = '{oSaleVersion.rev}' AND  ISNUMERIC(CUSTOMER) = 0 AND CUSTOMER != ''").ToList();
            //List<AlSaleForecaseMonth> rSaleForecase = _DBSCM.AlSaleForecaseMonths.Where(x => x.Lrev == oSaleVersion.lrev.ToString() && x.Rev == oSaleVersion.rev.ToString() && x.Ym == ym).ToList();
            List<AlGsdCurpln> rPlan = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();
            foreach (List<string> oProdType in rProdType)
            {
                #region STOCK CHART
                // (##) INIT CHART STOCK
                MChartSale rChartStock = new MChartSale();
                List<MChartDataSet> rChartStockDataSet = new List<MChartDataSet>();
                List<string> rChartLabelStock = new List<string>();
                List<MChartModelWithStock> rModelWithStock = (from sku in rSKUData
                                                              join s in rInventory
                                                              on sku.modelName.Trim() equals s.model.Trim() into oSKU
                                                              from oInv in oSKU.DefaultIfEmpty()
                                                              select new MChartModelWithStock()
                                                              {
                                                                  modelName = sku.modelName,
                                                                  sku = sku.sku,
                                                                  sum = oInv != null ? Convert.ToInt32(oInv.balstk) : 0,
                                                              }).Where(x => x.sku != "" && oProdType.Contains(serv.getModelGroup(x.modelName))).GroupBy(x => new
                                                              {
                                                                  x.sku,
                                                              }).Select(g => new MChartModelWithStock()
                                                              {
                                                                  sku = g.Key.sku,
                                                                  sum = g.Sum(s => s.sum)
                                                              }).OrderBy(x => x.sku).ToList();
                rChartStock.label = rModelWithStock.OrderBy(x => x.sku).Select(x => x.sku).ToList();

                //List<MChartModelWithStock> rModelWithStock = (from s in rInventory
                //                                              join sku in rSKUData
                //                                              on s.model.Trim() equals sku.modelName.Trim() into aaa
                //                                              from bbb in aaa.DefaultIfEmpty()
                //                                              select new MChartModelWithStock()
                //                                              {
                //                                                  modelName = s.model,
                //                                                  sku = bbb != null ? bbb.sku : "",
                //                                                  sum = Convert.ToInt32(s.lbalstk)
                //                                              }).Where(x => x.sku != "" && oProdType.Contains(serv.getModelGroup(x.modelName))).GroupBy(x => new
                //                                              {
                //                                                  x.sku,
                //                                              }).Select(g => new MChartModelWithStock()
                //                                              {
                //                                                  sku = g.Key.sku,
                //                                                  sum = g.Sum(s => s.sum)
                //                                              }).OrderBy(x => x.sku).ToList();
                //rChartStock.label = rModelWithStock.OrderBy(x => x.sku).Select(x => x.sku).ToList();
                MChartDataSet stockDataSet = new MChartDataSet();
                stockDataSet.label = "Stock";
                foreach (MChartModelWithStock oStock in rModelWithStock)
                {
                    stockDataSet.data.Add(oStock.sum);
                    stockDataSet.backgroundColor = "#54b9af";
                }
                rChartStockDataSet.Add(stockDataSet);
                rChartStock.dataset = rChartStockDataSet;
                rData.Add(new MChart()
                {
                    name = $"STOCK ({String.Join(",", oProdType)})",
                    chart = rChartStock
                });

                #endregion

                #region SALE CHART
                MChartSale rChartSale = new MChartSale();
                // (##) INIT CHART SALE
                List<string> rSKU = rSKUData.Where(x => oProdType.Contains(x.modelGroup)).OrderBy(x => x.sku).GroupBy(x => x.sku).Select(x => x.Key).ToList();
                List<MChartModelWithSale> rModelWithSale = (from oSku in rSKUData
                                                            join oSale in rSaleForecase
                                                            on oSku.modelName.Trim() equals oSale.ModelName.Trim() into mSKU
                                                            from newSKU in mSKU.DefaultIfEmpty()
                                                            select new MChartModelWithSale()
                                                            {
                                                                modelName = oSku.modelName,
                                                                sku = oSku.sku,
                                                                customer = newSKU != null ? newSKU.Customer : "",
                                                                sum = newSKU != null ? (newSKU.D01 + newSKU.D02 + newSKU.D03 + newSKU.D04 + newSKU.D05 + newSKU.D06 + newSKU.D07 + newSKU.D08 + newSKU.D09 + newSKU.D10 + newSKU.D11 + newSKU.D12 + newSKU.D13 + newSKU.D14 + newSKU.D15 + newSKU.D16 + newSKU.D17 + newSKU.D18 + newSKU.D19 + newSKU.D20 + newSKU.D21 + newSKU.D22 + newSKU.D23 + newSKU.D24 + newSKU.D25 + newSKU.D26 + newSKU.D27 + newSKU.D28 + newSKU.D29 + newSKU.D30 + newSKU.D31) : 0
                                                            }).Where(x => x.sku != "" && oProdType.Contains(serv.getModelGroup(x.modelName))).GroupBy(x => new
                                                            {
                                                                x.sku,
                                                                x.customer
                                                            }).Select(g => new MChartModelWithSale()
                                                            {
                                                                sku = g.Key.sku,
                                                                customer = g.Key.customer,
                                                                sum = g.Sum(s => s.sum)
                                                            }).OrderBy(x => x.customer).ToList();
                List<MChartDataSet> rChartDataSet = new List<MChartDataSet>();
                List<string> rCustomer = rModelWithSale.Where(x => x.customer != "").Select(x => x.customer).Distinct().OrderBy(x => x).ToList();
                foreach (string sku in rSKU)
                {

                    foreach (string customer in rCustomer)
                    {
                        List<MChartModelWithSale> oSumOfSKU = rModelWithSale.Where(x => x.sku == sku && x.customer == customer).ToList();
                        if (rChartDataSet.FirstOrDefault(x => x.label == customer) != null)
                        {
                            MChartDataSet oChartSale = rChartDataSet.FirstOrDefault(x => x.label == customer);
                            oChartSale.data.Add(oSumOfSKU.Sum(x => x.sum));
                        }
                        else
                        {
                            MChartDataSet oChartSale = new MChartDataSet();
                            oChartSale.label = customer;
                            oChartSale.data.Add(oSumOfSKU.Sum(x => x.sum));
                            MStyleChartOfCustomer oStyle = rStyle.FirstOrDefault(x => x.customer == customer);
                            if (oStyle != null)
                            {
                                oChartSale.backgroundColor = oStyle.backgroundColor;
                            }
                            else
                            {
                                oChartSale.backgroundColor = "";
                            }
                            rChartDataSet.Add(oChartSale);
                        }
                    }
                }
                rChartSale.label = rSKU;
                rChartSale.dataset = rChartDataSet;

                rData.Add(new MChart()
                {
                    name = $"SALE ({String.Join(",", oProdType)})",
                    chart = rChartSale
                });

                #endregion

                #region PLAN CHART
                List<MChartDataSet> rChartDataSetPlan = new List<MChartDataSet>();
                MChartSale rChartCurPln = new MChartSale();

                List<MChartModelWithPlan> rModelWithPlan = (from sku in rSKUData
                                                            join plan in rPlan
                                                            on sku.modelName equals plan.Model into oSKU
                                                            from oPlan in oSKU.DefaultIfEmpty()
                                                            select new MChartModelWithPlan()
                                                            {
                                                                modelGroup = serv.getModelGroup(sku.modelName),
                                                                sku = sku.sku,
                                                                sum = oPlan != null ? (helper.ConvDecToInt(oPlan.Day01) + helper.ConvDecToInt(oPlan.Day02) + helper.ConvDecToInt(oPlan.Day03) + helper.ConvDecToInt(oPlan.Day04) + helper.ConvDecToInt(oPlan.Day05) + helper.ConvDecToInt(oPlan.Day06) + helper.ConvDecToInt(oPlan.Day07) + helper.ConvDecToInt(oPlan.Day08) + helper.ConvDecToInt(oPlan.Day09) + helper.ConvDecToInt(oPlan.Day10) + helper.ConvDecToInt(oPlan.Day11) + helper.ConvDecToInt(oPlan.Day12) + helper.ConvDecToInt(oPlan.Day13) + helper.ConvDecToInt(oPlan.Day14) + helper.ConvDecToInt(oPlan.Day15) + helper.ConvDecToInt(oPlan.Day16) + helper.ConvDecToInt(oPlan.Day17) + helper.ConvDecToInt(oPlan.Day18) + helper.ConvDecToInt(oPlan.Day19) + helper.ConvDecToInt(oPlan.Day20) + helper.ConvDecToInt(oPlan.Day21) + helper.ConvDecToInt(oPlan.Day22) + helper.ConvDecToInt(oPlan.Day23) + helper.ConvDecToInt(oPlan.Day24) + helper.ConvDecToInt(oPlan.Day25) + helper.ConvDecToInt(oPlan.Day26) + helper.ConvDecToInt(oPlan.Day27) + helper.ConvDecToInt(oPlan.Day28) + helper.ConvDecToInt(oPlan.Day29) + helper.ConvDecToInt(oPlan.Day30) + helper.ConvDecToInt(oPlan.Day31)) : 0
                                                            }
                                                  ).Where(x => x.sku != "" && oProdType.Contains(x.modelGroup)).GroupBy(x => x.sku).Select(g => new MChartModelWithPlan()
                                                  {
                                                      sku = g.Key,
                                                      sum = g.Sum(s => s.sum)
                                                  }).OrderBy(x => x.sku).ToList();
                //List<MChartModelWithPlan> rModelWithPlan = (from plan in rPlan
                //                                            join sku in rSKUData
                //                                            on plan.Model equals sku.modelName into a
                //                                            from aa in a.DefaultIfEmpty()
                //                                            select new MChartModelWithPlan()
                //                                            {
                //                                                modelGroup = serv.getModelGroup(plan.Model),
                //                                                sku = aa != null ? aa.sku : "",
                //                                                sum = (Convert.ToInt32(plan.Day01) + Convert.ToInt32(plan.Day02) + Convert.ToInt32(plan.Day03) + Convert.ToInt32(plan.Day04) + Convert.ToInt32(plan.Day05) + Convert.ToInt32(plan.Day06) + Convert.ToInt32(plan.Day07) + Convert.ToInt32(plan.Day08) + Convert.ToInt32(plan.Day09) + Convert.ToInt32(plan.Day10) + Convert.ToInt32(plan.Day11) + Convert.ToInt32(plan.Day12) + Convert.ToInt32(plan.Day13) + Convert.ToInt32(plan.Day14) + Convert.ToInt32(plan.Day15) + Convert.ToInt32(plan.Day16) + Convert.ToInt32(plan.Day17) + Convert.ToInt32(plan.Day18) + Convert.ToInt32(plan.Day19) + Convert.ToInt32(plan.Day20) + Convert.ToInt32(plan.Day21) + Convert.ToInt32(plan.Day22) + Convert.ToInt32(plan.Day23) + Convert.ToInt32(plan.Day24) + Convert.ToInt32(plan.Day25) + Convert.ToInt32(plan.Day26) + Convert.ToInt32(plan.Day27) + Convert.ToInt32(plan.Day28) + Convert.ToInt32(plan.Day29) + Convert.ToInt32(plan.Day30) + Convert.ToInt32(plan.Day31))
                //                                            }
                //                                    ).Where(x => x.sku != "" && oProdType.Contains(x.modelGroup)).GroupBy(x => x.sku).Select(g => new MChartModelWithPlan()
                //                                    {
                //                                        sku = g.Key,
                //                                        sum = g.Sum(s => s.sum)
                //                                    }).OrderBy(x => x.sku).ToList();
                MChartSale oDataSet = new MChartSale();
                List<MChartDataSet> rDataSet = new List<MChartDataSet>();
                MChartDataSet iDataSet = new MChartDataSet();
                iDataSet.label = "Current Plan";
                foreach (MChartModelWithPlan item in rModelWithPlan)
                {
                    iDataSet.data.Add(item.sum);
                    iDataSet.backgroundColor = "#3b82f6";
                }
                rDataSet.Add(iDataSet);
                oDataSet.label = rModelWithPlan.Select(x => x.sku).OrderBy(x => x).ToList();
                oDataSet.dataset = rDataSet;
                rData.Add(new MChart()
                {
                    name = $"CURRENT PLAN ({String.Join(",", oProdType)})",
                    chart = oDataSet
                });
                #endregion


            }
            return Ok(rData);
        }



        [HttpPost]
        [Route("/wms/mdw12")]
        public IActionResult GetModelDetailMDW12([FromBody] ParamMDW12 param)
        {
            List<PropsMDW12> Models = new List<PropsMDW12>();
            try
            {
                string modelCode = param.modelCode != null ? param.modelCode : "";
                string modelName = param.modelName != null ? param.modelName : "";
                string modelGroup = param.modelGroup != null ? param.modelGroup : "";
                string pdType = param.prdType != null ? param.prdType : "";
                string condModelCode = modelCode != "" ? $" AND POSTCODE = '{(modelCode.Length != 4 ? helper.ConvStrToInt(modelCode).ToString("D4") : "0000")}'" : "";
                string condModelName = modelName != "" ? $" AND MODEL = '{modelName}'" : "";
                string condModelGroup = modelGroup != "" ? $" AND MGROUP LIKE '%{modelGroup}%'" : "";
                string condPrdType = pdType != "" ? $" AND PRDTYPE = '{pdType}'" : "";
                OracleCommand str = new OracleCommand();
                str.CommandText = $@"SELECT  M.POSTCODE ModelCode,M.MODEL ModelName, M.PRDTYPE, M.MGROUP, M.MDBAR, M.MDPRINT, M.Line , CASE WHEN M.PRDTYPE = 'COM' THEN M.Line ELSE 'O' END LineNo FROM SE.MT003 M WHERE M.LREV = '999' {condModelName} {condModelCode} {condModelGroup} {condPrdType}";
                DataTable dt = _ALPHAPD.Query(str);
                foreach (DataRow dr in dt.Rows)
                {
                    PropsMDW12 oModel = new PropsMDW12();
                    oModel.modelCode = dr["ModelCode"].ToString();
                    oModel.modelName = dr["ModelName"].ToString();
                    oModel.prdType = dr["PRDTYPE"].ToString();
                    oModel.modelGroup = dr["MGROUP"].ToString();
                    oModel.modelBarcode = dr["MDBAR"].ToString();
                    oModel.modelPrint = dr["MDPRINT"].ToString();
                    oModel.line = dr["Line"].ToString();
                    Models.Add(oModel);
                }
            }
            catch
            {
                Models = new List<PropsMDW12>();
            }
            return Ok(Models);
        }

        [HttpPost]
        [Route("/wms/mdw27")]
        public IActionResult GetPltypeType([FromBody] ParamMDW27 param)
        {
            List<PropsPltypeOfModel> res = new List<PropsPltypeOfModel>();
            string modelCode = param.modelCode != null ? param.modelCode : "";
            string modelName = param.modelName != null ? param.modelName : "";
            string pdType = param.plType != null ? param.plType : "";
            string condModelCode = modelCode != "" ? $" AND SEBANGO = '{helper.ConvStrToInt(modelCode).ToString("D4")}'" : "";
            string condModelName = modelName != "" ? $" AND MODEL = '{modelName}'" : "";
            string condPrdType = pdType != "" ? $" AND PLTYPE = '{pdType}'" : "";
            SqlCommand str = new SqlCommand();
            str.CommandText = $@"SELECT [SEBANGO] AS MODELCODE,[MODEL] AS MODELNAME ,[MODELGROUP]  ,[PLTYPE]   ,[SEBANGO]  FROM [dbSCM].[dbo].[WMS_MDW27_MODEL_MASTER]  WHERE ACTIVE = 'ACTIVE' AND LREV = '999' {condModelCode} {condModelName} {condPrdType}";
            DataTable dt = _SQLSCM.Query(str);
            foreach (DataRow dr in dt.Rows)
            {
                PropsPltypeOfModel oModel = new PropsPltypeOfModel();
                oModel.modelCode = dr["MODELCODE"].ToString();
                oModel.modelName = dr["MODELNAME"].ToString();
                oModel.plType = dr["PLTYPE"].ToString();
                oModel.modelGroup = dr["MODELGROUP"].ToString();
                res.Add(oModel);
            }
            return Ok(res);
        }

        [HttpPost]
        [Route("/GetIotResult")]
        public IActionResult GetIotResult([FromBody] ParamIotResult param)
        {
            List<WmsMdw27ModelMaster> mdw27s = _DBSCM.WmsMdw27ModelMasters.Where(x => x.Active == "ACTIVE").ToList();
            List<MMainResult> resultMain = new List<MMainResult>();
            string type = param.type;
            string year = param.year;
            string month = param.month;
            string line = param.line;
            string serial = "";
            if (type == "MAIN")
            {
                resultMain = new Service(_DBSCM).GetResultMain(year, month);
                if (line != "%")
                {
                    resultMain = resultMain.Where(x => x.LineName == $"90{line}").ToList();
                }

                foreach (MMainResult oResult in resultMain)
                {
                    if (oResult.ModelName == "")
                    {
                        WmsMdw27ModelMaster oModel = mdw27s.FirstOrDefault(x => x.Sebango == helper.ConvStrToInt(oResult.Model_No).ToString("D4"))!;
                        if (oModel != null)
                        {
                            oResult.ModelName = oModel.Model;
                        }
                    }
                    else if (oResult.Model_No == null || oResult.Model_No == "")
                    {
                        WmsMdw27ModelMaster oModel = mdw27s.FirstOrDefault(x => x.Model == oResult.ModelName)!;
                        if (oModel != null)
                        {
                            oResult.Model_No = oModel.Sebango;
                        }
                    }
                    oResult.Model_No = helper.ConvStrToInt(oResult.Model_No).ToString("D4");
                }
            }
            else
            {
                resultMain = new Service(_DBSCM).GetResultMain(year, month);
            }
            return Ok(resultMain);
        }
    }
}
