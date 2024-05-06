using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using BlogDomain.Model;
using DocumentFormat.OpenXml.Spreadsheet;

namespace BlogInfrastructure.Services;

public class ArticlesExportService : IExportService<BlogDomain.Model.Category>
{
    private const string RootWorksheetName = "";

    private static readonly IReadOnlyList<string> HeaderNames =
        new string[]
        {
            "Назва",
            "Текст",
            "Дата",
            "Статус",
            "Категорія",
            "Автор",
        };
    
    private readonly LldbContext _context;

    private static void SetDefaultColumnWidth(IXLWorksheet worksheet)
    {
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 50;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 10;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 20;
    }
    
    private static void SetDefaultCellStyle(IXLWorksheet worksheet)
    {
        var defaultStyle = worksheet.Style;
        defaultStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        defaultStyle.Font.FontSize = 12;
    }
    
    private static void SetCellBorder(IXLCell cell)
    {
        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.TopBorderColor = XLColor.FromHtml("#7B8AB8");
        cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.LeftBorderColor = XLColor.FromHtml("#7B8AB8");
        cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.RightBorderColor = XLColor.FromHtml("#7B8AB8");
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#7B8AB8");
    }
    
    private static void WriteHeader(IXLWorksheet worksheet)
    {
        SetDefaultColumnWidth(worksheet);
        SetDefaultCellStyle(worksheet);
        for(int columnIndex = 0; columnIndex < HeaderNames.Count; columnIndex++)
        {
            var headerCell = worksheet.Cell(1, columnIndex + 1);
            headerCell.Value = HeaderNames[columnIndex];
            headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E3F1");
            SetCellBorder(headerCell);
        }
        worksheet.Row(1).Style.Font.Bold = true;
    }
    
    private void WriteArticle(IXLWorksheet worksheet, BlogDomain.Model.Article article, int rowIndex)
    {
        worksheet.Cell(rowIndex, 1).Value = article.Title;
        worksheet.Cell(rowIndex, 2).Value = article.Text;
        worksheet.Cell(rowIndex, 3).Value = article.Data;
        worksheet.Cell(rowIndex, 4).Value = article.Status;
        worksheet.Cell(rowIndex, 5).Value = article.Category.Name;
        
        Writer writer = _context.Writers.Include(c => c.Articles).FirstOrDefault(c => c.Id == article.WriterId) ?? throw new InvalidOperationException();
        article.Writer = writer;
        worksheet.Cell(rowIndex, 6).Value = article.Writer.Username;
        
        for (int columnIndex = 1; columnIndex <= HeaderNames.Count; columnIndex++)
        {
            var cell = worksheet.Cell(rowIndex, columnIndex);
            SetCellBorder(cell);
        }
    }

    private int rowIndex = 2;
    private void WriteArticles(IXLWorksheet worksheet, IList<BlogDomain.Model.Article> articles)
    {
        WriteHeader(worksheet);
        foreach (var article in articles)
        {
            WriteArticle(worksheet, article, rowIndex);
            rowIndex++;
        }
    }

    private void Write(XLWorkbook workbook, BlogDomain.Model.Category category)
    {
        var worksheet = workbook.Worksheets.Add("Експорт з сервісу");
        if(category is not null)
        {                    
            WriteArticles(worksheet, category.Articles.ToList());
        }
    }

    public ArticlesExportService(LldbContext context)
    {
        _context = context;
    }
    
    public async Task WriteToAsync(int clarifier, Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException("Input stream is not writable");
        }
        
        var categoryId = clarifier;
        var category = await _context.Categories
            .Include(category => category.Articles)
            .FirstOrDefaultAsync(category => category.Id == categoryId, cancellationToken);

        var workbook = new XLWorkbook();

        Write(workbook, category);
        workbook.SaveAs(stream);    
    }
}
