using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.IO
{
    /// <summary>
    /// 文档管理工具
    /// </summary>
    public class DocumentAgent
    {
        public static MemoryStream CreateExcel<T>(string[] headerRow, IEnumerable<T> list, Func<T, object[]> convert)
        {
            MemoryStream stream = new MemoryStream();
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
            Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
            sheets.Append(sheet);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            Row header = new Row();
            foreach (string name in headerRow)
            {
                Cell cell = new Cell();
                cell.CellValue = new CellValue(name);
                cell.DataType = new EnumValue<CellValues>(CellValues.String);
                header.AppendChild(cell);
            }

            sheetData.Append(header);
            foreach (T item in list)
            {
                Row row = new Row();
                foreach (Object value in convert.Invoke(item))
                {
                    Cell dataCell = new Cell();
                    dataCell.CellValue = new CellValue(value.ToString());
                    dataCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    row.AppendChild(dataCell);
                }
                sheetData.Append(row);
            }
            workbookpart.Workbook.Save();
            spreadsheetDocument.Close();

            return stream;
        }
    }
}
