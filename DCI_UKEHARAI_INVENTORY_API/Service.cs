using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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
        internal DataTable WmsGetSales(string ym)
        {
            OracleCommand str = new OracleCommand();
            str.CommandText = $@"WITH H AS (
                                    SELECT CTD.IVNO, CTD.DONO, TO_CHAR(MIN(CTD.LOADDATE), 'yyyyMMdd') LOADDATE
                                    FROM SE.WMS_DELCTD CTD
                                    WHERE TO_CHAR(CTD.LOADDATE, 'yyyyMMdd') LIKE '{ym}%'  
                                    GROUP BY CTD.IVNO, CTD.DONO
                                )
                                SELECT H.IVNO, H.DONO, H.LOADDATE, D.MODEL, SUM(D.PICQTY) PICQTY 
                                FROM H
                                LEFT JOIN SE.WMS_DELDTL D ON H.IVNO = D.IVNO AND H.DONO = D.DONO 
                                GROUP BY H.IVNO, H.DONO, H.LOADDATE, D.MODEL  ";
            DataTable dt = _ALPHAPD.Query(str);
            return dt;
        }

        internal DataTable WmsGetAssortInOut(string ym)
        {
            DateTime ymdStart = DateTime.ParseExact(ym + "01", "yyyyMMdd", CultureInfo.InvariantCulture);
            int year = ymdStart.Year;
            int month = ymdStart.Month;
            int dayInMonth = DateTime.DaysInMonth(year, month);
            OracleCommand strInWH = new OracleCommand();
            strInWH.CommandText = $@"SELECT TO_CHAR(W.ASTDATE,'YYYY-MM-DD') AS ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE, SUM(W.ASTQTY) ASTQTY  FROM SE.WMS_ASSORT W 
                                    WHERE comid = 'DCI'  AND MODEL LIKE '%' AND PLNO LIKE '%' AND TO_CHAR(astdate,'YYYY-MM-DD') BETWEEN '" + ymdStart.ToString("yyyy-MM-dd") + "' AND '" + ($"{year}-{month}-{dayInMonth}") + "' GROUP BY W.ASTDATE, W.ASTTYPE, W.MODEL,  W.PLTYPE";
            DataTable dt = _ALPHAPD.Query(strInWH);
            return dt;
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

        internal List<WmsStkBal> GetCurrentInventory(int yyyy, int mm)
        {
            List<WmsStkBal> rInventory = new List<WmsStkBal>();
            //string ym = new DateTime(yyyy, mm, 01, 0, 0, 0).ToString("yyyyMM");
            //OracleCommand str = new OracleCommand();
            //str.CommandText = @"SELECT W.YM,  W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
            //FROM SE.WMS_STKBAL W
            //WHERE comid= 'DCI' and ym =  :YM and wc in ('DCI','SKO')
            //GROUP BY W.YM, W.MODEL";
            //str.Parameters.Add(new OracleParameter(":YM", ym));
            //DataTable dtLastInventory = _ALPHAPD.Query(str);
            //foreach (DataRow dr in dtLastInventory.Rows)
            //{
            //    WmsStkBal mInventory = new WmsStkBal();
            //    mInventory.ym = dr["YM"].ToString();
            //    //mInventory.wc = dr["WC"].ToString();
            //    mInventory.model = dr["MODEL"].ToString();
            //    mInventory.lbalstk = dr["LBALSTK"].ToString();
            //    mInventory.balstk = dr["BALSTK"].ToString();
            //    rInventory.Add(mInventory);
            //}
            DataTable dtStockFG = GetStockOfDay(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
            foreach (DataRow dr in dtStockFG.Rows)
            {
                WmsStkBal mInventory = new WmsStkBal();
                mInventory.ym = (dr["YMD"].ToString()).Substring(1, 6);
                //mInventory.wc = dr["WC"].ToString();
                mInventory.model = dr["MODEL"].ToString();
                //mInventory.lbalstk = dr["LBALSTK"].ToString();
                mInventory.balstk = dr["INVENTORY"].ToString();
                rInventory.Add(mInventory);
            }
            return rInventory;
        }

        internal List<MInventory> Get8AMInventory(string YMD)
        {
            List<MInventory> mInventories = new List<MInventory>();
            SqlCommand sql = new SqlCommand();
            //sql.CommandText = @"SELECT * FROM UKE_ALPHA_INVENTORY WHERE YMD = @YMD";
            sql.CommandText = @"  SELECT MODEL,CNT,YMD FROM UKE_ALPHA_INVENTORY WHERE YMD =  @YMD
  GROUP  BY   MODEL,CNT,YMD";
            sql.Parameters.Add(new SqlParameter("@YMD", YMD));
            DataTable dtInv = DBSCM.Query(sql);
            foreach (DataRow dr in dtInv.Rows)
            {
                MInventory item = new MInventory();
                item.cnt = dr["CNT"].ToString() != "" ? dr["CNT"].ToString() : "0";
                item.model = dr["MODEL"].ToString();
                mInventories.Add(item);
            }
            return mInventories;
        }

        internal List<MOSW03Delivery> GetDeliveryByYM(DateTime dtToday)
        {
            List<MOSW03Delivery> mOSW03Deliveries = new List<MOSW03Delivery>();
            OracleCommand Ora = new OracleCommand();
            Ora.CommandText = $@"SELECT TO_CHAR(H.DELDATE, 'yyyyMMdd') DELDATE,  
                                               W.MODEL, W.PLTYPE,   
                                               SUM(W.QTY) QTY, 
                                               SUM(W.ALQTY) ALQTY,   
                                               SUM(W.PICQTY) PICQTY   
                                            FROM SE.WMS_DELCTN W
                                            LEFT JOIN SE.WMS_DELCTL H ON H.COMID='DCI' AND H.IVNO = W.IVNO AND H.DONO = W.DONO 
                                            WHERE W.CFBIT = 'F' AND W.IFBIT = 'F' AND TO_CHAR(H.DELDATE, 'yyyyMMdd') LIKE '{dtToday.Year}{dtToday.ToString("MM")}%'
                                            GROUP BY TO_CHAR(H.DELDATE, 'yyyyMMdd') , W.MODEL, W.PLTYPE   ";
            DataTable dt = _ALPHAPD.Query(Ora);
            foreach (DataRow dr in dt.Rows)
            {
                MOSW03Delivery item = new MOSW03Delivery();
                item.model = dr["MODEL"].ToString().Trim();
                item.pltype = dr["PLTYPE"].ToString().Trim();
                //item.cfdate = dr["CFDATE"].ToString();
                //item.ifdate = dr["IFDATE"].ToString();
                item.deldate = dr["DELDATE"].ToString();
                item.qty = Convert.ToInt32(dr["QTY"].ToString());
                item.alqty = Convert.ToInt32(dr["ALQTY"].ToString());
                item.picqty = Convert.ToInt32(dr["PICQTY"].ToString());
                mOSW03Deliveries.Add(item);
            }
            return mOSW03Deliveries;
        }

        internal List<MHoldInventory> GetHoldByYM(int year, int month)
        {
            List<MHoldInventory> mHoldInventories = new List<MHoldInventory>();
            try
            {
                OracleCommand Ora = new OracleCommand();
                Ora.CommandText = @"SELECT W.YM ,W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK,  SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK   FROM SE.WMS_STKBAL W WHERE comid= 'DCI' and ym = :YM and wc in ('HWH','RWQ') and balstk > 0 GROUP BY W.YM,  W.MODEL";
                Ora.Parameters.Add(new OracleParameter(":YM", DateTime.Now.ToString("yyyyMM")));
                DataTable dt = _ALPHAPD.Query(Ora);
                foreach (DataRow dr in dt.Rows)
                {
                    MHoldInventory item = new MHoldInventory();
                    item.ym = dr["YM"].ToString();
                    item.model = dr["MODEL"].ToString();
                    item.balstk = dr["BALSTK"].ToString();
                    item.lbalstk = dr["LBALSTK"].ToString();
                    mHoldInventories.Add(item);
                }
                return mHoldInventories;
            }
            catch
            {
                return mHoldInventories;
            }
        }

        internal DataTable GetStockOfDay(string YMD)
        {
            DataTable dt = new DataTable();
            SqlCommand sql = new SqlCommand();
            sql.CommandText = $@"SELECT * FROM [dbSCM].[dbo].[UKE_INITIAL_STOCK_DCI_OF_DAY]  WHERE YMD = '{YMD}'";
            dt = DBSCM.Query(sql);
            return dt;
        }

        internal DataTable WmsGetTransferInOut(string ym)
        {
            OracleCommand str = new OracleCommand();
            str.CommandText = $@"select to_char(h.recdate,'YYYYMMDD') RECYMD, d.trnno, d.model, d.pltype, d.recqty, h.fromwh, h.towh, case when fromwh='DCI' then 'OUT' else 'IN' end trntype 
                               from wms_trnctl h
                               left join wms_trndtl d on d.trnno = h.trnno  
                               where (h.fromwh = 'DCI' or h.towh = 'DCI') and h.trnsts= 'Transfered' and h.recsts = 'Received'
                               and to_char(h.recdate,'YYYYMMDD') LIKE '{ym}%'
                               order by h.recdate asc  ";
            DataTable dt = _ALPHAPD.Query(str);
            return dt;
        }

        internal DataTable WmsGetSaleExport(string ym)
        {
            OracleCommand str = new OracleCommand();
            str.CommandText = $@"SELECT H.IVNO,TO_CHAR(H.DELDATE, 'yyyyMMdd') DELDATE,  W.MODEL,SUM(W.PICQTY) PICQTY  FROM SE.WMS_DELCTN W
                                            LEFT JOIN SE.WMS_DELCTL H ON H.COMID='DCI' AND H.IVNO = W.IVNO AND H.DONO = W.DONO 
                                            WHERE W.CFBIT = 'F' AND W.IFBIT IN ('F','U') AND TO_CHAR(H.DELDATE, 'yyyyMMdd') LIKE '{ym}%'
                                            AND TO_CHAR(W.UDATE, 'yyyyMMdd') LIKE '{ym}%'
                                            GROUP BY H.IVNO,TO_CHAR(H.DELDATE, 'yyyyMMdd') , W.MODEL  ";
            DataTable dt = _ALPHAPD.Query(str);
            return dt;
        }

        internal DataTable WmsGetSaleDomestic(string ym)
        {
            OracleCommand str = new OracleCommand();
            str.CommandText = $@"SELECT TO_CHAR(H.DELDATE, 'yyyyMMdd') DELDATE,  
                                               W.MODEL, W.PLTYPE,   
                                               SUM(W.QTY) QTY, 
                                               SUM(W.ALQTY) ALQTY,   
                                               SUM(W.PICQTY) PICQTY   
                                            FROM SE.WMS_DELCTN W
                                            LEFT JOIN SE.WMS_DELCTL H ON H.COMID='DCI' AND H.IVNO = W.IVNO AND H.DONO = W.DONO 
                                            WHERE W.CFBIT = 'F' AND W.IFBIT = 'F' AND TO_CHAR(H.DELDATE, 'yyyyMMdd') LIKE '{ym}%'
                                            GROUP BY TO_CHAR(H.DELDATE, 'yyyyMMdd') , W.MODEL, W.PLTYPE ";
            DataTable dt = _ALPHAPD.Query(str);
            return dt;
        }

        internal DataTable WmsGetInventoryIVW01(string ym)
        {
            OracleCommand str = new OracleCommand();
            str.CommandText = $@" SELECT W.YM, 
                               W.MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
                               SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK
                               FROM SE.WMS_STKBAL W
                               WHERE comid = 'DCI' and ym = '{ym}' and wc in ('DCI', 'SKO')
                               GROUP BY W.YM, W.MODEL";
            DataTable dt = _ALPHAPD.Query(str);
            return dt;
        }
        internal DataTable GetFGList()
        {
            SqlCommand sql = new SqlCommand();
            sql.CommandText = $@"SELECT [MODELGROUP], [MODEL]  ,CAST([SEBANGO] AS NVARCHAR(4))  SEBANGO,DIAMETER FROM [dbSCM].[dbo].[WMS_MDW27_MODEL_MASTER] WHERE ACTIVE = 'ACTIVE' GROUP BY [MODELGROUP], [MODEL]  ,[SEBANGO],DIAMETER ";
            DataTable dt = DBSCM.Query(sql);
            return dt;
        }

        internal DataTable GetSaleInfo(string ym)
        {
            string year = ym.Substring(0, 4);
            MSaleVersion saleVer = getSaleVersion(year);
            int rev = saleVer.rev;
            int lrev = saleVer.lrev;

            SqlCommand sqlGetSale = new SqlCommand();
            sqlGetSale.CommandText = $@"SELECT * FROM [dbSCM].[dbo].[AL_SaleForecaseMonth]  WHERE ym = '{ym}' AND REV = '{rev}' AND LREV = '{lrev}' AND (D01 >0 OR D02 >0 OR D03 >0 OR D04 >0 OR D05 >0 OR D06 >0 OR D07 >0 OR D08 >0 OR D09 >0 OR D10 >0 OR D11 >0 OR D12 >0 OR D13 >0 OR D14 >0 OR D15 >0 OR D16 >0 OR D17 >0 OR D18 >0 OR D19 >0 OR D20 >0 OR D21 >0 OR D22 >0 OR D23 >0 OR D24 >0 OR D25 >0 OR D26 >0 OR D27 >0 OR D28 >0 OR D29 >0 OR D30 >0 OR D31 >0)  order by Customer ASC";
            DataTable dtSaleInfo = DBSCM.Query(sqlGetSale);
            return dtSaleInfo;
        }
        internal DataTable GetDelivery(string ym)
        {
            string year = ym.Substring(0, 4);
            string month = ym.Substring(4, 2);
            List<MOSW03Delivery> mOSW03Deliveries = new List<MOSW03Delivery>();
            OracleCommand strGetDelivery = new OracleCommand();
            strGetDelivery.CommandText = $@"SELECT TO_CHAR(H.DELDATE, 'yyyyMMdd') DELDATE,  
                                               TRIM(W.MODEL) MODEL, W.PLTYPE,   
                                               SUM(W.QTY) QTY, 
                                               SUM(W.ALQTY) ALQTY,   
                                               SUM(W.PICQTY) PICQTY   
                                            FROM SE.WMS_DELCTN W
                                            LEFT JOIN SE.WMS_DELCTL H ON  H.IVNO = W.IVNO AND H.DONO = W.DONO 
                                            WHERE W.COMID = 'DCI' AND  W.CFBIT = 'F' AND W.IFBIT = 'F' AND TO_CHAR(H.DELDATE, 'yyyyMMdd') LIKE '{year}{month}%'
                                            GROUP BY TO_CHAR(H.DELDATE, 'yyyyMMdd') , W.MODEL, W.PLTYPE   ";
            DataTable dtDelivery = _ALPHAPD.Query(strGetDelivery);
            return dtDelivery;
        }

        internal void SetPropertyValue(object obj, string propertyName, object value)
        {
            PropertyInfo? prop = obj.GetType().GetProperty(propertyName);
            if (prop != null && prop.CanWrite)
            {
                if (value is decimal decimalValue)
                {
                    prop.SetValue(obj, decimalValue.ToString());
                }
                else
                {
                    prop.SetValue(obj, value);
                }

            }
        }

        internal object GetValueByKey(object obj, string key)
        {
            var propertyInfo = obj.GetType().GetProperty(key); // ค้นหา Property ตามชื่อ Key
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj); // ดึงค่า Property
            }

            return null; // คืนค่า null หากไม่พบ Property
        }

        internal T? GetDynamicValueNullable<T>(object obj, string propertyName) where T : struct
        {
            PropertyInfo? property = obj.GetType().GetProperty(propertyName);

            if (property == null || !property.CanRead)
            {
                throw new ArgumentException($"Property '{propertyName}' not found or is not readable.");
            }

            object? value = property.GetValue(obj);

            if (value == null)
            {
                return null;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        internal DataTable GetWMSFGInventory()
        {

            OracleCommand strAlphaPD = new OracleCommand();
            strAlphaPD.CommandText = @"select model, pltype, count(serial) cnt,to_char(current_date,'YYYY-MM-DD') as currentDate
from fh001 
where comid='DCI' and nwc in ('DCI','SKO')  
  and locacode like '%'
group by model, pltype
order by model";
            DataTable dtInventory = _ALPHAPD.Query(strAlphaPD);
            return dtInventory;
        }
        internal List<DstWipPrd> GetFGFinalResult(string ym)
        {
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
            return rResultFinal;
        }

        internal DataTable getCurPlnTotalOfDay(string ym)
        {
            SqlCommand sql = new SqlCommand();
            sql.CommandText = $@"SELECT TRIM([MODEL]) MODEL
      ,[SEBANGO] 
      ,SUM([DAY01]) [DAY01]
      ,SUM([DAY02]) [DAY02]
	   ,SUM([DAY03]) [DAY03]
	    ,SUM([DAY04]) [DAY04]
		 ,SUM([DAY05]) [DAY05]
		  ,SUM([DAY06]) [DAY06]
		   ,SUM([DAY07]) [DAY07]
		    ,SUM([DAY08]) [DAY08]
			 ,SUM([DAY09]) [DAY09]
			  ,SUM([DAY10]) [DAY10]
			   ,SUM([DAY11]) [DAY11]
			    ,SUM([DAY12]) [DAY12]
				 ,SUM([DAY13]) [DAY13]
				  ,SUM([DAY14]) [DAY14]
				   ,SUM([DAY15]) [DAY15]
				    ,SUM([DAY16]) [DAY16]
					 ,SUM([DAY17]) [DAY17]
					  ,SUM([DAY18]) [DAY18]
					   ,SUM([DAY19]) [DAY19]
					      ,SUM([DAY20]) [DAY20]
						     ,SUM([DAY21]) [DAY21]
							    ,SUM([DAY22]) [DAY22]
								   ,SUM([DAY23]) [DAY23]
								      ,SUM([DAY24]) [DAY24]
									     ,SUM([DAY25]) [DAY25]
										    ,SUM([DAY26]) [DAY26]
											   ,SUM([DAY27]) [DAY27]
											      ,SUM([DAY28]) [DAY28]
												     ,SUM([DAY29]) [DAY29]
													    ,SUM([DAY30]) [DAY30]   ,SUM([DAY31]) [DAY31]
      ,SUM([YM_QTY]) TOTAL
  FROM [dbSCM].[dbo].[AL_GSD_CURPLN]
  WHERE PRDYM = '{ym}' AND (DAY01 > 0 OR DAY02 > 0 OR DAY03 > 0 OR DAY04 > 0 OR DAY05 > 0  OR DAY06 > 0  OR DAY07 > 0  OR DAY08 > 0  OR DAY09 > 0  OR DAY10 > 0
   OR DAY11 > 0  OR DAY12 > 0  OR DAY13 > 0  OR DAY14 > 0  OR DAY15 > 0  OR DAY16 > 0  OR DAY17 > 0  OR DAY18 > 0  OR DAY19 > 0 OR DAY20 > 0
    OR DAY21 > 0  OR DAY22 > 0  OR DAY23 > 0  OR DAY24 > 0  OR DAY25 > 0  OR DAY26 > 0  OR DAY27 > 0  OR DAY28 > 0  OR DAY29 > 0  OR DAY30 > 0 OR DAY31 > 0) 
	GROUP BY   TRIM([MODEL]) 
      ,[SEBANGO] ";
            DataTable dt = DBSCM.Query(sql);
            return dt;
        }

        internal DataTable GetFGHold(string ym)
        {
            OracleCommand strHoldInventory = new OracleCommand();
            strHoldInventory.CommandText = $@"SELECT W.YM , TRIM( W.MODEL) MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
                                                   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
                                                FROM SE.WMS_STKBAL W
                                                WHERE comid= 'DCI' and ym = '{ym}'
                                                  and wc in ('HWH','RWQ')
                                                and balstk > 0
                                                GROUP BY W.YM,  W.MODEL";
            DataTable dtHoldInventory = _ALPHAPD.Query(strHoldInventory);
            return dtHoldInventory;
        }

        internal DataTable GetFGPDT(string ym)
        {
            OracleCommand strPDT = new OracleCommand();
            strPDT.CommandText = $@"SELECT W.YM, W.WC, 
                                   (W.MODEL) MODEL, SUM(W.LBALSTK) LBALSTK, SUM(W.INSTK) INSTK, 
                                   SUM(W.OUTSTK) OUTSTK, SUM(W.BALSTK) BALSTK  
                                FROM SE.WMS_STKBAL W
                                WHERE comid= 'DCI' and ym = '{ym}'
                                  and wc in ('PDT')
                                  and balstk > 0
                                GROUP BY W.YM, W.WC, W.MODEL";
            DataTable dtPDT = _ALPHAPD.Query(strPDT);
            return dtPDT;
        }
        internal DataTable GetLastFGInventory(string ym)
        {
            OracleCommand sql = new OracleCommand();
            sql.CommandText = $@"SELECT W.YM, 
                       TRIM(W.MODEL) MODEL, SUM(W.LBALSTK) LBALSTK,SUM(W.BALSTK) BALSTK  
                    FROM SE.WMS_STKBAL W
                    WHERE comid= 'DCI' and ym =  '{ym}'
                      and wc in ('DCI','SKO')
                    GROUP BY W.YM, W.MODEL";
            DataTable dtLastInventory = _ALPHAPD.Query(sql);
            return dtLastInventory;
        }
        internal DataTable GetUkeStartStockOfDay()
        {
            SqlCommand sql = new SqlCommand();
            sql.CommandText = $@"SELECT * FROM [dbSCM].[dbo].[UKE_INITIAL_STOCK_DCI_OF_DAY]
  WHERE YMD = CASE WHEN (SELECT TOP (1) [YMD]  FROM [dbSCM].[dbo].[UKE_INITIAL_STOCK_DCI_OF_DAY] ORDER BY YMD DESC ) IS NULL THEN FORMAT(GETDATE(),'yyyyMMdd') 
	ELSE (SELECT TOP (1) [YMD]  FROM [dbSCM].[dbo].[UKE_INITIAL_STOCK_DCI_OF_DAY] ORDER BY YMD DESC) END";
            DataTable dt = DBSCM.Query(sql);
            return dt;
        }

        internal DataTable GetInbounds()
        {
            DateTime dtNow = DateTime.Now;
            string year = dtNow.ToString("yyyy");
            string month = dtNow.ToString("MM");
            int dayOfMonth = DateTime.DaysInMonth(int.Parse(year), int.Parse(month));

            SqlCommand sql = new SqlCommand();
            sql.CommandText = $@"SELECT  [MODEL],[AREA] SBU  FROM [dbSCM].[dbo].[WMS_MDW27_MODEL_MASTER] GROUP BY [MODEL],[AREA]";
            DataTable dtSBU = DBSCM.Query(sql);

            OracleCommand ora = new OracleCommand();
            ora.CommandText = $@"SELECT '' SBU, MSTR.SEBANGO,CASE 
                                                        WHEN SUBSTR(INB.MODEL, 1, 2) = '1Y' THEN '1YC'
                                                        WHEN SUBSTR(INB.MODEL, 1, 2) = '2Y' THEN '2YC'
                                                        WHEN SUBSTR(INB.MODEL, 1, 1) = 'J' THEN 'SCR'
                                                        ELSE 'ODM'  END AS SKU, INB.* FROM (SELECT TO_CHAR(W.ASTDATE,'dd') AS Day, TRIM(W.MODEL) MODEL
                                                        --,  W.PLTYPE
                                                        , SUM(CASE WHEN W.ASTTYPE = 'IN' THEN W.ASTQTY ELSE (W.ASTQTY * -1) END) QTY 
FROM SE.WMS_ASSORT W
WHERE comid = 'DCI'  AND TO_CHAR(astdate -8/24,'YYYY-MM-DD') BETWEEN '{year}-{month}-01' AND '{year}-{month}-{dayOfMonth}' 
GROUP BY W.ASTDATE
--, W.ASTTYPE
, W.MODEL,  W.PLTYPE) 
PIVOT (
    SUM(QTY) FOR Day  IN ('01' AS d01, '02' AS d02, '03' AS d03, '04' AS d04, '05' AS d05, '06' AS d06, '07' AS d07, '08' AS d08, '09' AS d09, '10' AS d10, '11' AS d11, '12' AS d12, '13' AS d13, '14' AS d14, '15' AS d15, '16' AS d16, '17' AS d17, '18' AS d18, '19' AS d19, '20' AS d20, '21' AS d21, '22' AS d22, '23' AS d23, '24' AS d24, '25' AS d25, '26' AS d26, '27' AS d27, '28' AS d28, '29' AS d29, '30' AS d30, '31' AS d31 )
    ) INB
    LEFT JOIN (  SELECT MODEL ,POSTCODE AS SEBANGO FROM MT003 WHERE lrev = 999 GROUP BY  MODEL,POSTCODE) MSTR
    ON MSTR.MODEL = INB.MODEL";
            DataTable dtInbound = _ALPHAPD.Query(ora);
            foreach(DataRow dr in dtInbound.Rows)
            {
                var oSBU = dtSBU.AsEnumerable().Where(x => x.Field<string>("MODEL") == dr["MODEL"].ToString()).ToList();
                string sbu = "";
                if (oSBU != null)
                {
                    sbu = oSBU.FirstOrDefault().Field<string>("SBU");
                }
                dr["SBU"] = sbu;
            }
            return dtInbound;
        }
        public List<Dictionary<string, object>> DataTableToJson(DataTable table)
        {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var rowDictionary = new Dictionary<string, object>();
                foreach (DataColumn column in table.Columns)
                {
                    // Replace DBNull or null with an empty string
                    rowDictionary[column.ColumnName] = row[column] == DBNull.Value || row[column] == null
                        ? ""
                        : row[column];
                }
                rows.Add(rowDictionary);
            }
            return rows;
        }
    }
}
