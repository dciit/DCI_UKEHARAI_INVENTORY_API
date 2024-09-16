using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DCI_UKEHARAI_INVENTORY_API
{
    public class Service
    {
        private readonly DBSCM _DBSCM;
        SqlConnectDB DBIOTFAC2 = new SqlConnectDB("dbIoTFac2");
        SqlConnectDB DBIOTFAC3 = new SqlConnectDB("dbIoTFac3");
        SqlConnectDB DBIOT = new SqlConnectDB("dbIoT");
        SqlConnectDB DBIOTL8 = new SqlConnectDB("dbIoTL8");
        SqlConnectDB DBSCM = new SqlConnectDB("dbSCM");
        SqlConnectDB DBHRM = new SqlConnectDB("dbHRM");
        private OraConnectDB _ALPHAPD = new OraConnectDB("ALPHAPD");
        private OraConnectDB _ALPHAPD1 = new OraConnectDB("ALPHA01");
        private OraConnectDB _ALPHAPD2 = new OraConnectDB("ALPHA02");
        Helper oHelper = new Helper();
        //private readonly DBIOT2 _DBIOT2;
        public Service(DBSCM dBSCM)
        {
            _DBSCM = dBSCM;
        }
        public Service() { }

        public List<AlGsdActpln> Plans(string ym = "")
        {
            List<AlGsdActpln> res = new List<AlGsdActpln>();
            if (ym != "")
            {
                res = _DBSCM.AlGsdActplns.Where(x => ym.Contains(x.Prdymd != null ? x.Prdymd : "")).ToList();
            }
            return res;
        }
        public List<MMainResult> GetResultMain(string year, string month)
        {

            string startDate = $@"{year}-{month}-01";
            string endDate = $@"{year}-{month}-{Convert.ToInt32(DateTime.DaysInMonth(int.Parse(year), int.Parse(month))).ToString("##")} 23:59:59";
            List<MMainResult> rMainResult = new List<MMainResult>();
            //List<PnCompressor> rPnCompressor = _DBSCM.PnCompressors.Select(x => new PnCompressor { ModelCode = x.ModelCode, Model = x.Model }).Distinct().ToList();
            //---------------------------------------------------------------------------------------------//
            //-------------------------------  ADJ MAIN RESULT L1, 2 -----------------------------------//
            //---------------------------------------------------------------------------------------------//
            SqlCommand sqlL1 = new SqlCommand();
            sqlL1.CommandText = @"SELECT LineName, RIGHT(LEFT(SerialNo, 4),3) Model_No, COUNT(DISTINCT SerialNo) cnt,
                                                case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end shiftDate
                                        FROM etd_leak_check
                                        WHERE StampTime >= @StartDate AND StampTime < @EndDate 
                                            AND LEN(SerialNo) IN ('10', '18') AND LineName IN ('1', '2')
                                        GROUP BY LineName, RIGHT(LEFT(SerialNo, 4),3) , 
                                         case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end  ";
            sqlL1.Parameters.Add(new SqlParameter("@StartDate", startDate));
            sqlL1.Parameters.Add(new SqlParameter("@EndDate", endDate));
            DataTable dtMainL1_2 = DBIOTFAC2.Query(sqlL1);
            foreach (DataRow dr in dtMainL1_2.Rows)
            {
                MMainResult itemMain = new MMainResult();
                string LineName = dr["LineName"].ToString();
                string ModelCode = dr["Model_No"].ToString();
                string ModelName = "";
                string Wcno = "";
                if (LineName == "1")
                {
                    Wcno = "901";
                }
                else if (LineName == "2")
                {
                    Wcno = "902";
                }
                itemMain.LineName = Wcno;
                itemMain.Model_No = oHelper.ConvStrToInt(ModelCode).ToString("D4");
                itemMain.ModelName = ModelName;
                itemMain.shiftDate = dr["ShiftDate"].ToString();
                itemMain.cnt = int.Parse(dr["cnt"]!.ToString());
                rMainResult.Add(itemMain);
            }
            //---------------------------------------------------------------------------------------------//
            //------------------------------- END ADJ MAIN RESULT L1, 2 -----------------------------------//
            //---------------------------------------------------------------------------------------------//


            //---------------------------------------------------------------------------------------------//
            //-------------------------------  ADJ MAIN RESULT L5 -----------------------------------------//
            //---------------------------------------------------------------------------------------------//
            SqlCommand sqlMain5 = new SqlCommand();
            sqlMain5.CommandText = $@"SELECT LineName, RIGHT(LEFT(SerialNo, 4),3) Model_No, COUNT(DISTINCT SerialNo) cnt,
                                                case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end shiftDate
                                        FROM etd_leak_check
                                        WHERE StampTime >= '{startDate}' AND StampTime < '{endDate}' 
                                            AND LEN(SerialNo) IN ('10', '18') AND LineName IN ('5')
                                        GROUP BY LineName, RIGHT(LEFT(SerialNo, 4),3) , 
                                         case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end  ";
            //sqlMain5.Parameters.Add(new SqlParameter("@StartDate", startDate));
            //sqlMain5.Parameters.Add(new SqlParameter("@EndDate", endDate));
            DataTable dtMainL5 = DBIOTFAC2.Query(sqlMain5);
            foreach (DataRow dr in dtMainL5.Rows)
            {
                MMainResult itemMain = new MMainResult();
                string LineName = dr["LineName"].ToString();
                string ModelCode = dr["Model_No"].ToString();
                string ModelName = "";
                string Wcno = "";
                if (LineName == "5")
                {
                    Wcno = "905";
                }
                itemMain.LineName = Wcno;
                itemMain.Model_No = oHelper.ConvStrToInt(ModelCode).ToString("D4");
                itemMain.ModelName = ModelName;
                itemMain.shiftDate = dr["ShiftDate"].ToString();
                itemMain.cnt = int.Parse(dr["cnt"]!.ToString());
                rMainResult.Add(itemMain);
            }

            //---------------------------------------------------------------------------------------------//
            //-------------------------------  END ADJ MAIN RESULT L5 -------------------------------------//
            //---------------------------------------------------------------------------------------------//



            //---------------------------------------------------------------------------------------------//
            //---------------------------------  ADJ MAIN RESULT L6 ---------------------------------------//
            //---------------------------------------------------------------------------------------------//
            SqlCommand sqlMainL6 = new SqlCommand();
            sqlMainL6.CommandText = @"	SELECT  COUNT(DISTINCT PartSerialNo) cnt, RIGHT(LEFT(PartSerialNo,4),3) AS [Model_No], 
                                               case when DATEPART(HOUR, insertdate) >= 8 and DATEPART(HOUR, insertdate) < 20 then format(insertdate,'yyyy-MM-dd', 'en-US') 
                                                when DATEPART(HOUR, insertdate) < 8 then format(DATEADD(day,-1,insertdate),'yyyy-MM-dd','en-US') 
                                                else format(insertdate,'yyyy-MM-dd','en-US') end shiftDate
                                             FROM ElectricalConduction
                                             WHERE IntegratedJudgementResult='OK' AND CheckGlassTerminal='OK'
											 AND (insertdate >=  @StartDate AND insertdate <= CONCAT(DATEADD(day,1,CAST(@EndDate AS DATE)),' 08:00:00'))
											 GROUP BY RIGHT(LEFT(PartSerialNo,4),3), 
                                                case when DATEPART(HOUR, insertdate) >= 8 and DATEPART(HOUR, insertdate) < 20 then format(insertdate,'yyyy-MM-dd', 'en-US') 
                                                  when DATEPART(HOUR, insertdate) < 8 then format(DATEADD(day,-1,insertdate),'yyyy-MM-dd','en-US') 
                                                  else format(insertdate,'yyyy-MM-dd','en-US') end";
            sqlMainL6.Parameters.Add(new SqlParameter("@StartDate", startDate));
            sqlMainL6.Parameters.Add(new SqlParameter("@EndDate", endDate));
            DataTable dtMainL6 = DBIOTFAC3.Query(sqlMainL6);
            foreach (DataRow dr in dtMainL6.Rows)
            {
                MMainResult itemMain = new MMainResult();
                //string LineName = dr["LineName"].ToString();
                string ModelCode = dr["Model_No"].ToString();
                string ModelName = "";
                //var FindModelName = rPnCompressor.FirstOrDefault(x => x.ModelCode == ModelCode);
                //if (FindModelName != null)
                //{
                //    ModelName = FindModelName.Model;
                //}
                string Wcno = "906";
                itemMain.LineName = Wcno;
                itemMain.Model_No = oHelper.ConvStrToInt(ModelCode).ToString("D4");
                itemMain.ModelName = ModelName;
                itemMain.shiftDate = dr["ShiftDate"].ToString();
                itemMain.cnt = int.Parse(dr["cnt"]!.ToString());
                rMainResult.Add(itemMain);
            }
            //---------------------------------------------------------------------------------------------//
            //------------------------------- END ADJ MAIN RESULT L6 --------------------------------------//
            //---------------------------------------------------------------------------------------------//


            //---------------------------------------------------------------------------------------------//
            //---------------------------------  ADJ MAIN RESULT L7 ---------------------------------------//
            //---------------------------------------------------------------------------------------------//
            SqlCommand sqlSelect907 = new SqlCommand();
            sqlSelect907.CommandText = @"SELECT LineName, RIGHT(LEFT(SerialNo, 4),3) Model_No, COUNT(DISTINCT SerialNo) cnt,
                                                case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end shiftDate
                                        FROM etd_leak_check
                                        WHERE StampTime >= @StartDate AND StampTime < @EndDate 
                                            AND LEN(SerialNo) IN ('10', '18') AND LineName IN ('7')
                                        GROUP BY LineName, RIGHT(LEFT(SerialNo, 4),3) , 
                                         case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end  ";
            sqlSelect907.Parameters.Add(new SqlParameter("@StartDate", startDate));
            sqlSelect907.Parameters.Add(new SqlParameter("@EndDate", endDate));
            sqlSelect907.CommandTimeout = 180;
            DataTable dtSerial907 = DBIOTFAC2.Query(sqlSelect907);
            if (dtSerial907.Rows.Count > 0)
            {
                foreach (DataRow dr in dtSerial907.Rows)
                {
                    MMainResult itemMain = new MMainResult();
                    string LineName = dr["LineName"].ToString();
                    string ModelCode = dr["Model_No"].ToString();
                    string ModelName = "";
                    string Wcno = "";
                    if (LineName == "7")
                    {
                        Wcno = "907";
                    }
                    itemMain.LineName = Wcno;
                    itemMain.Model_No = oHelper.ConvStrToInt(ModelCode).ToString("D4");
                    itemMain.ModelName = ModelName;
                    itemMain.shiftDate = dr["ShiftDate"].ToString();
                    itemMain.cnt = int.Parse(dr["cnt"]!.ToString());
                    rMainResult.Add(itemMain);
                }
            }
            //---------------------------------------------------------------------------------------------//
            //------------------------------- END ADJ MAIN RESULT L7 --------------------------------------//
            //---------------------------------------------------------------------------------------------//


            //---------------------------------------------------------------------------------------------//
            //---------------------------------  ADJ MAIN RESULT L8 ---------------------------------------//
            //---------------------------------------------------------------------------------------------//
            SqlCommand sqlSelectLine8 = new SqlCommand();
            sqlSelectLine8.CommandText = @"SELECT shiftDate, [Model_No], COUNT([Part_Serial_No]) cnt FROM(
                                             SELECT [Model_No], [Part_Serial_No]
                                               ,case when DATEPART(HOUR, [Date_insert]) >= 8 and DATEPART(HOUR, [Date_insert]) < 20 then format([Date_insert],'yyyy-MM-dd', 'en-US') 
                                               when DATEPART(HOUR, [Date_insert]) < 8 then format(DATEADD(day,-1,[Date_insert]),'yyyy-MM-dd','en-US') 
                                               else format([Date_insert],'yyyy-MM-dd','en-US') end shiftDate
                                             FROM [dbIoT].[dbo].[L8_ConnectingCheck]
                                             WHERE [Integrated_judgment_result] = 'OK' and  ([Date_insert] >= @StartDate AND [Date_insert] <= @EndDate) 
                                            ) as t
                                            GROUP BY shiftDate, [Model_No]
                                            ORDER BY shiftDate ASC    ";

            sqlSelectLine8.Parameters.Add(new SqlParameter("@StartDate", startDate));
            sqlSelectLine8.Parameters.Add(new SqlParameter("@EndDate", endDate));
            sqlSelectLine8.CommandTimeout = 180;
            DataTable dtSerialLine8 = DBIOTL8.Query(sqlSelectLine8);
            if (dtSerialLine8.Rows.Count > 0)
            {
                foreach (DataRow dr in dtSerialLine8.Rows)
                {
                    MMainResult itemMain = new MMainResult();
                    //string LineName = dr["LineName"].ToString();
                    string ModelCode = dr["Model_No"].ToString();
                    string ModelName = "";
                    string Wcno = "908";
                    itemMain.LineName = Wcno;
                    itemMain.Model_No = oHelper.ConvStrToInt(ModelCode).ToString("D4");
                    itemMain.ModelName = ModelName;
                    itemMain.shiftDate = dr["ShiftDate"].ToString();
                    itemMain.cnt = int.Parse(dr["cnt"]!.ToString());
                    rMainResult.Add(itemMain);
                }
            }
            //---------------------------------------------------------------------------------------------//
            //------------------------------- END ADJ MAIN RESULT L8 --------------------------------------//
            //---------------------------------------------------------------------------------------------//

            //---------------------------------------------------------------------------------------------//
            //---------------------------------  ADJ MAIN RESULT L4 (SCR) ---------------------------------//
            //---------------------------------------------------------------------------------------------//
            SqlCommand sqlSelectLine4 = new SqlCommand();
            sqlSelectLine4.CommandText = @"SELECT FORMAT(DATEADD(hour,-8,plc_date),'yyyy-MM-dd') PRD_DATE
                ,FORMAT(DATEADD(hour,-8,plc_date),'yyyyMMdd') YMD
                ,M.[Model] 
                ,COUNT(DISTINCT [serial_no]) CNT
            FROM [dbIoT].[dbo].[SCR_AxisCore] A
            LEFT JOIN (SELECT [ModelCode],[Model] FROM [192.168.226.86].dbSCM.dbo.PN_Compressor 
                  WHERE [Status] = 'ACTIVE' and Line = '4' 
                  GROUP BY [ModelCode],[Model]) M ON M.[ModelCode] = SUBSTRING([serial_no],1,4)
            WHERE Judgement = 'OK' AND serial_no NOT LIKE '%MASTER%' AND serial_no <> '' AND (FORMAT(DATEADD(hour,-8,plc_date),'yyyy-MM-dd') >= @StartDate AND FORMAT(DATEADD(hour,-8,plc_date),'yyyy-MM-dd') <= @EndDate) 
            GROUP BY FORMAT(DATEADD(hour,-8,plc_date),'yyyy-MM-dd')
                ,FORMAT(DATEADD(hour,-8,plc_date),'yyyyMMdd') 
                ,M.[Model]";
            sqlSelectLine4.Parameters.Add(new SqlParameter("@StartDate", startDate));
            sqlSelectLine4.Parameters.Add(new SqlParameter("@EndDate", endDate));
            sqlSelectLine4.CommandTimeout = 180;
            DataTable dtSerialLine4 = DBIOT.Query(sqlSelectLine4);
            foreach (DataRow dr in dtSerialLine4.Rows)
            {
                MMainResult itemMain = new MMainResult();
                string ModelName = dr["Model"]!.ToString();
                string Wcno = "904";
                itemMain.LineName = Wcno;
                itemMain.ModelName = ModelName;
                itemMain.shiftDate = dr["PRD_DATE"].ToString();
                itemMain.cnt = int.Parse(dr["CNT"]!.ToString());
                rMainResult.Add(itemMain);
            }
            //---------------------------------------------------------------------------------------------//
            //------------------------------- END ADJ MAIN RESULT L4 (SCR) --------------------------------//
            //---------------------------------------------------------------------------------------------//
            var result = rMainResult.GroupBy(x => new { x.LineName, x.Model_No, x.shiftDate }).Select(y => new
            {
                y.Key.LineName,
                y.Key.Model_No,
                y.Key.shiftDate,
                cnt = y.Sum(x => x.cnt)
            }).ToList();
            return rMainResult;
        }

        internal List<PnCompressor> getModels()
        {
            List<PnCompressor> res = new List<PnCompressor>();
            SqlCommand sql = new SqlCommand();
            sql.CommandText = @"SELECT  ModelCode ,Model,SUBSTRING(ModelType,1,3) as ModelType FROM [dbSCM].[dbo].[PN_Compressor]
  WHERE Status = 'ACTIVE' AND ModelType NOT IN ('PACKING','SPECIAL') AND LEN(ModelType) >= 3
 GROUP BY ModelCode,Model,ModelType ORDER BY Model ";
            DataTable dt = DBSCM.Query(sql);
            foreach (DataRow dr in dt.Rows)
            {
                PnCompressor item = new PnCompressor();
                item.ModelCode = dr["ModelCode"].ToString();
                item.Model = dr["Model"].ToString();
                item.ModelType = dr["ModelType"].ToString();
                res.Add(item);
            }
            return res;
        }

        public string getModelGroup(string modelName = "")
        {
            string modelGroup = "";
            if (modelName.Substring(0, 1) == "1" || modelName.Substring(0, 1) == "2")
            {
                modelGroup = modelName.Substring(0, 1) + "YC";
            }
            else if (modelName.Substring(0, 1) == "J")
            {
                modelGroup = "SCR";
            }
            else
            {
                modelGroup = "ODM";
            }
            return modelGroup;
        }

        //        internal List<MInventory> GetInventory()
        //        {
        //            List<MInventory> res = new List<MInventory>();
        //            OracleCommand str = new OracleCommand();
        //            str.CommandText = @"select model, pltype, count(serial) cnt,to_char(current_date,'YYYY-MM-DD') as currentDate
        //from fh001 
        //where comid='DCI' and nwc in ('DCI','SKO')  
        //  and locacode like '%'
        //group by model, pltype
        //order by model";

        //            DataTable dt = _ALPHAPD.Query(str);
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                MInventory item = new MInventory();
        //                item.model = dr["model"].ToString();
        //                item.date = dr["currentDate"].ToString();
        //                item.pltype = dr["pltype"].ToString();
        //                item.cnt = dr["cnt"].ToString();
        //                res.Add(item);
        //            }
        //            return res;
        //        }


        internal List<MInbound> GetInbound(DateTime sDate, DateTime fDate, string type = "")
        {

            List<MInbound> res = new List<MInbound>();
            OracleCommand strInWH = new OracleCommand();
            strInWH.CommandText = $@"SELECT TO_CHAR(W.ASTDATE,'YYYY-MM-DD') AS ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE, SUM(W.ASTQTY) ASTQTY 
FROM SE.WMS_ASSORT W
WHERE comid = 'DCI'  AND MODEL LIKE '%' AND PLNO LIKE '%' " + ((type != "" && (type == "IN" || type == "OUT")) ? (" AND W.ASTTYPE = '" + type + "'") : "") + "AND TO_CHAR(astdate,'YYYY-MM-DD') BETWEEN '" + sDate.AddHours(-8).ToString("yyyy-MM-dd") + "' AND '" + fDate.AddDays(-8).ToString("yyyy-MM-dd") + "' GROUP BY W.ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE";
            DataTable dt = _ALPHAPD.Query(strInWH);
            foreach (DataRow dr in dt.Rows)
            {
                MInbound item = new MInbound();
                item.astDate = dr["ASTDATE"].ToString();
                item.model = dr["MODEL"].ToString();
                item.pltype = dr["PLTYPE"].ToString();
                item.astQty = dr["ASTQTY"].ToString() != "" ? int.Parse(dr["ASTQTY"].ToString()) : 0;
                item.astType = dr["ASTTYPE"].ToString();
                res.Add(item);
            }
            return res;
        }

        internal List<GstSalMdl> GetSKU()
        {
            List<GstSalMdl> rGstSalMdl = new List<GstSalMdl>();
            OracleCommand strGstSalMdl = new OracleCommand();
            strGstSalMdl.CommandText = @"SELECT G.AREA SKU, G.MODL_NM MODELNAME  FROM PLAN.GST_SALMDL G where lrev = '999'";
            DataTable dtGstSalMdl = _ALPHAPD1.Query(strGstSalMdl);
            foreach (DataRow drGstSalMdl in dtGstSalMdl.Rows)
            {
                GstSalMdl oGstSalMdl = new GstSalMdl();
                string modelName = drGstSalMdl["MODELNAME"].ToString();
                string sku = drGstSalMdl["SKU"].ToString().Replace(@"\r\n", "");
                if (sku != "")
                {
                    oGstSalMdl.modelName = modelName;
                    oGstSalMdl.sku = sku;
                    oGstSalMdl.modelGroup = getModelGroup(modelName);
                    rGstSalMdl.Add(oGstSalMdl);
                }
            }
            return rGstSalMdl;
        }


        internal List<MData> getInvPlnMain(string ym, string model, List<EkbWipPartStock> LastInventory, List<AlSaleForecaseMonth> ListSale, List<MHoldInventory> ListHold, List<MMainResult> ListMain, List<MHoldInventory> rInvHold)
        {
            List<MData> res = new List<MData>(); // IS RESPONSE
            try
            {
                int yyyy = int.Parse(ym.Substring(0, 4));
                int mmmm = int.Parse(ym.Substring(4, 2));
                DateTime dtStart = new DateTime(yyyy, mmmm, 1);
                DateTime dtEnd = new DateTime(dtStart.Year, dtStart.Month, DateTime.DaysInMonth(dtStart.Year, dtStart.Month));
                int total = LastInventory.FirstOrDefault(x => x.Partno == model.Trim() && x.Wcno == "999") != null ? (int)LastInventory.FirstOrDefault(x => x.Partno == model.Trim() && x.Wcno == "999").Bal : 0; // ยอด Inventory Main วันแรกของเดือน
                while (dtStart.Date <= dtEnd.Date)
                {
                    string dd = dtStart.ToString("dd");
                    string mm = dtStart.Month.ToString("MM");
                    int sale = ListSale.Count > 0 ? ListSale.Sum(x => int.Parse(x.GetType().GetProperty($"D{dd}").GetValue(x).ToString())) : 0;
                    //if (model == "1Y056BCBX1T#A")
                    //{
                    //    Console.WriteLine("asd");
                    //}
                    if (dd == "01")
                    {
                        List<MMainResult> rResultMain = ListMain.Where(x => x.shiftDate.Substring(8, 2) == dd).ToList();
                        int InvMain = rResultMain.Count > 0 ? rResultMain.FirstOrDefault().cnt : 0;
                        //total -= InvMain;
                        total -= sale;
                    }
                    else
                    {
                        //if (model == "1Y056BCCX1T#A")
                        //{
                        //    Console.WriteLine("asd");
                        //}
                        string PrevDD = dtStart.AddDays(-1).ToString("dd");
                        List<MMainResult> rResultMain = ListMain.Where(x => x.shiftDate.Substring(8, 2) == PrevDD && x.shiftDate.Substring(5, 2) == mmmm.ToString("D2")).ToList();
                        int InvMain = rResultMain.Count > 0 ? rResultMain.FirstOrDefault().cnt : 0;
                        //int InvHold = rInvHold.Count > 0 ? rInvHold.Sum(x => int.Parse(x.balstk)) : 0;
                        int InvHold = 0;
                        total = (total + InvMain + InvHold) - sale;
                    }
                    res.Add(new MData()
                    {
                        date = dtStart.ToString("yyyyMMdd"),
                        value = total
                    });
                    if (dtStart.Date == dtEnd.Date)
                    {
                        //if (model == "2Y550BVAX1S#A") {
                        //    Console.WriteLine("asdd");
                        //}
                        //int InvHold = 0;
                        //List<MMainResult> rResultMain = ListMain.Where(x => x.shiftDate.Substring(8, 2) == dtStart.ToString("dd")).ToList();
                        //int InvMain = rResultMain.Count > 0 ? rResultMain.FirstOrDefault().cnt : 0;
                        //total = (total + InvMain + InvHold);
                        //if (ListMain.FirstOrDefault(x => x.ModelName == model && x.ym == dtStart.ToString("yyyyMM")) == null)
                        //{
                        //    if (oMainResult.Where(x => x.shiftDate == dtStart.ToString("yyyy-MM-dd")).ToList().Count > 0)
                        //    {
                        //        nResultMain = oMainResult.Where(x => x.shiftDate == dtStart.ToString("yyyy-MM-dd")).Sum(x => x.cnt);
                        //    }
                        //    if (oSale.Count > 0)
                        //    {
                        //        try
                        //        {
                        //            nSale = oSale.Sum(x => int.Parse(x.GetType().GetProperty($"D{dd}").GetValue(x).ToString()));
                        //        }
                        //        catch (Exception e)
                        //        {
                        //            nSale = 0;
                        //        }
                        //    }
                        //    nTotal = (nTotal + nResultMain + nHold) - nSale;
                        //    ListMain.Add(new MResult()
                        //    {
                        //        partno = modelName,
                        //        wcno = "999",
                        //        ym = dtStart.ToString("yyyyMM"),
                        //        bal = nTotal
                        //    });
                        //}
                    }
                    dtStart = dtStart.AddDays(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return res;
        }
        internal MSaleVersion getSaleVersion(string year)
        {
            MSaleVersion oSaleVersion = new MSaleVersion();
            //int rev = 0;
            //int lrev = 0;
            //SqlCommand sqlCheckVersion = new SqlCommand();
            //sqlCheckVersion.CommandText = @"SELECT TOP(1) REV,LREV FROM [dbSCM].[dbo].[AL_SaleForecaseMonth] WHERE ym LIKE '" + year + "%'   order by CAST(rev as int) desc , CAST(lrev as int) desc";
            //DataTable dtGetVersion = DBSCM.Query(sqlCheckVersion);
            //if (dtGetVersion.Rows.Count > 0)
            //{
            //    oSaleVersion.foundData = true;
            //    rev = Convert.ToInt32(dtGetVersion.Rows[0]["REV"].ToString());
            //    lrev = Convert.ToInt32(dtGetVersion.Rows[0]["LREV"].ToString());
            //    // ถ้าเจอ lrev = 999 ใช้งานได้เลย เนื่องจาก แจกจ่าย แล้ว
            //    if (lrev != 999)  // ค้นหา (rev - 1), lrev = (rev - 1) เพื่อหาข้อมูลที่ Distribution ก่อนหน้านี้
            //    {
            //        rev = rev - 1;
            //    }
            //}
            //oSaleVersion.rev = rev;
            //oSaleVersion.lrev = lrev;



            string[] ver = new string[3];  //  1 = HAVE DATA, 2 = REV, 3 = LREV
            SqlCommand sqlCheckVersion = new SqlCommand();
            sqlCheckVersion.CommandText = @"SELECT TOP(1) REV,LREV FROM [dbSCM].[dbo].[AL_SaleForecaseMonth] WHERE ym LIKE '" + year + "%'   order by CAST(rev as int) desc , CAST(lrev as int) desc";
            DataTable dtCheckVersion = DBSCM.Query(sqlCheckVersion);
            if (dtCheckVersion.Rows.Count > 0)
            {
                oSaleVersion.foundData = true;
                oSaleVersion.rev = int.Parse(dtCheckVersion.Rows[0]["REV"].ToString());
                oSaleVersion.lrev = int.Parse(dtCheckVersion.Rows[0]["LREV"].ToString());
            }
            else
            {
                oSaleVersion.foundData = false;
                oSaleVersion.rev = 0;
                oSaleVersion.lrev = 0;
            }
            return oSaleVersion;
        }

        internal List<WmsStkBal> MWSGetInventory(int yyyy, int mm)
        {
            List<WmsStkBal> rInventory = new List<WmsStkBal>();
            string ym = new DateTime(yyyy, mm, 01, 0, 0, 0).ToString("yyyyMM");
            OracleCommand str = new OracleCommand();
            str.CommandText = @"SELECT W.YM,  W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
            FROM SE.WMS_STKBAL W
            WHERE comid= 'DCI' and ym =  :YM and wc in ('DCI','SKO')
            GROUP BY W.YM, W.MODEL";
            str.Parameters.Add(new OracleParameter(":YM", ym));
            DataTable dtLastInventory = _ALPHAPD.Query(str);
            foreach (DataRow dr in dtLastInventory.Rows)
            {
                WmsStkBal mInventory = new WmsStkBal();
                mInventory.ym = dr["YM"].ToString();
                //mInventory.wc = dr["WC"].ToString();
                mInventory.model = dr["MODEL"].ToString();
                mInventory.lbalstk = dr["LBALSTK"].ToString();
                mInventory.balstk = dr["BALSTK"].ToString();
                rInventory.Add(mInventory);
            }
            return rInventory;
        }
    }
}
