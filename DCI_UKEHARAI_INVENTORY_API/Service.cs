using DCI_UKEHARAI_INVENTORY_API.Contexts;
using DCI_UKEHARAI_INVENTORY_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;
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
            string endDate = $@"{year}-{month}-{Convert.ToInt32(DateTime.DaysInMonth(int.Parse(year), int.Parse(month))).ToString("##")}";
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
                //var FindModelName = rPnCompressor.FirstOrDefault(x => x.ModelCode == ModelCode);
                //if (FindModelName != null)
                //{
                //    ModelName = FindModelName.Model;
                //}
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
                itemMain.Model_No = ModelCode;
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
            sqlMain5.CommandText = @"SELECT LineName, RIGHT(LEFT(SerialNo, 4),3) Model_No, COUNT(DISTINCT SerialNo) cnt,
                                                case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end shiftDate
                                        FROM etd_leak_check
                                        WHERE StampTime >= @StartDate AND StampTime < @EndDate 
                                            AND LEN(SerialNo) IN ('10', '18') AND LineName IN ('5')
                                        GROUP BY LineName, RIGHT(LEFT(SerialNo, 4),3) , 
                                         case when DATEPART(HOUR, StampTime) >= 8 and DATEPART(HOUR, StampTime) < 20 then format(StampTime,'yyyy-MM-dd', 'en-US') 
                                          when DATEPART(HOUR, StampTime) < 8 then format(DATEADD(day,-1,StampTime),'yyyy-MM-dd','en-US') 
                                          else format(StampTime,'yyyy-MM-dd','en-US') end  ";
            sqlMain5.Parameters.Add(new SqlParameter("@StartDate", startDate));
            sqlMain5.Parameters.Add(new SqlParameter("@EndDate", endDate));
            DataTable dtMainL5 = DBIOTFAC2.Query(sqlMain5);
            foreach (DataRow dr in dtMainL5.Rows)
            {
                MMainResult itemMain = new MMainResult();
                string LineName = dr["LineName"].ToString();
                string ModelCode = dr["Model_No"].ToString();
                string ModelName = "";
                //var FindModelName = rPnCompressor.FirstOrDefault(x => x.ModelCode == ModelCode);
                //if (FindModelName != null)
                //{
                //    ModelName = FindModelName.Model;
                //}
                string Wcno = "";
                if (LineName == "5")
                {
                    Wcno = "905";
                }
                itemMain.LineName = Wcno;
                itemMain.Model_No = ModelCode;
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
            sqlMainL6.CommandText = @"SELECT shiftDate, Model_No, SUM(cnt) cnt FROM(
                                             SELECT  COUNT(DISTINCT PartSerialNo) cnt, RIGHT(LEFT(PartSerialNo,4),3) AS [Model_No], 
                                               case when DATEPART(HOUR, insertdate) >= 8 and DATEPART(HOUR, insertdate) < 20 then format(insertdate,'yyyy-MM-dd', 'en-US') 
                                                when DATEPART(HOUR, insertdate) < 8 then format(DATEADD(day,-1,insertdate),'yyyy-MM-dd','en-US') 
                                                else format(insertdate,'yyyy-MM-dd','en-US') end shiftDate
                                             FROM ElectricalConduction
                                             WHERE IntegratedJudgementResult='OK' AND CheckGlassTerminal='OK' AND (insertdate BETWEEN @StartDate and @EndDate )
                                             GROUP BY RIGHT(LEFT(PartSerialNo,4),3), 
                                                case when DATEPART(HOUR, insertdate) >= 8 and DATEPART(HOUR, insertdate) < 20 then format(insertdate,'yyyy-MM-dd', 'en-US') 
                                                  when DATEPART(HOUR, insertdate) < 8 then format(DATEADD(day,-1,insertdate),'yyyy-MM-dd','en-US') 
                                                  else format(insertdate,'yyyy-MM-dd','en-US') end
                                            ) as t
                                            GROUP BY shiftDate, Model_No ";
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
                itemMain.Model_No = ModelCode;
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
                    //var FindModelName = rPnCompressor.FirstOrDefault(x => x.ModelCode == ModelCode);
                    //if (FindModelName != null)
                    //{
                    //    ModelName = FindModelName.Model;
                    //}
                    string Wcno = "";
                    if (LineName == "7")
                    {
                        Wcno = "907";
                    }
                    itemMain.LineName = Wcno;
                    itemMain.Model_No = ModelCode;
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
                                             WHERE [Integrated_judgment_result] = 'OK' and [Date_insert] between @StartDate and @EndDate  
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
                    //var FindModelName = rPnCompressor.FirstOrDefault(x => x.ModelCode == ModelCode);
                    //if (FindModelName != null)
                    //{
                    //    ModelName = FindModelName.Model;
                    //}
                    string Wcno = "908";
                    itemMain.LineName = Wcno;
                    itemMain.Model_No = ModelCode;
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
            WHERE Judgement = 'OK' AND serial_no NOT LIKE '%MASTER%' AND serial_no <> '' AND FORMAT(DATEADD(hour,-8,plc_date),'yyyy-MM-dd') BETWEEN @StartDate AND @EndDate 
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
                //string ModelCode = rPnCompressor.FirstOrDefault(x => x.Model == ModelName).ModelCode;
                string Wcno = "904";
                itemMain.LineName = Wcno;
                //itemMain.Model_No = ModelCode;
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
                //ModelName = rPnCompressor.FirstOrDefault(x => x.ModelCode == y.Key.Model_No)!.Model,
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
    }
}
