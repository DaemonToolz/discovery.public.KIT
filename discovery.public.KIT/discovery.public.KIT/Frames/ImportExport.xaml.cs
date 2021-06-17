using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using discovery.KIT.Events;
using discovery.KIT.Internal;
using discovery.KIT.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using Page = Windows.UI.Xaml.Controls.Page;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace discovery.KIT.Frames
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class ImportExport : Page, INotifyPropertyChanged
    {
        public ImportExport()
        {
            this.InitializeComponent();
        }

        private async void ExportJsonBtn_ClickAsync(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;
            var exportName = $"EXPORT_{DateTime.Now.ToString("yyyyMMddHHmmstt")}.json";
            await folder.CreateFileAsync(exportName, CreationCollisionOption.ReplaceExisting);
            try
            {
                var myFile = await folder.GetFileAsync(exportName);
                _ = FileIO.WriteTextAsync(myFile, JsonConvert.SerializeObject(ActiveConnectionHandler.ExistingData));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failure: " + ex.Message);

                return;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }

        private async void ExportExcelBtn_Click(object sender, RoutedEventArgs e)
        {

            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;
            var exportName = $"EXPORT_{DateTime.Now.ToString("yyyyMMddHHmmstt")}.xls";
            var file = await folder.CreateFileAsync(exportName, CreationCollisionOption.GenerateUniqueName);
            var futureAccessToken =  StorageApplicationPermissions.FutureAccessList.Add(file);
            var accessFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(futureAccessToken);
            var writeStream = await accessFile.OpenStreamForWriteAsync();

            try
            {
                var data = ActiveConnectionHandler.ExistingData;
                // Open the document for editing.
                using var spreadsheetDocument = SpreadsheetDocument.Create(writeStream, SpreadsheetDocumentType.Workbook);

                var wrkBookpart = spreadsheetDocument.AddWorkbookPart();
                var shareStringPart = wrkBookpart.AddNewPart<SharedStringTablePart>();

                // Insert a new worksheet
                var worksheetPart = InsertWorksheet(spreadsheetDocument.WorkbookPart, "UNNAMED");

               

                for (var i = 0; i < ActiveConnectionHandler.Headers?.Count ; i++) {               
                    var cell = InsertCellInWorksheet("A", (uint)(i + 1), worksheetPart);
                    cell.CellValue = new CellValue(ActiveConnectionHandler.Headers[i]);
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
     
                }

                var totalElements = 0;
                /*
                for (var i = 0; i < ActiveConnectionHandler.ExistingData.Count; i++)
                {
                    
                    for (var j = 0; j < ActiveConnectionHandler.ExistingData[i]; ++j)
                    {
                        var cell = InsertCellInWorksheet("B", (uint) (i + 1), worksheetPart);
                        cell.CellValue = new CellValue(ActiveConnectionHandler.Headers[i]);
                        cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    }
                    
                }
                */
                // Save the new worksheet.
                worksheetPart.Worksheet.Save();
                spreadsheetDocument.Close();
                writeStream.Close();

            }
            catch(Exception ex)
            {
                var _ = ex;
            }
        }


        #region OpenXML SDK Features
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            var i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (var item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        private static WorksheetPart InsertWorksheet(WorkbookPart workbookPart, string name)
        {
            // Add a new worksheet part to the workbook.
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            workbookPart.Workbook = new Workbook();
            var sheets = workbookPart.Workbook.Sheets = new Sheets();

            workbookPart.Workbook.Save();
            var relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

            var rnd = new Random();
            var sheet = new Sheet() { Id = relationshipId, SheetId = (uint)rnd.Next(int.MaxValue), Name = name };
            sheets.Append(sheet);
            workbookPart.Workbook.Save();

            return newWorksheetPart;
        }

        private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();
            var cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Count(r => r.RowIndex == rowIndex) != 0)
            {
                row = sheetData.Elements<Row>().First(r => r.RowIndex == rowIndex);
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Any(c => c.CellReference.Value == columnName + rowIndex))
            {
                return row.Elements<Cell>().First(c => c.CellReference.Value == cellReference);
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                var refCell = row.Elements<Cell>().Where(cell => cell.CellReference.Value.Length == cellReference.Length).FirstOrDefault(cell => string.Compare(cell.CellReference.Value, cellReference, StringComparison.OrdinalIgnoreCase) > 0);

                var newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }
        #endregion OpenXML SDK Features

        #region P2P Features

        private EventManager _manager = new EventManager();

        private void ExportP2P_Click(object sender, RoutedEventArgs e)
        {
            _manager.OnNavigationEvent(new NavigationEventArgs<object>()
            {
                NavigationEvent = NavigationEvent.P2P,
            });
        }
        #endregion P2P Features


    }
}
