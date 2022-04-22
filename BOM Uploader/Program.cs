using MySql.Data.MySqlClient; //official oracle
using System.IO;

// See https://aka.ms/new-console-template for more information

//connect to db
string connectionString = @"server=localhost;userid=quality;password=Qu4l1ty;database=quality_web";
MySqlConnection db = new MySqlConnection(connectionString);
db.Open();


//del prev
Console.Write("Nuke previous data? ");
var answ = Console.ReadKey().Key;
if(answ == ConsoleKey.Y || answ == ConsoleKey.I)
{
    Console.WriteLine("");
    Console.WriteLine("Deleting current data...");
    string[] tables = { "part_types", "bom", "timestamps", "products","operator" };
    foreach(var x in tables)
    {
        string truncate = "TRUNCATE `"+x+"`";
        MySqlCommand truncateCmd = new MySqlCommand(truncate, db);
        truncateCmd.ExecuteNonQuery();
    }
}
Console.WriteLine("");


//read string
string file = "input.csv";
string[] lines = File.ReadAllLines(file,System.Text.Encoding.UTF8);
string[] headers = lines[0].Split(',');

List<int> partIds = new List<int>();

//part types
for(int i = 2; i < headers.Length; ++i)
{
    //is the category already present?
    string headerQuery = "SELECT COUNT(*) FROM `part_types` WHERE name = '"+headers[i]+"'";
    MySqlCommand headerCommand = new MySqlCommand(headerQuery,db);
    int c = int.Parse(headerCommand.ExecuteScalar().ToString());
    if(c == 0)
    {
        string insertHeader = "INSERT INTO `part_types` VALUES(NULL,'"+headers[i]+"')";
        MySqlCommand headerInsert = new MySqlCommand(insertHeader,db);
        headerInsert.ExecuteNonQuery();
    }
    string headerId = "SELECT id FROM `part_types` WHERE name = '" + headers[i] + "'";
    MySqlCommand headerIdCmd = new MySqlCommand(headerId,db);
    int id = int.Parse(headerIdCmd.ExecuteScalar().ToString());
    partIds.Add(id);
}
Console.WriteLine("part types added");

//actual BOM
for(int i = 1; i < lines.Length; ++i)
{
    string[] line = lines[i].Split(',');
    string name = line[0];
    string refer = line[1];

    //does the product even exist?
    string prodQuery = "SELECT COUNT(*) FROM `products` WHERE name = '"+name+"' AND ref = '"+refer+"'";
    MySqlCommand prodCommand = new MySqlCommand(prodQuery,db);
    int c = int.Parse(prodCommand.ExecuteScalar().ToString());
    if(c == 0)
    {
        string inserProd = "INSERT INTO `products` VALUES(NULL,'" + name + "','" + refer + "')";
        MySqlCommand inserProdCmd = new MySqlCommand(inserProd,db);
        inserProdCmd.ExecuteNonQuery();
    }
    string prodId = "SELECT id FROM `products` WHERE name = '" + name + "' AND ref = '" + refer + "'";
    MySqlCommand prodIdCmd = new MySqlCommand(prodId,db);
    int id = int.Parse(prodIdCmd.ExecuteScalar().ToString());
    //got id, insert parts
    
    string insert = "INSERT INTO `bom` VALUES";
    for (int j = 2; j < line.Length; ++j)
    {
        if (!String.IsNullOrWhiteSpace(line[j])){
            insert+="(" + id + "," + partIds[j - 2] + ",'" + line[j] + "')";
            insert += j + 1 == line.Length ? "" : ",";
        }
    }
    MySqlCommand bomCmd = new MySqlCommand(insert, db);
    bomCmd.ExecuteNonQuery();
    Console.WriteLine("product: "+i+" added");

}

//operators
lines = File.ReadAllLines("operators.txt");
foreach(var x in lines)
{
    string insertOperatorSql = "INSERT INTO `operator` VALUES(NULL,'"+x+"')";
    MySqlCommand insertOperatorCmd = new MySqlCommand(insertOperatorSql,db);
    insertOperatorCmd.ExecuteNonQuery();
}
Console.WriteLine("operators added");

db.CloseAsync();
Console.WriteLine("end...");
Console.ReadKey();