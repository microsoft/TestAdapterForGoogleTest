param (
    [string]$buildArtifactStagingDirectory
)

$filesToScan = @("GoogleTestAdapter.Common.dll", "GoogleTestAdapter.Common.Dynamic.dll", "GoogleTestAdapter.Core.dll", "GoogleTestAdapter.DiaResolver.dll", "GoogleTestAdapter.TestAdapter.dll", "GoogleTestAdapter.VsPackage.TAfGT.dll", "NewProjectWizard.dll", "gtest.dll", "gtestd.dll", "gtest_main.dll", "gtest_maind.dll")
$FilesToScanDrop = "$buildArtifactStagingDirectory/FilesToScanDrop"

foreach ($file in $filesToScan) {
    $sourcePaths = Get-ChildItem -Path "**/out/binaries/" -Recurse -Include $file -File
    foreach ($sourcePath in $sourcePaths) {
        $destinationPath = Join-Path $FilesToScanDrop $sourcePath.Name
        Copy-Item $sourcePath.FullName $destinationPath
    }
}