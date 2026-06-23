$connectionString = "Data Source=HONORMAGICBOOK;Initial Catalog=AgroCompany;Integrated Security=True;TrustServerCertificate=True;Encrypt=True"

function U([int[]]$codes) { -join ($codes | ForEach-Object { [char]$_ }) }

$appleName = U @(0x042F, 0x0431, 0x043B, 0x043E, 0x043A, 0x043E)
$seasonAutumn = (U @(0x041E, 0x0441, 0x0435, 0x043D, 0x044C)) + '-2026'
$seasonSummer = (U @(0x041B, 0x0435, 0x0442, 0x043E)) + '-2026'

$apples = @(
    @{ Variety = U @(0x0414,0x0436,0x043E,0x043D,0x0430,0x0442,0x0430,0x043D); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x041B,0x043E,0x0431,0x043E); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0410,0x0439,0x0434,0x0430,0x0440,0x0435,0x0434); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0421,0x0442,0x0430,0x0440,0x043A,0x0440,0x0438,0x043C,0x0441,0x043E,0x043D); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0414,0x0436,0x043E,0x043D,0x0430,0x0433,0x043E,0x043B,0x0434); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0413,0x0430,0x043B,0x0430); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0424,0x0443,0x0434,0x0436,0x0438); Seasonality = $seasonAutumn },
    @{ Variety = (U @(0x0420,0x0435,0x0434) + ' ' + U @(0x0414,0x0435,0x043B,0x0438,0x0448,0x0435,0x0441)); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0416,0x0438,0x0433,0x0443,0x043B,0x0435,0x0432,0x0441,0x043A,0x043E,0x0435); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0418,0x043C,0x0440,0x0443,0x0441); Seasonality = $seasonAutumn },
    @{ Variety = (U @(0x0410,0x043B,0x043E,0x0435) + ' ' + U @(0x043D,0x0430) + ' ' + U @(0x0441,0x0435,0x0440,0x0435,0x0431,0x0440,0x0435)); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0421,0x0442,0x0430,0x0440,0x043A,0x0438,0x043D); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x041A,0x043E,0x0440,0x0442,0x043B,0x0430,0x043D,0x0434); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x041C,0x0435,0x043B,0x0431,0x0430); Seasonality = $seasonSummer },
    @{ Variety = (U @(0x0411,0x0435,0x043B,0x044B,0x0439) + ' ' + U @(0x043D,0x0430,0x043B,0x0438,0x0432)); Seasonality = $seasonSummer },
    @{ Variety = (U @(0x041B,0x0435,0x0442,0x043D,0x0435,0x0435) + ' ' + U @(0x043F,0x043E,0x043B,0x043E,0x0441,0x0430,0x0442,0x043E,0x0435)); Seasonality = $seasonSummer },
    @{ Variety = (U @(0x041E,0x0441,0x0435,0x043D,0x043D,0x0435,0x0435) + ' ' + U @(0x043F,0x043E,0x043B,0x043E,0x0441,0x0430,0x0442,0x043E,0x0435)); Seasonality = $seasonAutumn },
    @{ Variety = (U @(0x041F,0x0435,0x043F,0x0438,0x043D) + ' ' + U @(0x0448,0x0430,0x0444,0x0440,0x0430,0x043D,0x043D,0x044B,0x0439)); Seasonality = $seasonAutumn },
    @{ Variety = (U @(0x041C,0x043E,0x0441,0x043A,0x043E,0x0432,0x0441,0x043A,0x043E,0x0435) + ' ' + U @(0x0437,0x043E,0x043B,0x043E,0x0442,0x043E,0x0435)); Seasonality = $seasonAutumn },
    @{ Variety = (U @(0x041F,0x043E,0x0434,0x0430,0x0440,0x043E,0x043A) + ' ' + U @(0x041C,0x0438,0x0447,0x0443,0x0440,0x0438,0x043D,0x0430)); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0420,0x0430,0x043D,0x0435,0x0442,0x043A,0x0430); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0410,0x043D,0x0442,0x043E,0x043D,0x043E,0x0432,0x043A,0x0430); Seasonality = $seasonAutumn },
    @{ Variety = U @(0x0411,0x043E,0x0440,0x043E,0x0432,0x0438,0x043D,0x043A,0x0430); Seasonality = $seasonSummer },
    @{ Variety = (U @(0x041A,0x0438,0x0442,0x0430,0x0439,0x043A,0x0430) + ' ' + U @(0x0437,0x043E,0x043B,0x043E,0x0442,0x0438,0x0441,0x0442,0x0430,0x044F)); Seasonality = $seasonAutumn },
    @{ Variety = (U @(0x041F,0x0430,0x0440,0x043C,0x0435,0x043D) + ' ' + U @(0x0437,0x0438,0x043C,0x043D,0x0438,0x0439)); Seasonality = $seasonAutumn }
)

Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$tx = $conn.BeginTransaction()

try {
    $deleteStock = $conn.CreateCommand()
    $deleteStock.Transaction = $tx
    $deleteStock.CommandText = 'DELETE FROM ProductStock WHERE ProductID >= 5'
    $deletedStock = $deleteStock.ExecuteNonQuery()

    $deleteProducts = $conn.CreateCommand()
    $deleteProducts.Transaction = $tx
    $deleteProducts.CommandText = 'DELETE FROM Product WHERE ProductID >= 5'
    $deletedProducts = $deleteProducts.ExecuteNonQuery()

    $insertProduct = $conn.CreateCommand()
    $insertProduct.Transaction = $tx
    $insertProduct.CommandText = 'INSERT INTO Product (Name, Variety, Seasonality) OUTPUT INSERTED.ProductID VALUES (@Name, @Variety, @Seasonality)'
    [void]$insertProduct.Parameters.Add('@Name', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertProduct.Parameters.Add('@Variety', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertProduct.Parameters.Add('@Seasonality', [System.Data.SqlDbType]::NVarChar, 50)

    $insertStock = $conn.CreateCommand()
    $insertStock.Transaction = $tx
    $insertStock.CommandText = 'INSERT INTO ProductStock (ProductID, WarehouseID, Quantity, CheckDate) VALUES (@ProductID, @WarehouseID, @Quantity, CAST(GETDATE() AS DATE))'
    [void]$insertStock.Parameters.Add('@ProductID', [System.Data.SqlDbType]::Int)
    [void]$insertStock.Parameters.Add('@WarehouseID', [System.Data.SqlDbType]::Int)
    [void]$insertStock.Parameters.Add('@Quantity', [System.Data.SqlDbType]::Int)

    $inserted = 0
    foreach ($apple in $apples) {
        $insertProduct.Parameters['@Name'].Value = $appleName
        $insertProduct.Parameters['@Variety'].Value = $apple.Variety
        $insertProduct.Parameters['@Seasonality'].Value = $apple.Seasonality
        $productId = [int]$insertProduct.ExecuteScalar()

        $insertStock.Parameters['@ProductID'].Value = $productId
        $insertStock.Parameters['@WarehouseID'].Value = if ($productId % 2 -eq 0) { 1 } else { 2 }
        $insertStock.Parameters['@Quantity'].Value = 10 + ($productId % 41)
        [void]$insertStock.ExecuteNonQuery()
        $inserted++
    }

    $tx.Commit()
    Write-Host "Deleted stock rows: $deletedStock"
    Write-Host "Deleted product rows: $deletedProducts"
    Write-Host "Inserted apple rows: $inserted"
}
catch {
    $tx.Rollback()
    throw
}
finally {
    $conn.Close()
}
