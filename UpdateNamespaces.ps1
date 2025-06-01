# PowerShell script to update namespace references from MicrofinanceApp to FinPlus
$projectPath = "C:\Users\oyini\OneDrive\Desktop\Microfinance App"

# Get all .cs, .cshtml, and .json files
$files = Get-ChildItem -Path $projectPath -Recurse -Include "*.cs", "*.cshtml", "*.json" -File

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Replace namespace references
    $updatedContent = $content -replace "namespace MicrofinanceApp", "namespace FinPlus"
    $updatedContent = $updatedContent -replace "using MicrofinanceApp\.", "using FinPlus."
    $updatedContent = $updatedContent -replace "MicrofinanceApp\.", "FinPlus."
    $updatedContent = $updatedContent -replace "@model MicrofinanceApp\.", "@model FinPlus."
    $updatedContent = $updatedContent -replace "MicrofinanceApp:", "FinPlus:"
    
    # Only write the file if changes were made
    if ($updatedContent -ne $content) {
        Write-Host "Updating references in: $($file.FullName)"
        Set-Content -Path $file.FullName -Value $updatedContent -NoNewline
    }
}

Write-Host "Namespace replacement complete!"
