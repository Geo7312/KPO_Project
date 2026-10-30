param(
    [string]$ResultsDirectory = 'TestResults',
    [string]$OutputPath = 'artifacts/metrics.json',
    [string]$Stage = 'implementation'
)
$ErrorActionPreference = 'Stop'
$trx = Get-ChildItem -Path $ResultsDirectory -Recurse -Filter '*.trx' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
$coverage = Get-ChildItem -Path $ResultsDirectory -Recurse -Filter 'coverage.cobertura.xml' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (-not $trx -or -not $coverage) { throw 'Test result or coverage file not found.' }
[xml]$testXml = Get-Content -Raw -LiteralPath $trx.FullName
[xml]$coverageXml = Get-Content -Raw -LiteralPath $coverage.FullName
$counters = $testXml.TestRun.ResultSummary.Counters
$results = @($testXml.TestRun.Results.UnitTestResult)
$duration = 0.0
foreach ($result in $results) {
    if ($result.duration) { $duration += [TimeSpan]::Parse($result.duration).TotalSeconds }
}
$warningCount = 0
if (Test-Path -LiteralPath 'build.log') {
    $warningCount = @(Select-String -Path 'build.log' -Pattern 'warning (CS|CA|IDE)[0-9]+' -AllMatches).Count
}
$commitCount = [int](git rev-list --count HEAD)
$metrics = [ordered]@{
    stage = $Stage
    commit = (git rev-parse HEAD).Trim()
    measuredAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    testsTotal = [int]$counters.total
    testsPassed = [int]$counters.passed
    testsFailed = [int]$counters.failed
    testDurationSeconds = [math]::Round($duration, 3)
    lineCoveragePercent = [math]::Round(100 * [double]::Parse($coverageXml.coverage.'line-rate', [cultureinfo]::InvariantCulture), 2)
    branchCoveragePercent = [math]::Round(100 * [double]::Parse($coverageXml.coverage.'branch-rate', [cultureinfo]::InvariantCulture), 2)
    buildWarningCount = $warningCount
    commitCount = $commitCount
}
$parent = Split-Path -Parent $OutputPath
if ($parent) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
$metrics | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath $OutputPath -Encoding utf8
$metrics | Format-Table -AutoSize
