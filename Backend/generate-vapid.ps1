# generate-vapid.ps1
# Generates a new VAPID key pair for Web Push notifications using the WebPush NuGet package.
# Usage: .\generate-vapid.ps1

$tempDir = Join-Path $env:TEMP "vapid-gen-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir | Out-Null

try {
    Push-Location $tempDir

    dotnet new console -n VapidGen --no-restore 2>&1 | Out-Null
    Set-Location VapidGen
    dotnet add package WebPush --version 1.0.12 2>&1 | Out-Null

    @'
using WebPush;

var keys = VapidHelper.GenerateVapidKeys();

Console.WriteLine();
Console.WriteLine("=== VAPID Keys Generated ===");
Console.WriteLine();
Console.WriteLine($"PublicKey:  {keys.PublicKey}");
Console.WriteLine($"PrivateKey: {keys.PrivateKey}");
Console.WriteLine();
Console.WriteLine("Paste into appsettings.json:");
Console.WriteLine();
Console.WriteLine($$"""
  "Vapid": {
    "Subject": "mailto:your@email.com",
    "PublicKey": "{{keys.PublicKey}}",
    "PrivateKey": "{{keys.PrivateKey}}"
  }
""");
'@ | Set-Content Program.cs

    dotnet run
}
finally {
    Pop-Location
    Remove-Item -Recurse -Force $tempDir -ErrorAction SilentlyContinue
}
