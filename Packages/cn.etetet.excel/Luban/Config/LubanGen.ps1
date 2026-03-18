# 参数定义
param(
    [string]$ConfigType = "Luban"
)

# 验证参数
if ($ConfigType -notin @("Luban", "Json")) {
    Write-Error "Invalid ConfigType: $ConfigType. Must be 'Luban' or 'Json'"
    exit 1
}

# 设置变量
$PACKAGE = "Packages/cn.etetet.excel"
$CONFIG_NAME = "Config"


$WORKSPACE = "Packages/cn.etetet.yiuiluban"
$GEN_CLIENT = "Packages/cn.etetet.yiuiluban/.Tools/Luban/Luban.dll"
$CUSTOM = "Packages/cn.etetet.yiuiluban/.ToolsGen/Custom"

# powershell判断是不是Mac平台
$DotNet = "dotnet.exe"
if ($null -ne $IsMacOS) {
    $DotNet = "/usr/local/share/dotnet/dotnet"
}

# 根据ConfigType选择输出格式
$OutputFormats
if ($ConfigType -eq "Luban") {
    $OutputFormats = "cs-bin"
} else {
    $OutputFormats = "cs-code"
}

# 客户端
& $DotNet $GEN_CLIENT --customTemplateDir $CUSTOM -t client -c cs-code -d cs-code-data --conf $PACKAGE/Luban/$CONFIG_NAME/luban.conf -x outputCodeDir=$PACKAGE/CodeMode/Model/Client/$CONFIG_NAME/ -x outputDataDir=$PACKAGE/CodeMode/Hotfix/Client/$CONFIG_NAME/
Write-Host "==================== 客户端 完成 ===================="

# 服务器
& $DotNet $GEN_CLIENT --customTemplateDir $CUSTOM -t server -c cs-code -d cs-code-data --conf $PACKAGE/Luban/$CONFIG_NAME/luban.conf -x outputCodeDir=$PACKAGE/CodeMode/Model/Server/$CONFIG_NAME/ -x outputDataDir=$PACKAGE/CodeMode/Hotfix/Server/$CONFIG_NAME/
Write-Host "==================== 服务器 完成 ===================="

# 所有
& $DotNet $GEN_CLIENT --customTemplateDir $CUSTOM -t all -c cs-code -d cs-code-data --conf $PACKAGE/Luban/$CONFIG_NAME/luban.conf -x outputCodeDir=$PACKAGE/CodeMode/Model/ClientServer/$CONFIG_NAME/ -x outputDataDir=$PACKAGE/CodeMode/Hotfix/ClientServer/$CONFIG_NAME/
Write-Host "==================== 所有 完成 ===================="