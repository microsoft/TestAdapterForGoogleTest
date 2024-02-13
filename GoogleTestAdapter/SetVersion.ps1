Param([parameter(Mandatory=$true)] [string] $version)

$common_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Common\Properties\AssemblyInfo.cs"

$common_dynamic_gta_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Common.Dynamic.GTA\Properties\AssemblyInfo.cs"
$common_dynamic_tafgt_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Common.Dynamic.TAfGT\Properties\AssemblyInfo.cs"

$core_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Core\Properties\AssemblyInfo.cs"
$coretests_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Core.Tests\Properties\AssemblyInfo.cs"

$dia_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\DiaResolver\Properties\AssemblyInfo.cs"
$diatests_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\DiaResolver.Tests\Properties\AssemblyInfo.cs"

$packaging_gta_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Packaging.GTA\Properties\AssemblyInfo.cs"
$packaging_tafgt_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\Packaging.TAfGT\Properties\AssemblyInfo.cs"
$projecttemplates_vstemplate = "TestAdapterForGoogleTest\GoogleTestAdapter\GoogleTestProjectTemplate\GoogleTest.vstemplate"

$testadapter_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\TestAdapter\Properties\AssemblyInfo.cs"
$testadaptertests_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\TestAdapter.Tests\Properties\AssemblyInfo.cs"

$vspackage_gta_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\VsPackage.GTA\Properties\AssemblyInfo.cs"
$vspackage_gta_unittests_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\VsPackage.GTA.Tests.Unit\Properties\AssemblyInfo.cs"
$vspackage_tafgt_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\VsPackage.TAfGT\Properties\AssemblyInfo.cs"
$vspackagetests_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\VsPackage.Tests\Properties\AssemblyInfo.cs"
$vspackagegeneratedtests_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\VsPackage.Tests.Generated\Properties\AssemblyInfo.cs"

$vsix_manifest_gta = "TestAdapterForGoogleTest\GoogleTestAdapter\Packaging.GTA\source.extension.vsixmanifest"
$vsix_manifest_tafgt = "TestAdapterForGoogleTest\GoogleTestAdapter\Packaging.TAfGT\source.extension.vsixmanifest"

$wizard_assembly_info = "TestAdapterForGoogleTest\GoogleTestAdapter\NewProjectWizard\Properties\AssemblyInfo.cs"

(Get-Content $common_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $common_assembly_info

(Get-Content $common_dynamic_gta_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $common_dynamic_gta_assembly_info
(Get-Content $common_dynamic_tafgt_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $common_dynamic_tafgt_assembly_info

(Get-Content $core_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $core_assembly_info
(Get-Content $coretests_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $coretests_assembly_info

(Get-Content $dia_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $dia_assembly_info
(Get-Content $diatests_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $diatests_assembly_info

(Get-Content $packaging_gta_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $packaging_gta_assembly_info
(Get-Content $packaging_tafgt_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $packaging_tafgt_assembly_info
(Get-Content $projecttemplates_vstemplate) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $projecttemplates_vstemplate

(Get-Content $testadapter_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $testadapter_assembly_info
(Get-Content $testadaptertests_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $testadaptertests_assembly_info

(Get-Content $vspackage_gta_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vspackage_gta_assembly_info
(Get-Content $vspackage_gta_unittests_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vspackage_gta_unittests_assembly_info
(Get-Content $vspackage_tafgt_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vspackage_tafgt_assembly_info
(Get-Content $vspackagetests_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vspackagetests_assembly_info
(Get-Content $vspackagegeneratedtests_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vspackagegeneratedtests_assembly_info

(Get-Content $vsix_manifest_gta) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vsix_manifest_gta
(Get-Content $vsix_manifest_tafgt) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $vsix_manifest_tafgt

(Get-Content $wizard_assembly_info) | ForEach-Object { $_ -replace "0.1.0.0", $version } | Set-Content $wizard_assembly_info
