using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using NReco.PdfGenerator;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Services
{
    public class PdfService : IPdfService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPayslipRepository _payslipRepository;

        public PdfService(IEmployeeRepository employeeRepository, IPayslipRepository payslipRepository)
        {
            _employeeRepository = employeeRepository;
            _payslipRepository = payslipRepository;
        }

        // Generate PDF file path by EmployeeId & PayslipId
        public async Task<string> GeneratePayslipPdfAsync(int employeeId, int payslipId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            var payslip = await _payslipRepository.GetPayslipByIdAsync(payslipId);

            if (employee == null || payslip == null)
                throw new Exception("Employee or Payslip not found");

            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ 
                            font-family: Arial, sans-serif; 
                            margin: 40px;
                            color: #333;
                        }}
                        .header {{ 
                            font-size: 24px; 
                            font-weight: bold; 
                            margin-bottom: 30px;
                            text-align: center;
                            color: #2c5aa0;
                            border-bottom: 2px solid #2c5aa0;
                            padding-bottom: 10px;
                        }}
                        .section {{ 
                            margin-bottom: 15px;
                            padding: 5px 0;
                        }}
                        .label {{ 
                            font-weight: bold;
                            display: inline-block;
                            width: 150px;
                        }}
                        .value {{
                            color: #555;
                        }}
                        .salary-section {{
                            margin-top: 30px;
                            padding: 20px;
                            background-color: #f8f9fa;
                            border-radius: 5px;
                        }}
                        .total {{
                            font-size: 18px;
                            font-weight: bold;
                            color: #28a745;
                            margin-top: 10px;
                            padding-top: 10px;
                            border-top: 1px solid #ddd;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Employee Payslip - {payslip.Month ?? "N/A"}</div>
                    
                    <div class='section'>
                        <span class='label'>Employee Name:</span>
                        <span class='value'>{employee.Name}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Employee Code:</span>
                        <span class='value'>{employee.EmployeeCode}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Email:</span>
                        <span class='value'>{employee.Email}</span>
                    </div>
                    
                    <div class='salary-section'>
                        <div class='section'>
                            <span class='label'>Base Salary:</span>
                            <span class='value'>${payslip.BaseSalary:F2}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Allowances:</span>
                            <span class='value'>${payslip.Allowances:F2}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Deductions:</span>
                            <span class='value'>${payslip.Deductions:F2}</span>
                        </div>
                        <div class='total'>
                            <span class='label'>Net Salary:</span>
                            <span class='value'>${payslip.NetSalary:F2}</span>
                        </div>
                    </div>
                    
                    <div style='margin-top: 40px; text-align: center; color: #666; font-size: 12px;'>
                        Generated on {DateTime.Now:MMMM dd, yyyy}
                    </div>
                </body>
                </html>";

            return await GeneratePayslipPdfFromHtmlAsync(htmlContent, payslipId);
        }

        // Generate PDF as byte array
        public async Task<byte[]> GeneratePayslipPdfBytesAsync(int employeeId, int payslipId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            var payslip = await _payslipRepository.GetPayslipByIdAsync(payslipId);

            if (employee == null || payslip == null)
                throw new Exception("Employee or Payslip not found");

            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ 
                            font-family: Arial, sans-serif; 
                            margin: 40px;
                            color: #333;
                        }}
                        .header {{ 
                            font-size: 24px; 
                            font-weight: bold; 
                            margin-bottom: 30px;
                            text-align: center;
                            color: #2c5aa0;
                            border-bottom: 2px solid #2c5aa0;
                            padding-bottom: 10px;
                        }}
                        .section {{ 
                            margin-bottom: 15px;
                            padding: 5px 0;
                        }}
                        .label {{ 
                            font-weight: bold;
                            display: inline-block;
                            width: 150px;
                        }}
                        .value {{
                            color: #555;
                        }}
                        .salary-section {{
                            margin-top: 30px;
                            padding: 20px;
                            background-color: #f8f9fa;
                            border-radius: 5px;
                        }}
                        .total {{
                            font-size: 18px;
                            font-weight: bold;
                            color: #28a745;
                            margin-top: 10px;
                            padding-top: 10px;
                            border-top: 1px solid #ddd;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Employee Payslip - {payslip.Month ?? "N/A"}</div>
                    
                    <div class='section'>
                        <span class='label'>Employee Name:</span>
                        <span class='value'>{employee.Name}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Employee Code:</span>
                        <span class='value'>{employee.EmployeeCode}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Email:</span>
                        <span class='value'>{employee.Email}</span>
                    </div>
                    
                    <div class='salary-section'>
                        <div class='section'>
                            <span class='label'>Base Salary:</span>
                            <span class='value'>${payslip.BaseSalary:F2}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Allowances:</span>
                            <span class='value'>${payslip.Allowances:F2}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Deductions:</span>
                            <span class='value'>${payslip.Deductions:F2}</span>
                        </div>
                        <div class='total'>
                            <span class='label'>Net Salary:</span>
                            <span class='value'>${payslip.NetSalary:F2}</span>
                        </div>
                    </div>
                    
                    <div style='margin-top: 40px; text-align: center; color: #666; font-size: 12px;'>
                        Generated on {DateTime.Now:MMMM dd, yyyy}
                    </div>
                </body>
                </html>";

            return GeneratePdfBytesFromHtml(htmlContent);
        }

        // Generate PDF and save to server
        public async Task<string> GeneratePayslipPdfFromHtmlAsync(string htmlContent, int payslipId)
        {
            var filePath = Path.Combine("Uploads", "Payslips", $"Payslip_{payslipId}.pdf");
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
        }

        // Convert HTML to PDF bytes using NReco.PdfGenerator
        public byte[] GeneratePdfBytesFromHtml(string htmlContent)
        {
            try
            {
                var htmlToPdf = new HtmlToPdfConverter();

                // Configure PDF settings
                htmlToPdf.Size = PageSize.A4;
                htmlToPdf.Orientation = PageOrientation.Portrait;

                // FIXED: Use property initialization instead of constructor arguments
                htmlToPdf.Margins = new PageMargins
                {
                    Top = 10,    // 10mm top margin
                    Bottom = 10, // 10mm bottom margin
                    Left = 10,   // 10mm left margin
                    Right = 10   // 10mm right margin
                };

                return htmlToPdf.GeneratePdf(htmlContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }
        }
    }
}
