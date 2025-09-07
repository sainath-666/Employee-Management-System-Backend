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

        // Generate PDF file path by EmployeeId & PayslipId
        public async Task<string> GeneratePayslipPdfAsync(int employeeId, int payslipId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            var payslip = await _payslipRepository.GetPayslipByIdAsync(payslipId);

            if (employee == null || payslip == null)
                throw new Exception("Employee or Payslip not found");

            var htmlContent = GeneratePayslipHtmlFromDatabase(employee, payslip);

            // FIXED: Generate organized file path and update database
            var fileName = $"Payslip_{employeeId}_{payslipId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var folderPath = Path.Combine("Uploads", "Payslips");

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = Path.Combine(folderPath, fileName);
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            // Save PDF to organized folder
            await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

            // FIXED: Store organized path in database (use forward slashes for web compatibility)
            var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
            await _payslipRepository.UpdatePdfPathAsync(payslipId, databasePath);

            return fullFilePath;
        }

        // Generate PDF as byte array
        public async Task<byte[]> GeneratePayslipPdfBytesAsync(int employeeId, int payslipId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            var payslip = await _payslipRepository.GetPayslipByIdAsync(payslipId);

            if (employee == null || payslip == null)
                throw new Exception("Employee or Payslip not found");

            var htmlContent = GeneratePayslipHtmlFromDatabase(employee, payslip);
            return GeneratePdfBytesFromHtml(htmlContent);
        }

        // FIXED: Generate PDF from frontend request data and save to server with DB update
        public async Task<string> GeneratePayslipFromRequestAsync(PayslipRequest request)
        {
            // Get employee details if not provided
            if (string.IsNullOrEmpty(request.EmployeeName) || string.IsNullOrEmpty(request.Email))
            {
                var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
                if (employee == null)
                    throw new Exception("Employee not found");

                request.EmployeeName = employee.Name;
                request.EmployeeCode = employee.EmployeeCode;
                request.Email = employee.Email;
            }

            var htmlContent = GeneratePayslipHtmlFromRequest(request);

            // Generate organized file path
            var fileName = $"Payslip_{request.EmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var folderPath = Path.Combine("Uploads", "Payslips");

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = Path.Combine(folderPath, fileName);
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            // Save PDF to organized folder
            await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

            // Return organized path for database storage (use forward slashes for web compatibility)
            var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
            return databasePath;
        }

        // Generate PDF bytes from frontend request data
        public async Task<byte[]> GeneratePayslipBytesFromRequestAsync(PayslipRequest request)
        {
            // Get employee details if not provided
            if (string.IsNullOrEmpty(request.EmployeeName) || string.IsNullOrEmpty(request.Email))
            {
                var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
                if (employee == null)
                    throw new Exception("Employee not found");

                request.EmployeeName = employee.Name;
                request.EmployeeCode = employee.EmployeeCode;
                request.Email = employee.Email;
            }

            var htmlContent = GeneratePayslipHtmlFromRequest(request);
            return GeneratePdfBytesFromHtml(htmlContent);
        }

        // FIXED: Generate PDF from JSON string and save to server with organized path
        public async Task<string> GeneratePayslipFromJsonAsync(string jsonData)
        {
            try
            {
                // Deserialize JSON to PayslipRequest object
                var request = JsonSerializer.Deserialize<PayslipRequest>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                    throw new Exception("Invalid JSON data provided");

                if (request.EmployeeId <= 0)
                    throw new Exception("Valid Employee ID is required in JSON");

                // Get employee details if not provided in JSON
                if (string.IsNullOrEmpty(request.EmployeeName) || string.IsNullOrEmpty(request.Email))
                {
                    var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
                    if (employee == null)
                        throw new Exception($"Employee with ID {request.EmployeeId} not found");

                    request.EmployeeName = employee.Name;
                    request.EmployeeCode = employee.EmployeeCode;
                    request.Email = employee.Email;
                }

                var htmlContent = GeneratePayslipHtmlFromRequest(request);

                // Generate organized file path
                var fileName = $"Payslip_{request.EmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var folderPath = Path.Combine("Uploads", "Payslips");

                // Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fullFilePath = Path.Combine(folderPath, fileName);
                var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

                // Save PDF to organized folder
                await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

                // Return organized path for database storage (use forward slashes for web compatibility)
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

        // Generate PDF bytes from JSON string
        public async Task<byte[]> GeneratePayslipBytesFromJsonAsync(string jsonData)
        {
            try
            {
                // Deserialize JSON to PayslipRequest object
                var request = JsonSerializer.Deserialize<PayslipRequest>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                    throw new Exception("Invalid JSON data provided");

                if (request.EmployeeId <= 0)
                    throw new Exception("Valid Employee ID is required in JSON");

                // Get employee details if not provided in JSON
                if (string.IsNullOrEmpty(request.EmployeeName) || string.IsNullOrEmpty(request.Email))
                {
                    var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
                    if (employee == null)
                        throw new Exception($"Employee with ID {request.EmployeeId} not found");

                    request.EmployeeName = employee.Name;
                    request.EmployeeCode = employee.EmployeeCode;
                    request.Email = employee.Email;
                }

                var htmlContent = GeneratePayslipHtmlFromRequest(request);
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

        // FIXED: Generate PDF and save to server with organized path and DB update
        public async Task<string> GeneratePayslipPdfFromHtmlAsync(string htmlContent, int payslipId)
        {
            // Generate organized file path
            var fileName = $"Payslip_{payslipId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var folderPath = Path.Combine("Uploads", "Payslips");

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullFilePath = Path.Combine(folderPath, fileName);
            var pdfBytes = GeneratePdfBytesFromHtml(htmlContent);

            // Save PDF to organized folder
            await File.WriteAllBytesAsync(fullFilePath, pdfBytes);

            // FIXED: Store organized path in database (use forward slashes for web compatibility)
            var databasePath = Path.Combine("Uploads", "Payslips", fileName).Replace("\\", "/");
            await _payslipRepository.UpdatePdfPathAsync(payslipId, databasePath);

            return fullFilePath;
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

                // Use property initialization instead of constructor arguments
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

        // PRIVATE: Generate HTML content from database data
        private static string GeneratePayslipHtmlFromDatabase(Employee employee, Payslip payslip)
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
        }

        // PRIVATE: Generate HTML content from frontend request data
        private static string GeneratePayslipHtmlFromRequest(PayslipRequest request)
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
                        Employee Payslip
                        {(!string.IsNullOrEmpty(request.Month) ? $" - {request.Month}" : "")}
                    </div>
                    
                    <div class='employee-info'>
                        <div class='section'>
                            <span class='label'>Employee Name:</span>
                            <span class='value'>{request.EmployeeName ?? "N/A"}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Employee Code:</span>
                            <span class='value'>{request.EmployeeCode ?? "N/A"}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Email Address:</span>
                            <span class='value'>{request.Email ?? "N/A"}</span>
                        </div>
                        <div class='section'>
                            <span class='label'>Employee ID:</span>
                            <span class='value'>{request.EmployeeId}</span>
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
                            <span class='total-amount'>${request.NetSalary:F2}</span>
                        </div>
                    </div>
                    
                    <div class='footer'>
                        <p>Generated on {DateTime.Now:MMMM dd, yyyy} at {DateTime.Now:hh:mm tt}</p>
                        <p>This is a computer-generated document. No signature required.</p>
                    </div>
                </body>
                </html>";
        }
    }
}
