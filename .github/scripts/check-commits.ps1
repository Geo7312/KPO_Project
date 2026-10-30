$ErrorActionPreference = 'Stop'
$event = Get-Content -Raw -LiteralPath $env:GITHUB_EVENT_PATH | ConvertFrom-Json
$base = if ($env:GITHUB_EVENT_NAME -eq 'pull_request') { $event.pull_request.base.sha } else { $event.before }
if (-not $base -or $base -match '^0+$') {
    $base = (git rev-list --max-parents=0 HEAD | Select-Object -First 1).Trim()
}
$messages = @(git log --format=%s "$base..HEAD")
foreach ($message in $messages) {
    if ($message -cnotmatch '^(feat|fix|docs|test|refactor|chore|ci|build)(\([a-z0-9-]+\))?: .+') {
        throw "Invalid commit message: $message"
    }
}
Write-Host "Checked $($messages.Count) commit message(s)."
