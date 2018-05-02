using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace PayrollApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //PaySlip ps = new PaySlip(2018, 2);
            //ps.GetOutputDirPath();
            //ps.GetPayslipDirPath("ps_baby");

            int year, month;
            DataCollection dataCollection = new DataCollection();
            year = dataCollection.GetYear();
            month = dataCollection.GetMonth();

            // get staff from file
            List<Staff> staffList = new List<Staff>();
            FileReader fr = new FileReader();
            staffList = fr.ReadFile();

            // input staff data
            staffList = dataCollection.InputStaffData(staffList);

            // write results to reports
            PaySlip ps = new PaySlip(year, month);
            ps.GeneratePaySlip(staffList);
            ps.GenerateSummary(staffList);

            Console.WriteLine("Payroll App complete.");
        }
    }

    class Staff
    {
        // fields
        private float hourlyRate;  // $10
        private int hWorked;  // per pay period

        // properties
        public float BasicPay { get; private set; }  // per pay period
        public float TotalPay { get; protected set; }
        public string NameOfStaff { get; private set; }
        public int HoursWorked
        {
            get
            {
                return hWorked;
            }
            set
            {
                if (value > 0)
                    hWorked = value;
                else
                    hWorked = 0;
            }
        }

        // constructor
        public Staff(string name, float rate)
        {
            NameOfStaff = name;
            hourlyRate = rate;
        }

        // methods
        public virtual void CalculatePay()
        {
            Console.WriteLine("Calculating Pay...");
            BasicPay = hWorked * hourlyRate;
            TotalPay = BasicPay;
        }

        public override string ToString()
        {
            return $"<{NameOfStaff}, {hourlyRate:C}, {hWorked}, {BasicPay:C}, {TotalPay:C}>";
        }
    }

    class Manager : Staff
    {
        // parent properties of which we have access; parent fields are private
        // BasicPay, TotalPay, HoursWorked, NameOfStaff

        // fields
        private const float managerHourlyRate = 50;

        // properies
        public int Allowance { get; private set; }

        // constructor
        public Manager(string name) : base(name, managerHourlyRate){}

        // methods
        public override void CalculatePay()
        {
            base.CalculatePay();
            Allowance = 0;
            if (HoursWorked > 160)
            {
                Allowance = 1000;
                TotalPay = BasicPay + Allowance;
                Debug.Assert(TotalPay > BasicPay);
            }
        }

		public override string ToString()
		{
            return String.Concat(
                $"<{NameOfStaff} the manager, ",
                $"hourlyRate={managerHourlyRate:C}, ",
                $"HoursWorked={HoursWorked}, ",
                $"BasicPay={BasicPay:C}, ",
                $"AllowancePay={Allowance:C}, ",
                $"TotalPay={TotalPay:C}>"
            );
		}
	}

    class Admin : Staff
    {
        // fields
        private const float overtimeRate = 15.5f;
        private const float adminHourlyRate = 30f;

        // properties
        public float Overtime { get; private set; }

        // constructor
        public Admin(string name) : base(name, adminHourlyRate){}

        // methods
        public override void CalculatePay()
        {
            base.CalculatePay();
            //Overtime = 0;  // I think default is 0
            if (HoursWorked > 160)
            {
                Overtime = overtimeRate * (HoursWorked - 160);
                TotalPay = BasicPay + Overtime;
                Debug.Assert(TotalPay > BasicPay);
            }
        }

        public override string ToString()
        {
            return String.Concat(
                $"<{NameOfStaff} the admin, ",
                $"hourlyRate={adminHourlyRate:C}, ",
                $"HoursWorked={HoursWorked}, ",
                $"BasicPay={BasicPay:C}, ",
                $"OvertimePay={Overtime:C}, ",
                $"TotalPay={TotalPay:C}>"
            );
        }

    }

    class FileReader
    {
        // methods
        public List<Staff> ReadFile()
        {
            List<Staff> myStaff = new List<Staff>();
            string[] results = new string[2];
            string path = "staff.txt";
            string[] separator = { ", " };

            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        string lineString = sr.ReadLine();
                        results = lineString.Split(separator, StringSplitOptions.RemoveEmptyEntries);  // None

                        if (results[1] == "Manager")
                            myStaff.Add(new Manager(results[0]));
                        else if (results[1] == "Admin")
                            myStaff.Add(new Admin(results[0]));
                    }
                    sr.Close();
                }
            } else
            {
                Console.WriteLine("No file called {0} found.", path);
            }
            return myStaff;
        }
    }

    class PaySlip
    {
        // fields
        private int year;
        private int month;

        // enum
        enum MonthsOfYear { JAN = 1, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, NOV, DEC }

        // no properties

        // constructor
        public PaySlip(int payYear, int payMonth)
        {
            year = payYear;
            month = payMonth;
        }

        // methods
        public void GeneratePaySlip(List<Staff> myStaff, string payslipDirName = "payslips")
        {
            string payslipDirPath = GetPayslipDirPath(payslipDirName);
            string path;

            foreach (Staff f in myStaff)
            {
                path = Path.Combine(payslipDirPath, $"{f.NameOfStaff}.txt");
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("PAYSLIP FOR {0} {1}", (MonthsOfYear)month, year);
                    sw.WriteLine("====================");
                    sw.WriteLine("Name of Staff: {0}", f.NameOfStaff);
                    sw.WriteLine("Hours Worked: {0}", f.HoursWorked);
                    sw.WriteLine("");
                    sw.WriteLine("Basic Pay: {0:C}", f.BasicPay);

                    if (f.GetType() == typeof(Manager))
                        //Console.WriteLine((Manager)f);
                        sw.WriteLine("Allowance: {0:C}", ((Manager)f).Allowance);
                    else if (f.GetType() == typeof(Admin))
                        sw.WriteLine("Overtime: {0:C}", ((Admin)f).Overtime);

                    sw.WriteLine("");
                    sw.WriteLine("====================");
                    sw.WriteLine("Total Pay: {0:C}", f.TotalPay);
                    sw.WriteLine("====================");
                    sw.Close();
                }
            }
            Console.WriteLine($"Payslips successfully saved to: {payslipDirPath}");
        }

        public void GenerateSummary(List<Staff> myStaff, string summaryFileName = "summary")
        {
            var result =
                from f in myStaff
                where f.HoursWorked < 10
                orderby f.NameOfStaff ascending
                select new { f.NameOfStaff, f.HoursWorked };

            string outputDirPath = GetOutputDirPath();
            string path = Path.Combine(outputDirPath, $"{summaryFileName}.txt");

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("Staff with less than 10 working hours.");
                foreach (var f in result)
                    sw.WriteLine("Name of Staff: {0}, Hours Worked: {1}",
                                 f.NameOfStaff, f.HoursWorked);
                sw.Close();
            }
            Console.WriteLine($"Summary file saved to: {path}");
        }

        public string GetOutputDirPath(string outputDirName = "output")
        {
            string cwd = Directory.GetCurrentDirectory();
            string outputDirPath = Path.Combine(cwd, outputDirName);
            if (!Directory.Exists(outputDirPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(outputDirPath);
                Console.WriteLine($"Output folder successfully created at {outputDirPath}");
            }
            return outputDirPath;
        }

        public string GetPayslipDirPath(string payslipDirName)
        {
            string outputDirPath = GetOutputDirPath();
            string payslipDirPath = Path.Combine(outputDirPath, payslipDirName);
            if (!Directory.Exists(payslipDirPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(payslipDirPath);
                Console.WriteLine($"Payslip folder successfully created at {payslipDirPath}");
            }
            return payslipDirPath;
        }

		public override string ToString()
		{
            return String.Concat(
                $"<Payroll for month = {month}, ",
                $"year = {year}>"
            );
		}

	}

    class DataCollection
    {
        // methods
        public int GetYear()
        {
            while (true)
            {
                Console.Write("Please enter the year: ");
                try
                {
                    string yrString = Console.ReadLine();
                    if (yrString.Length != 4)
                    {
                        Console.WriteLine("Year must be a 4-digit number.");
                    }
                    else
                    {
                        int year = Convert.ToInt32(yrString);
                        return year;
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine($"{e.Message} Please try again.");
                }
            }
        }

        public int GetMonth()
        {
            while (true)
            {
                Console.Write("Please enter the month: ");
                try
                {
                    string monthString = Console.ReadLine();
                    int month = Convert.ToInt32(monthString);
                    if (month < 1 || month > 12)
                    {
                        Console.WriteLine("Month must be from 1 to 12. Please try again.");
                    }
                    else
                    {
                        return month;
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine($"{e.Message} Please try again.");
                }
            }
        }

        public List<Staff> InputStaffData(List<Staff> staffList)
        {
            foreach (Staff f in staffList)
            {
                while (true)
                {
                    try
                    {
                        Console.Write("Enter hours worked for {0}: ", f.NameOfStaff);
                        f.HoursWorked = Convert.ToInt32(Console.ReadLine());
                        f.CalculatePay();
                        Console.WriteLine($"{f.NameOfStaff} made {f.TotalPay:C} this period.");
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            return staffList;
        }
    }

}
