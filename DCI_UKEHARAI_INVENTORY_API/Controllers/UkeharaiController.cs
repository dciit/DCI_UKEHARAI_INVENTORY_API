using Azure;
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
        private OraConnectDB _ALPHAPD2 = new OraConnectDB("ALPHA02");
        private SqlConnectDB _SQLSCM = new SqlConnectDB("dbSCM");
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

            List<MActual> response = new List<MActual>();
            List<MInbound> listInbound = new List<MInbound>();
            List<MWms_MstPkm> listPltype = new List<MWms_MstPkm>();
            //set inventory data
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
            strHoldInventory.CommandText = @"SELECT W.YM, W.WC, 
   W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
FROM SE.WMS_STKBAL W
WHERE comid= 'DCI' and ym = :YM
  and wc in ('HWH')
and balstk > 0
GROUP BY W.YM, W.WC, W.MODEL";
            // RWQ
            strHoldInventory.Parameters.Add(new OracleParameter(":YM", (year + "" + month)));
            DataTable dtHoldInventory = _ALPHAPD.Query(strHoldInventory);
            foreach (DataRow dr in dtHoldInventory.Rows)
            {
                MHoldInventory iLastInventory = new MHoldInventory();
                iLastInventory.ym = dr["YM"].ToString();
                iLastInventory.wc = dr["WC"].ToString();
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


            //List<>
            List<PnCompressor> rModel = serv.getModels();
            List<string> rModelType = rModel.Select(x => x.ModelType).ToList();
            List<AlSaleForecaseMonth> ListSaleForecast = _DBSCM.AlSaleForecaseMonths.Where(x => x.Ym == ym && (x.Rev == x.Lrev || x.Lrev == "999")).ToList();
            List<AlGsdCurpln> rCurrentPlan = _DBSCM.AlGsdCurplns.Where(x => x.Prdym == ym).ToList();
            //List<AlGsdActpln> rResultFinal = _DBSCM.AlGsdActplns.Where(x => x.DataType == "9" && x.Prdymd!.Contains(ym)).ToList();
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
            // MANUAL SALE FORECASE
            //            List<MSaleForecasePivot> rSaleForecase = new List<MSaleForecasePivot>();
            //            SqlCommand strSaleForecase = new SqlCommand();
            //            strSaleForecase.CommandText = @"SELECT ModelName,DayStr,QTY,PLTYPE,Customer FROM [dbSCM].[dbo].[AL_SaleForecaseMonth] 
            //unpivot(QTY for DayStr in (D01,D02,D03,D04,D05,D06,D07,D08,D09,D10,D11,D12,D13,D14,D15,D16,D17,D18,D19,D20,D21,D22,D23,D24,D25,D26,D27,D28,D29,D30,D31))
            //unpiv  WHERE  YM = :YM AND LREV = 999";
            //            strSaleForecase.Parameters.Add(new SqlParameter(":YM", ym));
            //            DataTable dtSaleForecase = _SQLSCM.Query(strSaleForecase);
            //            foreach (DataRow dr in dtSaleForecase.Rows)
            //            {
            //                MSaleForecasePivot iSaleForecase = new MSaleForecasePivot();
            //                iSaleForecase.Model = dr["ModelName"].ToString();
            //                iSaleForecase.DayStr = dr["DayStr"].ToString();
            //                iSaleForecase.Qty = Convert.ToDouble(dr["QTY"].ToString());
            //                iSaleForecase.Pltype = dr["PLTYPE"].ToString();
            //                iSaleForecase.Customer = dr["Customer"].ToString();
            //                rSaleForecase.Add(iSaleForecase);
            //            }
            // END

            List<PnCompressor> rModelDetail = _DBSCM.PnCompressors.Where(x => x.Status == "ACTIVE").ToList();

            if (ym != "")
            {
                //List<string> rModel = rCurrentPlan.Select(x => x.Model).Distinct().ToList();
                foreach (string oModel in rModel.Select(o => o.Model).Distinct())
                {
                    string sebango = rModelDetail.FirstOrDefault(x => x.Model == oModel) != null ? rModelDetail.FirstOrDefault(x => x.Model == oModel).ModelCode : "";
                    MActual oResponse = new MActual();
                    oResponse.ym = ym;
                    oResponse.model = oModel;
                    oResponse.sebango = sebango;
                    oResponse.modelCode = sebango;

                    // (1) GET,SET SALE FORECASE
                    List<AlSaleForecaseMonth> rSaleForecase = ListSaleForecast.Where(x => x.ModelName == oModel && x.Ym == ym && x.Lrev == "999").ToList();
                    //&& x.Sebango == sebango
                    if (rSaleForecase != null)
                    {
                        oResponse.listSaleForecast = rSaleForecase;
                    }

                    // (2) GET INVENTORY
                    List<MInventory> rInventory = allInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();
                    if (rInventory != null)
                    {
                        oResponse.Inventory = rInventory;
                    }


                    // (3) INBOUND
                    List<MInbound> rInbound = mInbounds.Where(x => x.model == oModel).OrderBy(x => x.astDate).ToList();
                    oResponse.listInbound = rInbound;


                    // (4) CURRENT PLAN
                    List<int> rWcno = rCurrentPlan.Where(x => x.Model == oModel).Select(x => x.Wcno).ToList();
                    foreach (int oWcno in rWcno)
                    {
                        AlGsdCurpln oCurrentPlan = rCurrentPlan.FirstOrDefault(x => x.Model == oModel && x.Wcno == oWcno);
                        if (oCurrentPlan != null)
                        {
                            oResponse.listCurpln.Add(oCurrentPlan);
                        }
                    }
                    // (5) INVENTORY HOLD
                    oResponse.listHoldInventory = oHoldInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();

                    // (6) INVENTORY PDT
                    oResponse.listPDTInventory = oPDTInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();

                    // (7) INVENTORY PLANNING
                    List<MLastInventory> rLastInventory = oLastInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();
                    double nLastInventory = (rLastInventory.Count > 0) ? Convert.ToDouble(rLastInventory.FirstOrDefault().balstk) : 0;
                    oResponse.LastInventory = nLastInventory;

                    //if (oModel == "2Y147BKBX1A#A")
                    //{
                    //    Console.WriteLine("123");
                    //}


                    // (8) RESULT MAIN
                    oResponse.listActMain = mMainResult.Where(x => x.Model_No == sebango || x.ModelName == oModel.Trim()).ToList();

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
                    oResponse.listActFinal = itemFinal;
                    response.Add(oResponse);
                    //new MActual()
                    //{
                    //    ym = ym,
                    //    //wcno = oWcno!.Value.ToString(),
                    //    wcno = oWcno.ToString(),
                    //    model = oModel,
                    //    sebango = sebango,
                    //    listCurpln = itemCurrentPlan,
                    //    listActPln = itemFinal.OrderBy(x => x.Prdymd).ToList(),
                    //    listActMain = mMainResult.Where(x => x.Model_No == sebango).ToList(),
                    //    listSaleForecast = rSaleForecase,
                    //    listInbound = listInbound.OrderBy(x => x.astDate).ToList(),
                    //    listPltype = listPltype.Where(x => x.model.Trim() == oModel.Trim()).Select(x => x.pltype).ToList(),
                    //    listInventory = listInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList(),
                    //    listLastInventory = oLastInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList(),
                    //    LastInventoryMain = listEkbInventoryMain.FirstOrDefault(x => x.Partno == oModel.Trim() && x.Wcno == oWcno.ToString()),
                    //    LastInventory = nLastInventory,
                    //    listHoldInventory = oHoldInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList(),
                    //}


                    //string sebango = "";
                    //var modelDetail = _DBSCM.PnCompressors.Where(x => x.Model == oModel).FirstOrDefault();
                    //if (modelDetail != null)
                    //{
                    //    sebango = modelDetail.ModelCode;
                    //}

                    //List<AlGsdActpln> itemFinal = new List<AlGsdActpln>();
                    //AlGsdCurpln itemCurrentPlan = new AlGsdCurpln();
                    //AlGsdCurpln resultCurrentPlan = ListCurrentPlan.Where(x => x.Model == oModel).FirstOrDefault(x => x.Wcno == oWcno);
                    //if (resultCurrentPlan != null)
                    //{
                    //    itemCurrentPlan = resultCurrentPlan;
                    //}
                    //var plans = items.Where(x => x.Wcno == oWcno && x.Model == oModel).ToList();
                    //foreach (var itemPlan in plans)
                    //{
                    //    itemFinal.Add(new AlGsdActpln()
                    //    {
                    //        Model = oModel,
                    //        Wcno = oWcno,
                    //        Qty = itemPlan.Qty,
                    //        Prdymd = itemPlan.Prdymd!.Substring(itemPlan.Prdymd.Length - 2),
                    //    });
                    //}
                    //foreach (MInbound oInbound in mInbounds.Where(x => x.model == oModel).ToList())
                    //{
                    //    listInbound.Add(oInbound);
                    //}
                    //ListPltype = listInbound.Select(x => x.pltype).ToList();

                    //List<MMainResult> rListResultMain = mMainResult.Where(x => x.Model_No == sebango).ToList();
                    //EkbWipPartStock oLastInventoryMain = listEkbInventoryMain.FirstOrDefault(x => x.Partno == oModel.Trim() && x.Wcno == oWcno.ToString());
                    //decimal? nLastInventoryMain = 0;
                    //if (oLastInventoryMain != null)
                    //{
                    //    nLastInventoryMain = oLastInventoryMain.Bal;
                    //}
                    //List<EkbWipPartStock> rInventoryMain = new List<EkbWipPartStock>();
                    //for (int i = 1; i <= 31; i++)
                    //{
                    //    double nResultMainOfDay = 0;
                    //    double nInventoryBold = 0;
                    //    double nSaleOfDay = 0;
                    //    // IF วันที่ 1 = ((Inventory Main - 1 เดือน) - SaleForecaseOfDay), ELSE 2-31 : ((Balance + ResultMainOfDay + Inventory (Hold)) - SaleForecaseOfDay) 
                    //    if (i == 1)
                    //    {
                    //        //nSaleOfDay = rSaleForecase.FirstOrDefault(x => x.pa)
                    //        //nLastInventoryMain = nLastInventoryMain - 
                    //    }
                    //    else
                    //    {

                    //    }
                    //    //rInventoryMain.Add();
                    //}
                    //List<MLastInventory> rLastInventory = oLastInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList();
                    //double nLastInventory = (rLastInventory.Count > 0) ? Convert.ToDouble(rLastInventory.FirstOrDefault().balstk) : 0;
                    //response.Add(new MActual()
                    //{
                    //    ym = ym,
                    //    //wcno = oWcno!.Value.ToString(),
                    //    wcno = oWcno.ToString(),
                    //    model = oModel,
                    //    sebango = sebango,
                    //    listCurpln = itemCurrentPlan,
                    //    listActPln = itemFinal.OrderBy(x => x.Prdymd).ToList(),
                    //    listActMain = mMainResult.Where(x => x.Model_No == sebango).ToList(),
                    //    listSaleForecast = rSaleForecase,
                    //    listInbound = listInbound.OrderBy(x => x.astDate).ToList(),
                    //    listPltype = listPltype.Where(x => x.model.Trim() == oModel.Trim()).Select(x => x.pltype).ToList(),
                    //    listInventory = listInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList(),
                    //    listLastInventory = oLastInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList(),
                    //    LastInventoryMain = listEkbInventoryMain.FirstOrDefault(x => x.Partno == oModel.Trim() && x.Wcno == oWcno.ToString()),
                    //    LastInventory = nLastInventory,
                    //    listHoldInventory = oHoldInventory.Where(x => x.model.Trim() == oModel.Trim()).ToList(),
                    //});
                    //listInbound.Clear();
                    //foreach (int oWcno in rWcno)
                    //{

                    //}
                }
            }

            return Ok(new
            {
                content = response,
                modeltype = rModelType
            });
        }
    }


}
