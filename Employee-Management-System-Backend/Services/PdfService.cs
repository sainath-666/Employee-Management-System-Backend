using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using NReco.PdfGenerator;
using System.Text;
using System.Text.Json;
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

        // UPDATED: Use PayslipWithEmployee DTO instead of separate queries
        public async Task<string> GeneratePayslipPdfAsync(int employeeId, int payslipId)
        {
            var payslipWithEmployee = await _payslipRepository.GetPayslipWithEmployeeAsync(payslipId);
            if (payslipWithEmployee == null)
                throw new Exception("Payslip not found");

            var htmlContent = GeneratePayslipHtmlFromPayslipWithEmployee(payslipWithEmployee);
            var fileName = $"Payslip_{employeeId}_{payslipId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var folderPath = Path.Combine("Uploads", "Payslips");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = Path.Combine(folderPath, fileName);
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

            // FIXED: Correct escape sequence for backslashes
            var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
            await _payslipRepository.UpdatePdfPathAsync(payslipId, databasePath);

            return fullFilePath;
        }

        public async Task<byte[]> GeneratePayslipPdfBytesAsync(int employeeId, int payslipId)
        {
            var payslipWithEmployee = await _payslipRepository.GetPayslipWithEmployeeAsync(payslipId);
            if (payslipWithEmployee == null)
                throw new Exception("Payslip not found");

            var htmlContent = GeneratePayslipHtmlFromPayslipWithEmployee(payslipWithEmployee);
            return GeneratePdfBytesFromHtml(htmlContent);
        }

        public async Task<string> GeneratePayslipFromRequestAsync(PayslipRequest request)
        {
            var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            var fileName = $"Payslip_{request.EmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var folderPath = Path.Combine("Uploads", "Payslips");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = Path.Combine(folderPath, fileName);
            var htmlContent = GeneratePayslipHtmlFromDatabaseData(employee, request);
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

            // FIXED: Correct escape sequence
            var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
            return databasePath;
        }

        public async Task<byte[]> GeneratePayslipBytesFromRequestAsync(PayslipRequest request)
        {
            var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            var htmlContent = GeneratePayslipHtmlFromDatabaseData(employee, request);
            return GeneratePdfBytesFromHtml(htmlContent);
        }

        public async Task<string> GeneratePayslipFromJsonAsync(string jsonData)
        {
            try
            {
                var request = JsonSerializer.Deserialize<PayslipRequest>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                    throw new Exception("Invalid JSON data provided");

                if (request.EmployeeId <= 0)
                    throw new Exception("Valid Employee ID is required in JSON");

                var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
                if (employee == null)
                    throw new Exception($"Employee with ID {request.EmployeeId} not found");

                var fileName = $"Payslip_{request.EmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var folderPath = Path.Combine("Uploads", "Payslips");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fullFilePath = Path.Combine(folderPath, fileName);
                var htmlContent = GeneratePayslipHtmlFromDatabaseData(employee, request);
                var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

                await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

                // FIXED: Correct escape sequence
                var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
                return databasePath;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid JSON format: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing JSON to PDF: {ex.Message}");
            }
        }

        public async Task<byte[]> GeneratePayslipBytesFromJsonAsync(string jsonData)
        {
            try
            {
                var request = JsonSerializer.Deserialize<PayslipRequest>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                    throw new Exception("Invalid JSON data provided");

                if (request.EmployeeId <= 0)
                    throw new Exception("Valid Employee ID is required in JSON");

                var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
                if (employee == null)
                    throw new Exception($"Employee with ID {request.EmployeeId} not found");

                var htmlContent = GeneratePayslipHtmlFromDatabaseData(employee, request);
                return GeneratePdfBytesFromHtml(htmlContent);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid JSON format: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing JSON to PDF: {ex.Message}");
            }
        }

        public async Task<string> GeneratePayslipPdfFromHtmlAsync(string htmlContent, int payslipId)
        {
            var fileName = $"Payslip_{payslipId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var folderPath = Path.Combine("Uploads", "Payslips");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = Path.Combine(folderPath, fileName);
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

            // FIXED: Correct escape sequence
            var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
            await _payslipRepository.UpdatePdfPathAsync(payslipId, databasePath);

            return fullFilePath;
        }

        public byte[] GeneratePdfBytesFromHtml(string htmlContent)
        {
            try
            {
                var htmlToPdf = new HtmlToPdfConverter();
                htmlToPdf.Size = PageSize.A4;
                htmlToPdf.Orientation = PageOrientation.Portrait;
                htmlToPdf.Margins = new PageMargins
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10
                };
                return htmlToPdf.GeneratePdf(htmlContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }
        }

        // PERFECT: Generate HTML from PayslipWithEmployee DTO including Department Name
        private static string GeneratePayslipHtmlFromPayslipWithEmployee(PayslipWithEmployee payslip)
        {
            return $@"
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
                    <div class='header'>Employee Payslip - {payslip.Month ?? DateTime.Now.ToString("MMMM yyyy")}</div>
                    
                    <div class='section'>
                        <span class='label'>Employee Name:</span>
                        <span class='value'>{payslip.EmployeeName}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Employee Code:</span>
                        <span class='value'>{payslip.EmployeeCode}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Employee ID:</span>
                        <span class='value'>{payslip.EmployeeId}</span>
                    </div>
                    <div class='section'>
                        <span class='label'>Department:</span>
                        <span class='value'>{payslip.DepartmentName ?? "Not Assigned"}</span>
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
        }

        private static string GeneratePayslipHtmlFromDatabaseData(EmployeeWithDepartment employee, PayslipRequest request)
        {
            var payslipMonth = DateTime.Now.ToString("MMMM yyyy");
            var netSalary = request.BaseSalary + request.Allowances - request.Deductions;

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ 
                            font-family: Arial, sans-serif; 
                            margin: 40px;
                            color: #333;
                            line-height: 1.6;
                        }}
                        .header {{ 
                            font-size: 28px; 
                            font-weight: bold; 
                            margin-bottom: 30px;
                            text-align: center;
                            color: #2c5aa0;
                            border-bottom: 3px solid #2c5aa0;
                            padding-bottom: 15px;
                        }}
                        .employee-info {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            border-radius: 8px;
                            margin-bottom: 30px;
                        }}
                        .section {{ 
                            margin-bottom: 15px;
                            padding: 8px 0;
                            display: flex;
                        }}
                        .label {{ 
                            font-weight: bold;
                            width: 180px;
                            color: #2c5aa0;
                        }}
                        .value {{
                            color: #555;
                            flex: 1;
                        }}
                        .salary-section {{
                            margin-top: 30px;
                            padding: 25px;
                            background-color: #ffffff;
                            border: 2px solid #e9ecef;
                            border-radius: 8px;
                        }}
                        .salary-title {{
                            font-size: 20px;
                            font-weight: bold;
                            color: #2c5aa0;
                            margin-bottom: 20px;
                            text-align: center;
                            border-bottom: 1px solid #dee2e6;
                            padding-bottom: 10px;
                        }}
                        .salary-row {{
                            display: flex;
                            justify-content: space-between;
                            padding: 12px 0;
                            border-bottom: 1px solid #f1f3f4;
                        }}
                        .salary-row:last-child {{
                            border-bottom: none;
                        }}
                        .amount {{
                            font-weight: bold;
                            color: #28a745;
                        }}
                        .deduction {{
                            color: #dc3545;
                        }}
                        .total-row {{
                            background-color: #e8f5e8;
                            margin-top: 15px;
                            padding: 15px;
                            border-radius: 5px;
                            border: 2px solid #28a745;
                        }}
                        .total-amount {{
                            font-size: 22px;
                            font-weight: bold;
                            color: #28a745;
                        }}
                        .footer {{
                            margin-top: 50px;
                            text-align: center;
                            color: #6c757d;
                            font-size: 12px;
                            border-top: 1px solid #dee2e6;
                            padding-top: 20px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        Employee Payslip - {payslipMonth}
                    </div>
                    
                    <div class='employee-info'>
                        <div class='section'>
                            <span class='label'>Employee Name:</span>
                            <span class='value'>{employee.Name}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Employee Code:</span>
                            <span class='value'>{employee.EmployeeCode}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Email Address:</span>
                            <span class='value'>{employee.Email}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Employee ID:</span>
                            <span class='value'>{employee.Id}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Department:</span>
                            <span class='value'>{employee.DepartmentName ?? "Not Assigned"}</span>
                        </div>
                    </div>
                    
                    <div class='salary-section'>
                        <div class='salary-title'>Salary Breakdown</div>
                        
                        <div class='salary-row'>
                            <span class='label'>Base Salary:</span>
                            <span class='amount'>${request.BaseSalary:F2}</span>
                        </div>
                        
                        <div class='salary-row'>
                            <span class='label'>Allowances:</span>
                            <span class='amount'>${request.Allowances:F2}</span>
                        </div>
                        
                        <div class='salary-row'>
                            <span class='label'>Deductions:</span>
                            <span class='amount deduction'>-${request.Deductions:F2}</span>
                        </div>
                        
                        <div class='total-row salary-row'>
                            <span class='label' style='color: #28a745; font-size: 18px;'>Net Salary:</span>
                            <span class='total-amount'>${netSalary:F2}</span>
                        </div>
                    </div>
                    
                    <div class='footer'>
                        <p>Generated on {DateTime.Now:MMMM dd, yyyy} at {DateTime.Now:hh:mm tt}</p>
                        <p>This is a computer-generated document. No signature required.</p>
                        <p>Employee Management System</p>
                    </div>
                </body>
                </html>";
        }
    }
}
