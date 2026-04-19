param(
    [string]$ConfigType = "Code"
)

# 设置变量
$PACKAGE = "Packages/cn.etetet.config"
$CONFIG_NAME = "Config"


$WORKSPACE = "Packages/cn.etetet.yiuiluban"
$GEN_CLIENT = "Packages/cn.etetet.yiuiluban/.Tools/Luban/Luban.dll"
$CUSTOM = "Packages/cn.etetet.yiuiluban/.ToolsGen/Custom"

# powershell判断是不是Mac平台
$DotNet = "dotnet.exe"
if ($null -ne $IsMacOS) {
    $DotNet = "/usr/local/share/dotnet/dotnet"
}

function Invoke-ConfigExport
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$TargetName,

        [Parameter(Mandatory = $true)]
        [string]$OutputCodeDir,

        [Parameter(Mandatory = $true)]
        [string]$OutputDataDir
    )

    & $DotNet $GEN_CLIENT --customTemplateDir $CUSTOM -t $TargetName -c cs-code -d cs-code-data --conf $PACKAGE/Luban/$CONFIG_NAME/luban.conf -x outputCodeDir=$OutputCodeDir -x outputDataDir=$OutputDataDir -x configGroup=$CONFIG_NAME
    Write-Host "==================== $TargetName 完成 ===================="
}

# 客户端
Invoke-ConfigExport -TargetName client -OutputCodeDir "$PACKAGE/CodeMode/Model/Client/$CONFIG_NAME/" -OutputDataDir "$PACKAGE/CodeMode/Config/Client/$CONFIG_NAME/"

# 服务器
Invoke-ConfigExport -TargetName server -OutputCodeDir "$PACKAGE/CodeMode/Model/Server/$CONFIG_NAME/" -OutputDataDir "$PACKAGE/CodeMode/Config/Server/$CONFIG_NAME/"

# 所有
Invoke-ConfigExport -TargetName all -OutputCodeDir "$PACKAGE/CodeMode/Model/ClientServer/$CONFIG_NAME/" -OutputDataDir "$PACKAGE/CodeMode/Config/ClientServer/$CONFIG_NAME/"
