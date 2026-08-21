# Kiota.ps1
$OpenApiFile = ".\dishes.Server\dishes.Server.json"
$FrontendPath = ".\dishes.client\src\client\"

kiota generate -l typescript -d $OpenApiFile -c DishesClient -o $FrontendPath

# How to run 
# powershell -ExecutionPolicy Bypass -File .\kiota.ps1