using System.Diagnostics;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

public class PostmanTests : IDisposable
{
    private const int _port = 5000;
    private Process? _adminServiceProcess = null;
    private readonly HttpClient _httpClient = new HttpClient();
    
    public void Dispose()
    {
        // Stop the AdminService process if it's running
        if (_adminServiceProcess != null && !_adminServiceProcess.HasExited)
        {
            try
            {
                _adminServiceProcess.Kill(entireProcessTree: true);
                _adminServiceProcess.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping AdminService process: {ex.Message}");
            }
        }
        
        _httpClient.Dispose();
    }

    private async Task StartAdminServiceAsync()
    {
        Console.WriteLine("Starting AdminService process...");
        
        // Get path to the AdminService executable
        var projectDir = Directory.GetCurrentDirectory();
        var adminServiceExePath = Path.Combine(projectDir, "..\\..\\..\\..\\..\\src\\AdminService\\bin\\Debug\\net9.0\\AdminService.exe");
        
        if (!File.Exists(adminServiceExePath))
        {
            Console.WriteLine($"AdminService executable not found at {adminServiceExePath}, trying to build it...");
            
            // Build the project if not already built
            var buildProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "build ..\\..\\..\\..\\..\\src\\AdminService\\AdminService.csproj",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };
            
            buildProcess.Start();
            await buildProcess.WaitForExitAsync();
            
            if (buildProcess.ExitCode != 0)
            {
                throw new Exception("Failed to build AdminService");
            }
        }
        
        // Start the service with explicit URL on port 5000 for testing and collect coverage
        _adminServiceProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "..\\..\\..\\..\\..\\src\\AdminService\\bin\\Debug\\net9.0\\AdminService.dll --urls=http://localhost:5000 --environment=Development",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                Environment = {
                    { "DOTNET_EnableDiagnostics", "1" }
                }
            }
        };
        
        _adminServiceProcess.OutputDataReceived += (sender, e) => { 
            if (e.Data != null) Console.WriteLine($"AdminService: {e.Data}"); 
        };
        
        _adminServiceProcess.ErrorDataReceived += (sender, e) => { 
            if (e.Data != null) Console.WriteLine($"AdminService Error: {e.Data}"); 
        };
        
        _adminServiceProcess.Start();
        _adminServiceProcess.BeginOutputReadLine();
        _adminServiceProcess.BeginErrorReadLine();
        
        Console.WriteLine($"AdminService process started with PID: {_adminServiceProcess.Id}");
    }
    
    private async Task WaitForServerReadyAsync()
    {
        Console.WriteLine("Waiting for server to be ready...");
        
        int maxRetries = 30;
        int retryCount = 0;
        bool isReady = false;
        
        while (!isReady && retryCount < maxRetries)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:{_port}/health");
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                {
                    isReady = true;
                    Console.WriteLine("Server is ready!");
                }
                else
                {
                    retryCount++;
                    Console.WriteLine($"Server not ready yet. Status: {response.StatusCode}. Retry {retryCount}/{maxRetries}");
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Error connecting to server: {ex.Message}. Retry {retryCount}/{maxRetries}");
                await Task.Delay(1000);
            }
        }
        
        if (!isReady)
        {
            throw new Exception("Server failed to start in a reasonable time");
        }
    }

    [Fact]
    public async Task RunPostmanCollection_ShouldSucceed()
    {
        // Start AdminService as a separate process
        await StartAdminServiceAsync();
        
        try
        {
            Console.WriteLine($"Server is running on http://localhost:{_port}");

            // Wait for the server to be ready
            await WaitForServerReadyAsync();

            // Verify if Postman environment file exists
            var environmentPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../net9.0/Postman/PracticesLocal.postman_environment.json"));
            var collectionPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../net9.0/Postman/Practices.postman_collection.json"));

            Console.WriteLine($"Using Postman collection: {collectionPath}");
            Console.WriteLine($"Using Postman environment: {environmentPath}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c newman run \"{collectionPath}\" -e \"{environmentPath}\" --env-var \"baseUrl=http://localhost:{_port}\" --reporters cli,htmlextra --reporter-htmlextra-export \"../newman-report.html\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            Console.WriteLine("Starting Newman process...");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Newman Output:");
            Console.WriteLine(output);
            
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Newman Error:");
                Console.WriteLine(error);
            }

            Assert.True(process.ExitCode == 0, $"Newman failed with exit code {process.ExitCode}");
        }
        finally
        {
            // Ensure the admin service is stopped
            if (_adminServiceProcess != null && !_adminServiceProcess.HasExited)
            {
                Console.WriteLine("Stopping AdminService process...");
                _adminServiceProcess.Kill(entireProcessTree: true);
            }
        }
    }
}
