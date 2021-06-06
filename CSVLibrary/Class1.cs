using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CSVLibrary
{
    public class Column
    {
        public string targetColumn { get; set; }
        public string targetDataType { get; set; }
        public bool newColumn { get; set; }
        public List<string> sourceColumns { get; set; }
        public string separator { get; set; }
        public string pattern {get; set;}
        public string defaultValue { get; set; }
    }

    public class Table
    {
        public List<Column> Columns { get; set; }
    }

    public class CSVConfig
    {
        public Table tableConfig;

        public CSVConfig(string filePath){
            string configText = File.ReadAllText(filePath);
            tableConfig = JsonSerializer.Deserialize<Table>(configText); 
        }
    }

    public class CSVCleaner
    {
        private string inputFilePath;
        private string outputFilePath;
        private CSVConfig config;

        public CSVCleaner(CSVConfig config, string inFilePath, string outFilePath){
            if (config == null) {

            }

            this.config = config;
            inputFilePath = inFilePath;
            outputFilePath = outFilePath;
        }

        public void Transform()
        {
            try
            {
                StartFileOutput();

                using (var reader = new StreamReader(inputFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    int count = 0;
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        count++;

                        var dict = new Dictionary<string, object> ();                        
                        foreach (Column col in config.tableConfig.Columns){
                            dict.Add(col.targetColumn, "");

                            for(int i = 0; i < col.sourceColumns.Count; i++) {
                                dict[col.targetColumn] += csv.GetField(col.sourceColumns[i]);
                                if (col.separator != null && (i != col.sourceColumns.Count-1)) {
                                    dict[col.targetColumn] += col.separator;
                                }
                            }

                            if (col.sourceColumns.Count == 0){
                                dict[col.targetColumn] = col.defaultValue;
                            }
                        }

                        if (IsRecordValid(dict, count)){
                            AppendFileOutput(dict);
                        }
                    }
                    

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        private void StartFileOutput() {
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (Column col in config.tableConfig.Columns){
                    csv.WriteField(col.targetColumn);
                }
            }
        }

        private void AppendFileOutput(Dictionary<string, object> record) {
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var stream = File.Open(outputFilePath, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.NextRecord();

                foreach (Column col in this.config.tableConfig.Columns){
                    csv.WriteField(record[col.targetColumn]);
                }
            }
        }

        private bool IsRecordValid(Dictionary<string, object> record, int recordNum){
            bool isValid = true;
            MatchCollection matches;

            foreach (Column col in this.config.tableConfig.Columns){

                switch(col.targetDataType) 
                {
                    case "string":
                        if (col.pattern != null){
                            Regex rgxStr = new Regex(col.pattern, RegexOptions.IgnoreCase);
                            matches = rgxStr.Matches(record[col.targetColumn].ToString());
                            if (matches.Count == 0)
                            {
                                isValid = false;
                                Console.WriteLine("Invalid format for " + col.targetColumn + ". Record Number: " + recordNum);
                            }
                        }
                        break;
                    case "integer":
                        int num;
                        if (!Int32.TryParse(record[col.targetColumn].ToString(), out num)){
                            isValid = false;
                            Console.WriteLine("Invalid integer for " + col.targetColumn + ". Record Number: " + recordNum);
                        }
                        break;
                    case "big decimal":
                        record[col.targetColumn] = record[col.targetColumn].ToString().Replace(",", "");
                        string pattern = "^[1-9]\\d*(\\.\\d+)?$";
                        Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                        matches = rgx.Matches(record[col.targetColumn].ToString());
                        if (matches.Count == 0)
                        {
                            isValid = false;
                            Console.WriteLine("Invalid big decimal for " + col.targetColumn + ". Record Number: " + recordNum);
                        }
                        break;
                    case "datetime":
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(record[col.targetColumn].ToString(), out dt)){
                            record[col.targetColumn] = dt.ToString();
                        }
                        else {
                            isValid = false;
                            Console.WriteLine(record[col.targetColumn].ToString());
                            Console.WriteLine("Invalid datetime for " + col.targetColumn + ". Record Number: " + recordNum);
                        }
                        break;
                }
            }

            return isValid;
        }
    }
}
