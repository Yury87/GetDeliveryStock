using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GetDeliveryStock
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ConnectMSCount("SELECT Count(*) From [AptStocs].[dbo].[Stock] where apt=26") > 10000)
            { 
            List<PharmaHelper> lst = ConnectMS("SELECT * FROM [AptStocs].[dbo].[vw_Stock] where apt = 26 and MedName like '%Линза%' or apt = 26 and tkrid=225011");
            List<SFHelper> list = ConnectMSIshop("SELECT * FROM [AptStocs].[dbo].[vw_Stock] where apt = 26 and MedName like '%Линза%' or apt = 26 and tkrid=225011");
            MakeXMLFilePharma(lst);
            MakeXMLFileSF(list);
            }
        }


        static int ConnectMSCount(string sql)
        {
            int i = 0;
            string MSConnectionStr = "data source=192.168.1.39;persist security info=True;" +
                "user id=ftp;password=Sekret123;" +
                "MultipleActiveResultSets=True;Connection Timeout=120";

            SqlConnection myConnection = new SqlConnection(MSConnectionStr);
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
            }
            try
            {
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myCommand.CommandTimeout = 120;
                i = Convert.ToInt16(myCommand.ExecuteScalar());
                myConnection.Close();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                myConnection.Close();
            }
            return i;
        }

        static List<PharmaHelper> ConnectMS(string sql)
        {
            List<PharmaHelper> lst = new List<PharmaHelper>();
            string MSConnectionStr = "data source=192.168.1.39;persist security info=True;" +
                "user id=ftp;password=Sekret123;" +
                "MultipleActiveResultSets=True;Connection Timeout=120";

            SqlConnection myConnection = new SqlConnection(MSConnectionStr);
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
            }
            try
            {
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myCommand.CommandTimeout = 120;
                SqlDataReader reader =  myCommand.ExecuteReader();
                while(reader.Read())
                {
                    PharmaHelper ph = new PharmaHelper();
                    ph.IID = (long)reader[1];
                    ph.MedId = (int)reader[3];
                    ph.MedName = (string)reader[4];
                    ph.VendorName = (string)reader[5];
                    ph.CountryName = (string)reader[6];
                    ph.RPrice = (decimal)reader[9];
                    ph.DPrice = (decimal)reader[10];
                    ph.Qtty = (int)reader[2];
                    ph.KindID=(int)reader[20];
                    ph.KindName = (string)reader[11];
                    if(reader[15].ToString() != "")
                        ph.VendorBarCode=(decimal)reader[15];
                    ph.PharmId=(int)reader[13];
                    ph.PharmName=(string)reader[14];
                    ph.TKId=(int)reader[16];
                    ph.TKName=(string)reader[18];
                    ph.TKRId= (int)reader[17]; ;
                    ph.TKRName = (string)reader[19];
                    lst.Add(ph);
                }
                myConnection.Close();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                myConnection.Close();
            }
            return lst;
        }

        static List<SFHelper> ConnectMSIshop(string sql)
        {
            List<SFHelper> lst = new List<SFHelper>();
            string MSConnectionStr = "data source=192.168.1.39;persist security info=True;" +
                "user id=ftp;password=Sekret123;" +
                "MultipleActiveResultSets=True;Connection Timeout=120";

            SqlConnection myConnection = new SqlConnection(MSConnectionStr);
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
            }
            try
            {
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myCommand.CommandTimeout = 120;
                SqlDataReader reader = myCommand.ExecuteReader();
                while (reader.Read())
                {
                    SFHelper ph = new SFHelper();
                    ph.Code = reader[3].ToString();
                    ph.Name = reader[4].ToString();
                    ph.Vendor = (string)reader[5];
                    ph.Country = (string)reader[6];
                    ph.Type = (string)reader[11];
                    if((string)reader[7] == "Неопределенное")
                        ph.MNN = "";
                    else
                        ph.MNN = (string)reader[7];
                    ph.RPrice = (decimal)reader[9];
                    ph.DPrice = (decimal)reader[10];
                    lst.Add(ph);
                }
                myConnection.Close();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                myConnection.Close();
            }
            return lst;
        }

        static void WriteLog(string str)
        {
            string dt = DateTime.Now.ToString("yyyyMMdd");
            string winpath = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Samson Pharma LLC\StockParser";

            if (!File.Exists(@".\" + dt + ".log"))
            {

                using (var fs = File.Create(@".\" + dt + ".log"))
                {

                }

            }
            using (StreamWriter log = new StreamWriter(@".\" + dt + ".log", true))
            {
                // Console.WriteLine(str);
                log.WriteLine(str);
            }

        }

        static void MakeXMLFileSF(List<SFHelper> lst)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "windows-1251", null);
            XmlElement root = doc.CreateElement("ВыгрузкаОстатковВИнтернетМагазин");
            doc.InsertBefore(dec, doc.DocumentElement);
            root.SetAttribute("ДатаВыгрузки", DateTime.Now.ToString("dd.MM.yyyy"));
            root.SetAttribute("ВерсияФормата", "1.0");
            root.SetAttribute("Клиент", "iapteka");
            root.SetAttribute("ПО", "ИнфоАптека");
            doc.AppendChild(root);
            XmlElement tovost = doc.CreateElement("ТоварныеОстатки");
            
            int i = 1;
            foreach (SFHelper ph in lst)
            {
                XmlElement pos = doc.CreateElement("Позиция");
                pos.SetAttribute("Nom", i.ToString());
                
                XmlElement code = doc.CreateElement("КодТовара");
                code.SetAttribute("value", ph.Code);
                pos.AppendChild(code);
                XmlElement name = doc.CreateElement("НаименованиеТовара");
                name.SetAttribute("value", ph.Name);
                pos.AppendChild(name);
                XmlElement vendor = doc.CreateElement("НаименованиеПроизводителя");
                vendor.SetAttribute("value", ph.Vendor);
                pos.AppendChild(vendor);
                XmlElement country = doc.CreateElement("НаименованиеСтраны");
                country.SetAttribute("value", ph.Country);
                pos.AppendChild(country);
                XmlElement type = doc.CreateElement("ТипТовара");
                type.SetAttribute("value", ph.Type);
                pos.AppendChild(type);
                XmlElement prop = doc.CreateElement("Свойство1001");
                prop.SetAttribute("value", ph.prop1001);
                pos.AppendChild(prop);
                XmlElement mnn = doc.CreateElement("МНН");
                mnn.SetAttribute("value", ph.MNN);
                pos.AppendChild(mnn);
                XmlElement rprice = doc.CreateElement("РозничнаяЦена");
                rprice.SetAttribute("value", ph.RPrice.ToString());
                pos.AppendChild(rprice);
                XmlElement dprice = doc.CreateElement("СкидкаПоКартеСамсонФарма");
                dprice.SetAttribute("value", ph.DPrice.ToString());
                pos.AppendChild(dprice);
                XmlElement prepay = doc.CreateElement("Предзаказ");
                prepay.SetAttribute("value", ph.Prepay);
                pos.AppendChild(prepay);
                tovost.AppendChild(pos);
                i++;
            }
            root.AppendChild(tovost);
            doc.Save(@"\\s-ftp\c$\FTP\samson-f\ishop0.xml");
        }


        static void MakeXMLFilePharma(List<PharmaHelper> lst)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "WINDOWS-1251", null);
            XmlElement root = doc.CreateElement("STOCK");
            doc.InsertBefore(dec, doc.DocumentElement);
            root.SetAttribute("STOCK_DATE", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            root.SetAttribute("ALL_STOCK", 1.ToString());
            doc.AppendChild(root);

            foreach(PharmaHelper ph in lst)
            {
                XmlElement ST = doc.CreateElement("ST");
                ST.SetAttribute("IID", ph.IID.ToString());
                ST.SetAttribute("MED_ID", ph.MedId.ToString());
                ST.SetAttribute("MED_NAME", ph.MedName.ToString());
                ST.SetAttribute("VENDOR_NAME", ph.VendorName.ToString());
                ST.SetAttribute("COUNTRY_NAME", ph.CountryName.ToString());
                ST.SetAttribute("RPRICE", ph.RPrice.ToString());
                ST.SetAttribute("DPRICE", ph.DPrice.ToString());
                ST.SetAttribute("QTTY", ph.Qtty.ToString());
                ST.SetAttribute("KIND_ID", ph.KindID.ToString());
                ST.SetAttribute("KIND_NAME", ph.KindName.ToString());
                ST.SetAttribute("VENDORBARCODE", ph.VendorBarCode.ToString());
                ST.SetAttribute("PHARM_ID", ph.PharmId.ToString());
                ST.SetAttribute("PHARM_NAME", ph.PharmName.ToString());
                ST.SetAttribute("IS_NARC", "0");
                ST.SetAttribute("TK_ID", ph.TKId.ToString());
                ST.SetAttribute("TK_NAME", ph.TKName.ToString());
                ST.SetAttribute("TK_ID", ph.TKId.ToString());
                ST.SetAttribute("TKR_ID", ph.TKRId.ToString());
                ST.SetAttribute("TKR_NAME", ph.TKRName.ToString());
                ST.SetAttribute("PREPAY_ENABLE", ph.PrepayEnable.ToString());
                root.AppendChild(ST);
            }
            doc.Save(@"\\s-ftp\c$\FTP\samson-f\stock_9029.xml");
        }
    }


    class PharmaHelper
    {
        public long IID { get; set; }
        public long MedId { get; set; }
        public string MedName { get; set; }
        public string VendorName { get; set; }
        public string CountryName { get; set; }
        public decimal RPrice { get; set; }
        public decimal DPrice { get; set; }
        public int Qtty { get; set; }
        public int KindID { get; set; }
        public string KindName { get; set; }
        public decimal VendorBarCode { get; set; }
        public int PharmId { get; set; }
        public string PharmName { get; set; }
        public string IsNarc { get; set; }
        public int TKId { get; set; }
        public string TKName { get; set; }
        public int TKRId { get; set; }
        public string TKRName { get; set; }
        public int PrepayEnable { get; set; }

        public PharmaHelper()
        {
            PrepayEnable = 0;
        }
    }

    class SFHelper
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Country { get; set; }
        public string Type { get; set; }
        public string prop1001 { get; set; }
        public string MNN { get; set; }
        public decimal RPrice { get; set; }
        public decimal DPrice { get; set; }
        
        public string Prepay { get; set; }


        public SFHelper()
        {
            prop1001 = "";
            Prepay = "0";
        }
    }


}
