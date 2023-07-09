using System.Globalization;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NeerCore.Exceptions;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Repositories;

namespace VoicerStudio.Application.Infrastructure.Repositories;

internal sealed class SubtitleRepository : ISubtitleRepository
{
    private const string TableName = "Subtitle";

    private readonly GoogleSheetsAccessor _googleSheets;
    private readonly SpreadsheetsResource.ValuesResource _googleSheetValues;

    public SubtitleRepository(GoogleSheetsAccessor googleSheetsAccessor)
    {
        _googleSheets = googleSheetsAccessor;
        _googleSheetValues = googleSheetsAccessor.Service.Spreadsheets.Values;
    }


    public async Task<Subtitle> GetByIdAsync(Guid id, string sheet)
    {
        return await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            var all = await GetAllAsync(sheet);
            var speaker = all.FirstOrDefault(x => x.Id == id);
            if (speaker is null) throw new NotFoundException<Subtitle>();
            return speaker;
        });
    }

    public async Task<IEnumerable<Subtitle>> GetAllAsync(string sheet)
    {
        return await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            var request = _googleSheetValues.Get(sheet, $"{TableName}!A:F");
            var response = await request.ExecuteAsync();
            return MapFromValues(response.Values);
        });
    }

    public async Task UpdateAsync(IEnumerable<Subtitle> subs, string sheet)
    {
        await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            await RemoveAllAsync(sheet);
            var valueRange = new ValueRange { Values = subs.Select(MapToValuesRow).ToArray() };
            var updateRequest = _googleSheetValues.Update(valueRange, sheet, $"{TableName}!A2:F");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();
        });
    }

    public async Task RemoveAllAsync(string sheet)
    {
        await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            var requestBody = new ClearValuesRequest();
            var removeRequest = _googleSheetValues.Clear(requestBody, sheet, $"{TableName}!A2:F");
            await removeRequest.ExecuteAsync();
        });
    }


    private static IEnumerable<Subtitle> MapFromValues(IList<IList<object>> values)
    {
        var index = 1;
        foreach (var row in values.Skip(1))
        {
            index++;
            if (row.Count < 3
                || !Guid.TryParse(row[0].ToString(), out var id)
                || !int.TryParse(row[1].ToString(), out var speaker)
                || !TimeSpan.TryParse(row[2].ToString(), CultureInfo.InvariantCulture, out var start)
                || !TimeSpan.TryParse(row[3].ToString(), CultureInfo.InvariantCulture, out var end))
                continue;

            var sub = new Subtitle
            {
                Id = id,
                RowId = index,
                Speaker = speaker,
                Start = start,
                End = end,
                Text = row[4].ToString()!,
                Note = row[5].ToString()!,
            };
            yield return sub;
        }
    }

    private static IList<object> MapToValuesRow(Subtitle speaker)
    {
        var row = new object[]
        {
            speaker.Id,
            speaker.Speaker,
            speaker.Start,
            speaker.End,
            speaker.Text,
            speaker.Note ?? "",
        };
        return row;
    }
}