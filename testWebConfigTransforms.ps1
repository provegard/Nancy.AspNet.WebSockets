#
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$testPath = (join-path $scriptPath "webConfigTransformTestFiles")
$transformPath = (join-path $scriptPath "src\Nancy.AspNet.WebSockets\install")
$testTool = (join-path $scriptPath "src\WebConfigTransformTester\bin\Release\WebConfigTransformTester.exe")

$tests = 0
$failures = 0
Get-ChildItem $testPath -Filter *.input.config -recurse | foreach-object {

  $subFolder = $_.Directory.Name
  $transformFile = (join-path $transformPath "Web.config.$subFolder.xdt")
  
  $dirName = $_.Directory.Name
  $input = $_.FullName
  $output = $_.FullName -replace ".input.config",".output.config"
  
  
  write-host "==> Testing $dirName\$_ ..." -foreground yellow
  
  & $testTool $input $transformFile $output
  if ($LastExitCode -ne 0) {
    $failures++
    write-host "==> Failure"
  } else {
    write-host "==> Success"
  }
  
  $tests++
}

write-host "$tests test(s) run, of which $failures failed"
exit $failures